using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Git.Services.LoggingExtensions;

internal static partial class GitServerLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Executing Git command: {arguments}")]
    public static partial void ExecutingCommand(this ILogger<GitServer> logger, string arguments);

    [LoggerMessage(LogLevel.Error, EventId = 2, Message = "Failed to start git: {exe} {arguments}")]
    public static partial void FailedToStartGit(this ILogger<GitServer> logger, string exe, string arguments);

    [LoggerMessage(LogLevel.Information, EventId = 3, Message = "Reading File: {repo} -> {path}")]
    public static partial void ReadingFile(this ILogger<GitServer> logger, string repo, string path);

    [LoggerMessage(LogLevel.Information, EventId = 4, Message = "Requesting clone/update of {repo} from {upstream} into {path}")]
    public static partial void RequestingCloneOrUpdateOfRepo(this ILogger<GitServer> logger, string repo, string upstream, string path);

    [LoggerMessage(LogLevel.Information, EventId = 5, Message = "Failed to clone {upstream} into {path}: {message}")]
    public static partial void FailedToCloneGit(this ILogger<GitServer> logger, string upstream, string path, string message);

    [LoggerMessage(LogLevel.Information, EventId = 6, Message = "Failed to update {upstream} in {path}: {message}")]
    public static partial void FailedToUpdateGit(this ILogger<GitServer> logger, string upstream, string path, string message);

    [LoggerMessage(LogLevel.Information, EventId = 7, Message = "{repo}: Found at {path}")]
    public static partial void RepoExists(this ILogger<GitServer> logger, string repo, string path);

    [LoggerMessage(LogLevel.Information, EventId = 8, Message = "{repo}: Not found at {path}")]
    public static partial void RepoMissing(this ILogger<GitServer> logger, string repo, string path);

    [LoggerMessage(LogLevel.Information, EventId = 9, Message = "{repo}: Failed to open {path}: {message}")]
    public static partial void RepoInvalid(this ILogger<GitServer> logger, string repo, string path, string message, Exception exception);
}