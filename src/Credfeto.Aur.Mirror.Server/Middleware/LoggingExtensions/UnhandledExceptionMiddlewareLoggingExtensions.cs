using System;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Middleware.LoggingExtensions;

internal static partial class UnhandledExceptionMiddlewareLoggingExtensions
{
    [LoggerMessage(LogLevel.Error, EventId = 1, Message = "Unhandled exception in request pipeline")]
    public static partial void UnhandledException(this ILogger logger, Exception exception);

    [LoggerMessage(LogLevel.Debug, EventId = 2, Message = "Client disconnected before error response could be sent")]
    public static partial void ClientDisconnectedDuringErrorResponse(this ILogger logger, Exception exception);
}
