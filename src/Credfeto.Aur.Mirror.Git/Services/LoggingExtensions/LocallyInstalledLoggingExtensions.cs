using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Git.Services.LoggingExtensions;

public static partial class LocallyInstalledLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Adding {repo} to clone cache for {timestamp}")]
    public static partial void AddingToCloneCache(this ILogger<LocallyInstalled> logger, string repo, DateTimeOffset timestamp);

    [LoggerMessage(LogLevel.Information, EventId = 2, Message = "Updating {repo} in clone cache to {timestamp}")]
    public static partial void UpdatingCloneCache(this ILogger<LocallyInstalled> logger, string repo, DateTimeOffset timestamp);
}