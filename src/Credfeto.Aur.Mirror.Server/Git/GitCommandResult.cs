using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Git;

public static class GitCommandResult
{
    public static async ValueTask ExecuteResultAsync(string gitPath, GitCommandOptions options, HttpContext httpContext, CancellationToken cancellationToken)
    {
        HttpResponse response = httpContext.Response;
        Stream responseStream = httpContext.Response.Body;

        string contentType = $"application/x-{options.Service}";

        if (options.AdvertiseRefs)
        {
            contentType += "-advertisement";
        }

        response.ContentType = contentType;

        response.Headers.Append(key: "Expires", value: "Fri, 01 Jan 1980 00:00:00 GMT");
        response.Headers.Append(key: "Pragma", value: "no-cache");
        response.Headers.Append(key: "Cache-Control", value: "no-cache, max-age=0, must-revalidate");

        ProcessStartInfo info = new(fileName: gitPath, options.ToString())
                                {
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                };

        //info.Environment.Add("AUTH_USER", userName);
        //info.Environment.Add("REMOTE_USER", userName);
        //info.Environment.Add("GIT_COMMITTER_EMAIL", email);

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

            await using (StreamWriter writer = new(responseStream))
            {
                if (options.AdvertiseRefs)
                {
                    string service = $"# service={options.Service}\n";
                    await writer.WriteAsync(new StringBuilder($"{service.Length + 4:x4}{service}0000"), cancellationToken: cancellationToken);
                    await writer.FlushAsync(cancellationToken);
                }

                await process.StandardOutput.BaseStream.CopyToAsync(destination: responseStream, cancellationToken: cancellationToken);
            }

            await process.WaitForExitAsync(cancellationToken);
        }
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