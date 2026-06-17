using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

internal static partial class AurMetadataGzLoggingExtensions
{
    [LoggerMessage(
        LogLevel.Information,
        EventId = 1,
        Message = "Downloading packages-meta-ext-v1.json.gz from upstream"
    )]
    public static partial void DownloadingMetadataGz(this ILogger<AurMetadataGz> logger);

    [LoggerMessage(
        LogLevel.Information,
        EventId = 2,
        Message = "Downloaded packages-meta-ext-v1.json.gz: {byteCount} bytes, parsed {packageCount} packages"
    )]
    public static partial void DownloadedMetadataGz(
        this ILogger<AurMetadataGz> logger,
        int byteCount,
        int packageCount
    );

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 3,
        Message = "Failed to download packages-meta-ext-v1.json.gz: {message}"
    )]
    public static partial void FailedToDownloadMetadataGz(
        this ILogger<AurMetadataGz> logger,
        string message,
        Exception exception
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 4,
        Message = "Loaded packages-meta-ext-v1.json.gz from disk: {byteCount} bytes"
    )]
    public static partial void LoadedMetadataGzFromDisk(this ILogger<AurMetadataGz> logger, int byteCount);

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 5,
        Message = "Failed to parse packages-meta-ext-v1.json.gz content: {message}"
    )]
    public static partial void FailedToParseMetadataGz(
        this ILogger<AurMetadataGz> logger,
        string message,
        Exception exception
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 6,
        Message = "Triggering packages-meta-ext-v1.json.gz refresh: package LastModified {lastModified} is newer than cached at {cachedAt}"
    )]
    public static partial void TriggeringRefresh(
        this ILogger<AurMetadataGz> logger,
        DateTimeOffset lastModified,
        DateTimeOffset cachedAt
    );

    [LoggerMessage(
        LogLevel.Debug,
        EventId = 7,
        Message = "Skipping packages-meta-ext-v1.json.gz refresh: package LastModified {lastModified} is not newer than cached at {cachedAt}"
    )]
    public static partial void SkippingRefresh(
        this ILogger<AurMetadataGz> logger,
        DateTimeOffset lastModified,
        DateTimeOffset cachedAt
    );
}
