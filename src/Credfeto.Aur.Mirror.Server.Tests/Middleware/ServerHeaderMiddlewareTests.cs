using System;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Middleware;
using FunFair.Test.Common;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Credfeto.Aur.Mirror.Server.Tests.Middleware;

public sealed class ServerHeaderMiddlewareTests : TestBase
{
    [Fact]
    public async Task InvokeAsync_ShouldSetXServerHeaderToMachineName()
    {
        ServerHeaderMiddleware middleware = new(NextAsync);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        string? header = context.Response.Headers["X-Server"];
        Assert.Equal(expected: Environment.MachineName, actual: header);

        return;

        static Task NextAsync(HttpContext _) => Task.CompletedTask;
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        bool nextCalled = false;
        Task NextAsync(HttpContext _)
        {
            nextCalled = true;

            return Task.CompletedTask;
        }

        ServerHeaderMiddleware middleware = new(NextAsync);
        DefaultHttpContext context = new();

        await middleware.InvokeAsync(context);

        Assert.True(condition: nextCalled, userMessage: "next delegate should have been called");
    }
}
