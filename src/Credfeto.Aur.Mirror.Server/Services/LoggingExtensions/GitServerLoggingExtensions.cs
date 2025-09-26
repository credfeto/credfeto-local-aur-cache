using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;

internal static partial class GitServerLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Executing Git command: {arguments}")]
    public static partial void ExecutingCommand(this ILogger<GitServer> logger, string arguments);

    [LoggerMessage(LogLevel.Error, EventId = 2, Message = "Failed to start git: {exe} {arguments}")]
    public static partial void FailedToStartGit(this ILogger<GitServer> logger, string exe, string arguments);

    [LoggerMessage(LogLevel.Information, EventId = 2, Message = "Reading File: {repo}:: {path}")]
    public static partial void ReadingFile(this ILogger<GitServer> logger, string repo, string path);
}