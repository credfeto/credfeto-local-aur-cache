using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models.Git;
using Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class GitServer : IGitServer
{
    private const string GIT_PATH = "/usr/bin/git";
    private readonly ILogger<GitServer> _logger;

    private readonly IRepoConfig _repoConfig;

    public GitServer(IRepoConfig repoConfig, ILogger<GitServer> logger)
    {
        this._repoConfig = repoConfig;
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
}