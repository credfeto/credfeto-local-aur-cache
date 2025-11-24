using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

internal static partial class RemoteAurRpcLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Requesting from upstream {uri}")]
    public static partial void RequestingUpstream(this ILogger<RemoteAurRpc> logger, Uri uri);

    [LoggerMessage(LogLevel.Information, EventId = 2, Message = "Requesting from upstream {uri} => {statusCode}")]
    public static partial void SuccessFromUpstream(
        this ILogger<RemoteAurRpc> logger,
        Uri uri,
        HttpStatusCode statusCode
    );

    [LoggerMessage(LogLevel.Information, EventId = 3, Message = "Retrieving package infos: {packages}")]
    private static partial void PackageInfo(this ILogger<RemoteAurRpc> logger, string packages);

    public static void PackageInfo(this ILogger<RemoteAurRpc> logger, IReadOnlyList<string> packages)
    {
        logger.PackageInfo(string.Join(separator: ", ", values: packages));
    }
}
