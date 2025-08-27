using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Helpers.LoggingExtensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Credfeto.Aur.Mirror.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode(
        "Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)"
    )]
    private static WebApplication ConfigureAurRpcEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur RPC Endpoint");

        // https://wiki.archlinux.org/title/Aurweb_RPC_interface

        // note Yay is using the old interface
        // https://aur.archlinux.org/rpc/olddoc.html so ideally need to understand that and forward to the new interface

        RouteGroupBuilder group = app.MapGroup("/rpc");

        group.MapGet(
            pattern: "",
            handler: static (
                HttpContext httpContext,
                IAurRpc aurRpc,
                ILogger<IAurRpc> logger,
                CancellationToken cancellationToken
            ) =>
                ExecuteRpcAsync(
                    httpContext: httpContext,
                    aurRpc: aurRpc,
                    logger: logger,
                    cancellationToken: cancellationToken
                )
        );


        RouteGroupBuilder v5Group = group.MapGroup("v5");
        v5Group.MapGet("search/{keyword}", handler: static () =>
                                                       {
                                                           // name (search by package name only)
                                                           // name-desc (search by package name and description)
                                                           // maintainer (search by package maintainer)
                                                           // depends (search for packages that depend on keywords)
                                                           // makedepends (search for packages that makedepend on keywords)
                                                           // optdepends (search for packages that optdepend on keywords)
                                                           // checkdepends (search for packages that checkdepend on keywords)

                                                           // return types: search or error

                                                           return Results.Ok(RpcResults.SearchNotFound);
                                                       });

        group.MapGet("info/{keyword}", handler: static () =>
                                                     {
                                                         //?arg%5B%5D=pkg1&arg%5B%5D=pkg2&…

                                                         // return types: multiInfo or error

                                                         return Results.Ok(RpcResults.InfoNotFound);
                                                     });

        return app;
    }

    private static async Task<IResult> ExecuteRpcAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        ILogger<IAurRpc> logger,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<string, StringValues> query1 = httpContext.Request.Query.ToDictionary(
            x => x.Key,
            x => x.Value,
            StringComparer.OrdinalIgnoreCase
        );

        bool multi = httpContext.Request.Query.ContainsKey("args[]");

        string queryText = GetPathWithQuery(httpContext.Request.Query);
        logger.Query(queryText);

        try
        {
            ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

            httpContext.Response.Headers.KeepAlive = "60";

            RpcResponse result = await aurRpc.SearchAsync(
                query: query1,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            return Results.Ok(result);
        }
        catch (Exception exception)
        {
            logger.Failed(queryText, exception.Message, exception);

            string type = multi
                ? "multiinfo"
                : "search";

            return Results.Ok(new RpcResponse(count: 0, [], rpcType: type, version: 5));
        }
    }

    private static string GetPathWithQuery(IQueryCollection query)
    {
        return query.Count == 0 ? string.Empty : string.Join(separator: '&', query.Select(q => $"{q.Key}={q.Value}"));
    }
}
