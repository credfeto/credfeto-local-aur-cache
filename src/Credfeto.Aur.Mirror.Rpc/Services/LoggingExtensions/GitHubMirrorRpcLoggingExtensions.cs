using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;

internal static partial class GitHubMirrorRpcLoggingExtensions
{
    [LoggerMessage(
        LogLevel.Information,
        EventId = 1,
        Message = "Fetching package info from GitHub AUR mirror for: {packages}"
    )]
    private static partial void FetchingFromGitHubMirror(this ILogger<GitHubMirrorRpc> logger, string packages);

    public static void FetchingFromGitHubMirror(this ILogger<GitHubMirrorRpc> logger, IReadOnlyList<string> packages)
    {
        logger.FetchingFromGitHubMirror(string.Join(separator: ", ", values: packages));
    }

    [LoggerMessage(LogLevel.Warning, EventId = 2, Message = "GitHub AUR mirror: package {package} not found (404)")]
    public static partial void PackageNotFoundOnMirror(this ILogger<GitHubMirrorRpc> logger, string package);

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 3,
        Message = "GitHub AUR mirror: failed to parse .SRCINFO for package {package}"
    )]
    public static partial void FailedToParseSrcinfo(this ILogger<GitHubMirrorRpc> logger, string package);

    [LoggerMessage(
        LogLevel.Warning,
        EventId = 4,
        Message = "GitHub AUR mirror: HTTP error fetching {package}: {message}"
    )]
    public static partial void HttpErrorFetchingPackage(
        this ILogger<GitHubMirrorRpc> logger,
        string package,
        string message,
        Exception exception
    );
}
