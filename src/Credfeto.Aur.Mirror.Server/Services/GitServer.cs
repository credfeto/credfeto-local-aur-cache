using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models.Git;
using Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;
using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class GitServer : IGitServer
{
    private const string GIT_PATH = "/usr/bin/git";
    private readonly ILogger<GitServer> _logger;

    private readonly IRepoConfig _repoConfig;
    private readonly IUpdateLock _updateLock;

    public GitServer(IRepoConfig repoConfig, IUpdateLock updateLock, ILogger<GitServer> logger)
    {
        this._repoConfig = repoConfig;
        this._updateLock = updateLock;
        this._logger = logger;
    }

    public async ValueTask<GitCommandResponse> ExecuteResultAsync(GitCommandOptions options, HttpContext httpContext, CancellationToken cancellationToken)
    {
        string repoBasePath = this._repoConfig.GetRepoBasePath(options.RepositoryName);

        string arguments = options.BuildCommand(repoBasePath);
        this._logger.ExecutingCommand(arguments);

        string contentType = GetMimeType(options);

        ProcessStartInfo info = new(fileName: GIT_PATH, arguments: arguments)
                                {
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    WorkingDirectory = repoBasePath
                                };

        using (Process? process = Process.Start(info))
        {
            if (process is null)
            {
                this._logger.FailedToStartGit(exe: GIT_PATH, arguments: arguments);

                throw new DataException("Git could not be started.");
            }

            await GetInputStream(httpContext)
                .CopyToAsync(destination: process.StandardInput.BaseStream, cancellationToken: cancellationToken);

            if (options.EndStreamWithNull)
            {
                await process.StandardInput.WriteAsync(new StringBuilder('\0'), cancellationToken: cancellationToken);
            }

            await process.StandardInput.DisposeAsync();

            MemoryStream memoryStream = new();

            if (options.AdvertiseRefs)
            {
                await using (StreamWriter writer = new(stream: memoryStream, leaveOpen: true))
                {
                    string service = $"# service={options.Service}\n";
                    await writer.WriteAsync(new StringBuilder($"{service.Length + 4:x4}{service}0000"), cancellationToken: cancellationToken);
                    await writer.FlushAsync(cancellationToken);
                }
            }

            await process.StandardOutput.BaseStream.CopyToAsync(destination: memoryStream, cancellationToken: cancellationToken);
            memoryStream.Seek(offset: 0, loc: SeekOrigin.Begin);

            await process.WaitForExitAsync(cancellationToken);

            return new(Content: memoryStream, ContentType: contentType);
        }
    }

    public async ValueTask EnsureRepositoryHasBeenClonedAsync(string repoName, string upstreamRepo, bool changed, CancellationToken cancellationToken)
    {
        string repoBasePath = this._repoConfig.GetRepoBasePath(repoName);

        SemaphoreSlim wait = await this._updateLock.GetLockAsync(fileName: repoBasePath, cancellationToken: cancellationToken);

        try
        {
            if (Directory.Exists(repoBasePath))
            {
                string? repoFolder = Repository.Discover(repoBasePath);

                if (repoFolder is null)
                {
                    CloneRepository(upstreamRepo: upstreamRepo, repoPath: repoBasePath);
                }
                else if (changed)
                {
                    UpdateRepository(repoFolder);
                }
            }
            else
            {
                CloneRepository(upstreamRepo: upstreamRepo, repoPath: repoBasePath);
            }
        }
        finally
        {
            wait.Release();
        }
    }

    public async ValueTask<GitCommandResponse> GetFileAsync(string repoName, string path, CancellationToken cancellationToken)
    {
        this._logger.ReadingFile(repo: repoName, path: path);
        string repoBasePath = this._repoConfig.GetRepoBasePath(repoName);

        string fileName = Path.Combine(path1: repoBasePath, path2: path);

        MemoryStream memoryStream = new();

        await using (Stream file = File.OpenRead(fileName))
        {
            await file.CopyToAsync(destination: memoryStream, cancellationToken: cancellationToken);
        }

        memoryStream.Seek(offset: 0, loc: SeekOrigin.Begin);

        return new(Content: memoryStream, ContentType: "application/octet-stream");
    }

    private static string GetMimeType(in GitCommandOptions options)
    {
        string contentType = $"application/x-{options.Service}";

        return options.AdvertiseRefs
            ? contentType + "-advertisement"
            : contentType;
    }

    [SuppressMessage(category: "Microsoft.Reliability", checkId: "CA2000:DisposeObjectsBeforeLosingScope", Justification = "For Review")]
    private static Stream GetInputStream(HttpContext context)
    {
        return StringComparer.Ordinal.Equals(context.Request.Headers["Content-Encoding"], y: "gzip")
            ? new GZipStream(stream: context.Request.Body, mode: CompressionMode.Decompress)
            : context.Request.Body;
    }

    private static void UpdateRepository(string repoFolder)
    {
        using (Repository repo = new(repoFolder))
        {
            FetchOptions options = new() { Prune = true, TagFetchMode = TagFetchMode.Auto };

            Remote? remote = repo.Network.Remotes["origin"];
            const string msg = "Fetching remote";
            IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repository: repo, remote: remote.Name, refspecs: refSpecs, options: options, logMessage: msg);
        }
    }

    private static void CloneRepository(string upstreamRepo, string repoPath)
    {
        Repository.Clone(sourceUrl: upstreamRepo, workdirPath: repoPath, new() { IsBare = true });
    }
}