using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Git;

internal static class GitCommandExecutor
{
    public static async ValueTask<GitCommandResponse> ExecuteResultAsync(
        string gitPath,
        GitCommandOptions options,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        string contentType = GetMimeType(options);

        ProcessStartInfo info = new(fileName: gitPath, options.BuildCommand())
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using (Process? process = Process.Start(info))
        {
            if (process is null)
            {
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
                await using (StreamWriter writer = new(memoryStream, leaveOpen: true))
                {
                    string service = $"# service={options.Service}\n";
                    await writer.WriteAsync(
                        new StringBuilder($"{service.Length + 4:x4}{service}0000"),
                        cancellationToken: cancellationToken
                    );
                    await writer.FlushAsync(cancellationToken);
                }
            }

            await process.StandardOutput.BaseStream.CopyToAsync(
                destination: memoryStream,
                cancellationToken: cancellationToken
            );
            memoryStream.Seek(0, SeekOrigin.Begin);

            await process.WaitForExitAsync(cancellationToken);

            return new(memoryStream, contentType);
        }
    }

    private static string GetMimeType(in GitCommandOptions options)
    {
        string contentType = $"application/x-{options.Service}";

        return options.AdvertiseRefs ? contentType + "-advertisement" : contentType;
    }

    [SuppressMessage(
        category: "Microsoft.Reliability",
        checkId: "CA2000:DisposeObjectsBeforeLosingScope",
        Justification = "For Review"
    )]
    private static Stream GetInputStream(HttpContext context)
    {
        return StringComparer.Ordinal.Equals(context.Request.Headers["Content-Encoding"], y: "gzip")
            ? new GZipStream(stream: context.Request.Body, mode: CompressionMode.Decompress)
            : context.Request.Body;
    }
}
