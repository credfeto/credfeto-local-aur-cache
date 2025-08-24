using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    private static WebApplication ConfigureTestEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Test/Ping Endpoint");

        app.MapGet(pattern: "/ping", handler: static () => Results.Ok(PingPong.Model));

        return app;
    }
}
