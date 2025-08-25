using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;

internal static partial class AurRpcLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Requesting from upstream {uri}")]
    public static partial void RequestingUpstream(this ILogger<AurRpc> logger, Uri uri);

    [LoggerMessage(LogLevel.Information, EventId = 2, Message = "Requesting from upstream {uri} => {statusCode}")]
    public static partial void SuccessFromUpstream(this ILogger<AurRpc> logger, Uri uri, HttpStatusCode statusCode);

    [LoggerMessage(LogLevel.Error, EventId = 3, Message = "Failed to save metadata: {filename}: {message}")]
    public static partial void SaveMetadataFailed(
        this ILogger<AurRpc> logger,
        string filename,
        string message,
        Exception exception
    );

    [LoggerMessage(LogLevel.Error, EventId = 3, Message = "Failed to read saved metadata: {filename}: {message}")]
    public static partial void FailedToReadSavedMetadata(
        this ILogger<AurRpc> logger,
        string filename,
        string message,
        Exception exception
    );
}
