using System;
using System.Collections.Generic;
using System.Threading;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    private static WebApplication ConfigureCacheEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Cache Endpoint");

        app.MapGet(
            pattern: "/cache",
            handler: static async (ILocallyInstalled locallyInstalled, CancellationToken cancellationToken) =>
            {
                IReadOnlyList<RepoCloneInfo> cached = await locallyInstalled.GetRecentlyClonedAsync(cancellationToken);

                return Results.Ok(cached);
            }
        );

        return app;
    }
}
