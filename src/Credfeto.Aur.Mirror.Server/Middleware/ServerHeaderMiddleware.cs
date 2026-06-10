using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Middleware;

public sealed class ServerHeaderMiddleware
{
    private static readonly string MachineName = Environment.MachineName;
    private readonly RequestDelegate _next;

    public ServerHeaderMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append(key: "X-Server", value: MachineName);

        return this._next(context);
    }
}
