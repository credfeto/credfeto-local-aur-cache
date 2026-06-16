using System;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Git.Exceptions;
using Credfeto.Aur.Mirror.Server.Middleware;
using FunFair.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.Aur.Mirror.Server.Tests.Middleware;

public sealed class UnhandledExceptionMiddlewareTests : TestBase
{
    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_ShouldNotModifyStatusCode()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected: StatusCodes.Status200OK, actual: context.Response.StatusCode);

        return;

        static Task NextAsync(HttpContext _) => Task.CompletedTask;
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsGitException_ShouldReturn503ServiceUnavailable()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected: StatusCodes.Status503ServiceUnavailable, actual: context.Response.StatusCode);

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new GitException("git failed"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsGitException_ShouldNotIncludeRetryAfterHeader()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.False(context.Response.Headers.ContainsKey("Retry-After"), userMessage: "Retry-After header should not be present for GitException responses");

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new GitException("git failed"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsGitException_ShouldSetContentTypeToJson()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected: "application/json", actual: context.Response.ContentType);

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new GitException("git failed"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsOtherException_ShouldReturn429TooManyRequests()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected: StatusCodes.Status429TooManyRequests, actual: context.Response.StatusCode);

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new InvalidOperationException("error"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsOtherException_ShouldIncludeRetryAfterHeader()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.ContainsKey("Retry-After"), userMessage: "Retry-After header should be present for non-GitException responses");

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new InvalidOperationException("error"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsOtherException_ShouldSetContentTypeToJson()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.Equal(expected: "application/json", actual: context.Response.ContentType);

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new InvalidOperationException("error"));
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsOperationCanceledException_ShouldPropagateException()
    {
        ILogger<UnhandledExceptionMiddleware> logger = this.GetTypedLogger<UnhandledExceptionMiddleware>();
        UnhandledExceptionMiddleware middleware = new(NextAsync, logger);
        DefaultHttpContext context = new();

        await Assert.ThrowsAsync<OperationCanceledException>(() => middleware.InvokeAsync(context));

        return;

        static Task NextAsync(HttpContext _) => Task.FromException(new OperationCanceledException());
    }
}
