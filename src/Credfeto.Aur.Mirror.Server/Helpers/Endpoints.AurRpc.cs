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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

        app.MapGet(
            pattern: "/rpc",
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

        return app;
    }

    private static async Task<IResult> ExecuteRpcAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        ILogger<IAurRpc> logger,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<string, string> query1 = httpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        bool multi = httpContext.Request.Query.ContainsKey("args[]");

        string queryText = GetPathWithQuery(httpContext.Request.Query);
        logger.Query(queryText);

        try
        {
            ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

            httpContext.Response.Headers.KeepAlive = "60";

            RpcResponse result = await aurRpc.GetAsync(
                query: query1,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            return Results.Ok(result);
        }
        catch (Exception exception)
        {
            logger.Failed(queryText, exception.Message, exception);

            return Results.Ok(new RpcResponse(count: 0, [], rpcType:
                                              multi ? "multiinfo" : "search", version: 5));
        }
    }

    private static string GetPathWithQuery(IQueryCollection query)
    {
        return query.Count == 0 ? string.Empty : string.Join(separator: '&', query.Select(q => $"{q.Key}={q.Value}"));
    }
}
