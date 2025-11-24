using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

internal static partial class AurRpcLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Searching for: {keyword} by {by}")]
    public static partial void SearchingFor(this ILogger<AurRpc> logger, string keyword, string by);

    [LoggerMessage(LogLevel.Information, EventId = 2, Message = "Retrieving package infos: {packages}")]
    private static partial void PackageInfo(this ILogger<AurRpc> logger, string packages);

    public static void PackageInfo(this ILogger<AurRpc> logger, IReadOnlyList<string> packages)
    {
        logger.PackageInfo(string.Join(separator: ", ", values: packages));
    }

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 3,
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

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 4,
        Message = "Failed Upstream search for: {keyword} by {by} => {message}"
    )]
    public static partial void FailedToSearchUpstreamPackageInfo(
        this ILogger<AurRpc> logger,
        string keyword,
        string by,
        string message,
        Exception exception
    );

    [LoggerMessage(LogLevel.Information, EventId = 5, Message = "Package {package} is not cached locally")]
    public static partial void NotCachedLocally(this ILogger<AurRpc> logger, string package);

    [LoggerMessage(
        LogLevel.Information,
        EventId = 6,
        Message = "Package {package} was not accessed recently (Accessed {lastAccessed} was {age} ago Max Age {maxAge})"
    )]
    public static partial void NotAccessedRecently(
        this ILogger<AurRpc> logger,
        string package,
        DateTimeOffset lastAccessed,
        TimeSpan age,
        TimeSpan maxAge
    );

    [LoggerMessage(
        LogLevel.Information,
        EventId = 7,
        Message = "Package {package} was not requested recently (Requested {lastRequested} was {age} ago Max Age {maxAge})"
    )]
    public static partial void NotRequestedRecently(
        this ILogger<AurRpc> logger,
        string package,
        DateTimeOffset lastRequested,
        TimeSpan age,
        TimeSpan maxAge
    );

    [LoggerMessage(LogLevel.Information, EventId = 8, Message = "Using local cache for package infos: {packages}")]
    private static partial void UsingLocalCache(this ILogger<AurRpc> logger, string packages);

    public static void UsingLocalCache(this ILogger<AurRpc> logger, IReadOnlyList<string> packages)
    {
        logger.UsingLocalCache(string.Join(separator: ", ", values: packages));
    }
}
