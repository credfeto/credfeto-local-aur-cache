using System;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Helpers.LoggingExtensions;

internal static partial class AurRpcLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, EventId = 1, Message = "Request query: {query}")]
    public static partial void Query(this ILogger<IAurRpc> logger, string query);

    [LoggerMessage(LogLevel.Error, EventId = 2, Message = "Failed query: {query}  => {message}")]
    public static partial void Failed(this ILogger<IAurRpc> logger, string query, string message, Exception exception);

}