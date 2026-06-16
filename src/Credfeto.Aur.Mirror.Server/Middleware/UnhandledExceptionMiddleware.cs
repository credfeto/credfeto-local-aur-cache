using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Git.Exceptions;
using Credfeto.Aur.Mirror.Server.Middleware.LoggingExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Server.Middleware;

public sealed class UnhandledExceptionMiddleware
{
    private const int RetryAfterSeconds = 30;
    private static readonly ErrorDto ErrorResponse = new("Service temporarily unavailable. Please retry later.");
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;

    public UnhandledExceptionMiddleware(RequestDelegate next, ILogger<UnhandledExceptionMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this._next(context);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();

            if (exception is GitException)
            {
                this._logger.GitServerUnavailable(exception);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            }
            else
            {
                this._logger.UnhandledException(exception);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append(
                    key: "Retry-After",
                    value: RetryAfterSeconds.ToString(CultureInfo.InvariantCulture)
                );
            }

            context.Response.ContentType = "application/json";

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(
                value: ErrorResponse,
                jsonTypeInfo: AppJsonContext.Default.ErrorDto
            );

            try
            {
                await context.Response.Body.WriteAsync(buffer: body, cancellationToken: context.RequestAborted);
            }
            catch (Exception writeException)
                when (writeException is OperationCanceledException or System.IO.IOException)
            {
                this._logger.ClientDisconnectedDuringErrorResponse(writeException);
            }
        }
    }
}
