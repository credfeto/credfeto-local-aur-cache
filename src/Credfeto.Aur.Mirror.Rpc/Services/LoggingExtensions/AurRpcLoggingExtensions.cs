using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

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





        [LoggerMessage(LogLevel.Information, EventId = 8, Message = "Request query: {query}")]
    public static partial void Query(this ILogger<AurRpc> logger, string query);

    [LoggerMessage(LogLevel.Error, EventId = 9, Message = "Failed query: {query}  => {message}")]
    public static partial void Failed(this ILogger<AurRpc> logger, string query, string message, Exception exception);

    [LoggerMessage(LogLevel.Information, EventId = 10, Message = "Searching for: {keyword} by {by}")]
    public static partial void SearchingFor(this ILogger<AurRpc> logger, string keyword, string by);

    [LoggerMessage(LogLevel.Information, EventId = 11, Message = "Retrieving package infos: {packages}")]
    private static partial void PackageInfo(this ILogger<AurRpc> logger, string packages);

    public static void PackageInfo(this ILogger<AurRpc> logger, IReadOnlyList<string> packages)
    {
        logger.PackageInfo(string.Join(separator: ", ", values: packages));
    }

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 12,
        Message = "Failed to retrieve package infos: {packages} => {message}"
    )]
    private static partial void FailedToGetUpstreamPackageInfo(
        this ILogger<AurRpc> logger,
        string packages,
        string message,
        Exception exception
    );

    public static void FailedToGetUpstreamPackageInfo(
        this ILogger<AurRpc> logger,
        IReadOnlyList<string> packages,
        string message,
        Exception exception
    )
    {
        logger.FailedToGetUpstreamPackageInfo(
            string.Join(separator: ", ", values: packages),
            message: message,
            exception: exception
        );
    }

    [LoggerMessage(LogLevel.Information, EventId = 13, Message = "Search Found: {package} using {keyword} by {by}")]
    public static partial void OfflineSearchFound(
        this ILogger<AurRpc> logger,
        string package,
        string keyword,
        string by
    );

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 14,
        Message = "Failed Upstream search for: {keyword} by {by} => {message}"
    )]
    public static partial void FailedToSearchUpstreamPackageInfo(
        this ILogger<AurRpc> logger,
        string keyword,
        string by,
        string message,
        Exception exception
    );

}
