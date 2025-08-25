using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode(
        "Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)"
    )]
    private static WebApplication ConfigureAurRpcEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur RPC Endpoint");

        app.MapGet(
            pattern: "/rpc",
            handler: static (HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                ExecuteRpcAsync(httpContext: httpContext, aurRpc: aurRpc, cancellationToken: cancellationToken)
        );

        return app;
    }

    private static async Task<IResult> ExecuteRpcAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

            httpContext.Response.Headers.KeepAlive = "60";

            string query = GetPathWithQuery(httpContext.Request.Query);

            Debug.WriteLine(query);
            Debug.WriteLine(userAgent?.ToString());
            RpcResponse result = await aurRpc.GetAsync(
                query: query,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            return Results.Ok((object?)result);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception.Message);

            return Results.Ok(new RpcResponse(count: 0, [], rpcType: "search", version: 5));
        }
    }

    private static string GetPathWithQuery(IQueryCollection query)
    {
        return query.Count == 0 ? string.Empty : string.Join(separator: '&', query.Select(q => $"{q.Key}={q.Value}"));
    }
}
