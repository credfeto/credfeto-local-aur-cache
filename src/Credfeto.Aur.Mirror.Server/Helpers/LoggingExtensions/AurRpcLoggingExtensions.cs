using System;
using System.Collections.Generic;
using Credfeto.Aur.Mirror.Interfaces;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Helpers.LoggingExtensions;

internal static partial class AurRpcLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Request query: {query}")]
    public static partial void Query(this ILogger<IAurRpc> logger, string query);

    [LoggerMessage(LogLevel.Error, EventId = 2, Message = "Failed query: {query}  => {message}")]
    public static partial void Failed(this ILogger<IAurRpc> logger, string query, string message, Exception exception);

    [LoggerMessage(LogLevel.Information, EventId = 3, Message = "Searching for: {keyword} by {by}")]
    public static partial void SearchingFor(this ILogger<IAurRpc> logger, string keyword, string by);

    [LoggerMessage(LogLevel.Information, EventId = 4, Message = "Retrieving package infos: {packages}")]
    private static partial void PackageInfo(this ILogger<IAurRpc> logger, string packages);

    public static void PackageInfo(this ILogger<IAurRpc> logger, IReadOnlyList<string> packages)
    {
        logger.PackageInfo(string.Join(separator: ", ", values: packages));
    }

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 5,
        Message = "Failed to retrieve package infos: {packages} => {message}"
    )]
    private static partial void FailedToGetUpstreamPackageInfo(
        this ILogger<IAurRpc> logger,
        string packages,
        string message,
        Exception exception
    );

    public static void FailedToGetUpstreamPackageInfo(
        this ILogger<IAurRpc> logger,
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

    [LoggerMessage(LogLevel.Information, EventId = 6, Message = "Search Found: {package} using {keyword} by {by}")]
    public static partial void OfflineSearchFound(
        this ILogger<IAurRpc> logger,
        string package,
        string keyword,
        string by
    );

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 7,
        Message = "Failed Upstream search for: {keyword} by {by} => {message}"
    )]
    public static partial void FailedToSearchUpstreamPackageInfo(
        this ILogger<IAurRpc> logger,
        string keyword,
        string by,
        string message,
        Exception exception
    );
}
