using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.Git;
using Credfeto.Aur.Mirror.Server.Exceptions;
using Credfeto.Aur.Mirror.Server.Helpers;
using Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;
using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class GitServer : IGitServer
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    private readonly ILocallyInstalled _locallyInstalled;
    private readonly ILogger<GitServer> _logger;

    private readonly IRepoConfig _repoConfig;
    private readonly IUpdateLock _updateLock;

    public GitServer(IRepoConfig repoConfig, IUpdateLock updateLock, ILocallyInstalled locallyInstalled, ILogger<GitServer> logger)
    {
        this._repoConfig = repoConfig;
        this._updateLock = updateLock;
        this._locallyInstalled = locallyInstalled;
        this._logger = logger;
    }

    public async ValueTask<GitCommandResponse> ExecuteResultAsync(GitCommandOptions options, Stream source, CancellationToken cancellationToken)
    {
        string repoBasePath = this._repoConfig.GetRepoBasePath(options.RepositoryName);

        string arguments = options.BuildCommand(repoBasePath);
        this._logger.ExecutingCommand(arguments);

        using (Process? process = Process.Start(new ProcessStartInfo(fileName: this._repoConfig.GitExecutable, arguments: arguments)
                                                {
                                                    UseShellExecute = false,
                                                    CreateNoWindow = true,
                                                    RedirectStandardInput = true,
                                                    RedirectStandardOutput = true,
                                                    RedirectStandardError = true,
                                                    WorkingDirectory = repoBasePath
                                                }))
        {
            if (process is null)
            {
                this._logger.FailedToStartGit(exe: this._repoConfig.GitExecutable, arguments: arguments);

                throw new DataException("Git could not be started.");
            }

            await source
                .CopyToAsync(destination: process.StandardInput.BaseStream, cancellationToken: cancellationToken);

            if (options.EndStreamWithNull)
            {
                await process.StandardInput.WriteAsync(new StringBuilder('\0'), cancellationToken: cancellationToken);
            }

            await process.StandardInput.DisposeAsync();

            await using (RecyclableMemoryStream memoryStream = MemoryStreamManager.GetStream())
            {
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

                await process.WaitForExitAsync(cancellationToken);

                await this._locallyInstalled.MarkAsClonedAsync(repo: options.RepositoryName, cancellationToken: cancellationToken);

                return new(memoryStream.ToArray(), ContentType: options.ContentType);
            }
        }
    }

    public async ValueTask EnsureRepositoryHasBeenClonedAsync(string repoName, string upstreamRepo, bool changed, CancellationToken cancellationToken)
    {
        string repoBasePath = this._repoConfig.GetRepoBasePath(repoName);

        this._logger.RequestingCloneOrUpdateOfRepo(repo: repoName, upstream: upstreamRepo, path: repoBasePath);
        SemaphoreSlim wait = await this._updateLock.GetLockAsync(fileName: repoBasePath, cancellationToken: cancellationToken);

        try
        {
            if (Directory.Exists(repoBasePath))
            {
                string? repoFolder = Repository.Discover(repoBasePath);

                if (repoFolder is null)
                {
                    await this.CloneRepositoryAsync(upstreamRepo: upstreamRepo, repoPath: repoBasePath, cancellationToken: cancellationToken);
                }
                else if (changed)
                {
                    await this.UpdateRepositoryAsync(upstreamRepo: upstreamRepo, repoPath: repoFolder, cancellationToken: cancellationToken);
                }
            }
            else
            {
                await this.CloneRepositoryAsync(upstreamRepo: upstreamRepo, repoPath: repoBasePath, cancellationToken: cancellationToken);
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

        await using (RecyclableMemoryStream memoryStream = MemoryStreamManager.GetStream())
        {
            await using (Stream file = File.OpenRead(fileName))
            {
                await file.CopyToAsync(destination: memoryStream, cancellationToken: cancellationToken);
            }

            return new(memoryStream.ToArray(), ContentType: "application/octet-stream");
        }
    }



    private async ValueTask CloneRepositoryAsync(string upstreamRepo, string repoPath, CancellationToken cancellationToken)
    {
        (string[] output, int exitCode) = await GitCommandLine.ExecAsync(gitExecutable: this._repoConfig.GitExecutable,
                                                                         clonePath: upstreamRepo,
                                                                         repoPath: repoPath,
                                                                         $"clone --mirror {upstreamRepo} {repoPath}",
                                                                         cancellationToken: cancellationToken);

        if (exitCode != 0)
        {
            string message = string.Join(separator: Environment.NewLine, value: output);
            this._logger.FailedToCloneGit(upstream: upstreamRepo, path: repoPath, message: message);

            throw new GitException(message);
        }
    }

    private async ValueTask UpdateRepositoryAsync(string upstreamRepo, string repoPath, CancellationToken cancellationToken)
    {
        (string[] output, int exitCode) = await GitCommandLine.ExecAsync(gitExecutable: this._repoConfig.GitExecutable,
                                                                         clonePath: upstreamRepo,
                                                                         repoPath: repoPath,
                                                                         $"-C \"{repoPath}\" fetch",
                                                                         cancellationToken: cancellationToken);

        if (exitCode != 0)
        {
            string message = string.Join(separator: Environment.NewLine, value: output);
            this._logger.FailedToCloneGit(upstream: upstreamRepo, path: repoPath, message: message);

            throw new GitException(message);
        }
    }
}