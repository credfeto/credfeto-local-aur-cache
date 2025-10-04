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

    [LoggerMessage(
        LogLevel.Error,
        EventId = 4,
        Message = "Failed to read saved metadata from folder: {directory}: {message}"
    )]
    public static partial void CouldNotFindMetadataFiles(
        this ILogger<AurRpc> logger,
        string directory,
        string message,
        Exception exception
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 5,
        Message = "Checking Package {packageId}) ({packageName}) metadata: {metadataFileName} from {upstreamRepo}"
    )]
    public static partial void CheckingPackage(
        this ILogger<AurRpc> logger,
        int packageId,
        string packageName,
        string metadataFileName,
        string upstreamRepo
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 6,
        Message = "Found Metadata for {packageId}) ({packageName}) in {metadataFileName}"
    )]
    public static partial void FoundMetadata(
        this ILogger<AurRpc> logger,
        int packageId,
        string packageName,
        string metadataFileName
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 7,
        Message = "No Metadata for {packageId}) ({packageName}) in {metadataFileName}"
    )]
    public static partial void NoMetadata(
        this ILogger<AurRpc> logger,
        int packageId,
        string packageName,
        string metadataFileName
    );
}
