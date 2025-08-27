using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode("Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)")]
    private static WebApplication ConfigureAurRepoEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur Repo Endpoint");

        RouteGroupBuilder group = app.MapGroup("/repos");

        group.MapGet(pattern: "packages.gz",
                     handler: static (IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GetPackagesAsync(httpContext: httpContext, aurRepos: aurRepos, cancellationToken: cancellationToken));

        return app;
    }

    private static async ValueTask<IResult> GetPackagesAsync(HttpContext httpContext, IAurRepos aurRepos, CancellationToken cancellationToken)
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        byte[]? result = await aurRepos.GetPackagesAsync(userAgent: userAgent, cancellationToken: cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        return Results.File(fileContents: result, contentType: "application/gzip");
    }
}