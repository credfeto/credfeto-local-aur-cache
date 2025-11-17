using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

internal static partial class LocalAurMetadataLoggingExtensions
{
    [LoggerMessage(LogLevel.Error, EventId = 1, Message = "Failed to save metadata: {filename}: {message}")]
    public static partial void SaveMetadataFailed(this ILogger<LocalAurMetadata> logger, string filename, string message, Exception exception);

    [LoggerMessage(LogLevel.Error, EventId = 2, Message = "Failed to read saved metadata: {filename}: {message}")]
    public static partial void FailedToReadSavedMetadata(this ILogger<LocalAurMetadata> logger, string filename, string message, Exception exception);

    [LoggerMessage(LogLevel.Error, EventId = 3, Message = "Failed to read saved metadata from folder: {directory}: {message}")]
    public static partial void CouldNotFindMetadataFiles(this ILogger<LocalAurMetadata> logger, string directory, string message, Exception exception);

    [LoggerMessage(LogLevel.Information, EventId = 4, Message = "Checking Package {packageId} ({packageName}) metadata: {metadataFileName} from {upstreamRepo}")]
    public static partial void CheckingPackage(this ILogger<LocalAurMetadata> logger, int packageId, string packageName, string metadataFileName, string upstreamRepo);

    [LoggerMessage(LogLevel.Information, EventId = 5, Message = "Found Metadata for {packageId} ({packageName}) in {metadataFileName}")]
    public static partial void FoundMetadata(this ILogger<LocalAurMetadata> logger, int packageId, string packageName, string metadataFileName);

    [LoggerMessage(LogLevel.Information, EventId = 6, Message = "No Metadata for {packageId} ({packageName}) in {metadataFileName}")]
    public static partial void NoMetadata(this ILogger<LocalAurMetadata> logger, int packageId, string packageName, string metadataFileName);

    [LoggerMessage(LogLevel.Information, EventId = 7, Message = "Search Found: {package} using {keyword} by {by}")]
    public static partial void OfflineSearchFound(this ILogger<LocalAurMetadata> logger, string package, string keyword, string by);

    [LoggerMessage(LogLevel.Information, EventId = 8, Message = "Loaded {packageId} ({packageName}) from {metadataFileName}")]
    public static partial void LoadedPackageFromCache(this ILogger<LocalAurMetadata> logger, int packageId, string packageName, string metadataFileName);

    [LoggerMessage(LogLevel.Information, EventId = 9, Message = "Saved {packageId} ({packageName}) to {metadataFileName}")]
    public static partial void SavedPackageToCache(this ILogger<LocalAurMetadata> logger, int packageId, string packageName, string metadataFileName);

}