using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Credfeto.Aur.Mirror.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode("Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)")]
    private static WebApplication ConfigureAurRpcEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur RPC Endpoint");

        // https://wiki.archlinux.org/title/Aurweb_RPC_interface

        // note Yay is using the old interface
        // https://aur.archlinux.org/rpc/olddoc.html so ideally need to understand that and forward to the new interface

        RouteGroupBuilder group = app.MapGroup("/rpc");

        group.MapGet(pattern: "",
                     handler: static (HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                                  ExecuteLegacyRpcAsync(httpContext: httpContext, aurRpc: aurRpc, cancellationToken: cancellationToken));

        RouteGroupBuilder v5Group = group.MapGroup("v5");

        // https://aur.archlinux.org/rpc/v5/search/fetch?by=name
        v5Group.MapGet(pattern: "search/{keyword}",
                       handler: async static (string keyword, [FromQuery] string by, HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                                    await SearchUserAgentAsync(keyword: keyword, by: by, aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken));

        // Single
        // https://aur.archlinux.org/rpc/v5/info/afetch-git
        v5Group.MapGet(pattern: "info/{package}",
                       handler: async static (string package, HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                                {
                                    return await PackageInfoSingleAsync(package: package, aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken);
                                });

        // Multiple
        // 'https://aur.archlinux.org/rpc/v5/info?arg%5B%5D=afetch-git&arg%5B%5D=brave-bin' \
        v5Group.MapGet(pattern: "info",
                       handler: static ([FromQuery(Name = "arg[]")] string packages, HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                                    PackageInfoMultiAsync(packages: packages, aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken));

        return app;
    }

    private static async ValueTask<IResult> PackageInfoMultiAsync(string packages, IAurRpc aurRpc, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // Multi
        // curl -X 'GET' \
        // 'https://aur.archlinux.org/rpc/v5/info?arg%5B%5D=afetch-git&arg%5B%5D=brave-bin' \
        // -H 'accept: application/json'

        //?arg%5B%5D=pkg1&arg%5B%5D=pkg2&…

        // return types: multiInfo or error

        IReadOnlyList<string> splitPackages =
        [
            ..packages.Split(',')
                      .Distinct(StringComparer.OrdinalIgnoreCase)
        ];

        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        RpcResponse result = await aurRpc.InfoAsync(package: splitPackages, userAgent: userAgent, cancellationToken: cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> PackageInfoSingleAsync(string package, IAurRpc aurRpc, HttpContext httpContext, CancellationToken cancellationToken)
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        RpcResponse result = await aurRpc.InfoAsync([package], userAgent: userAgent, cancellationToken: cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> SearchUserAgentAsync(string keyword, string by, IAurRpc aurRpc, HttpContext httpContext, CancellationToken cancellationToken)
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        // name (search by package name only)
        // name-desc (search by package name and description)
        // maintainer (search by package maintainer)
        // depends (search for packages that depend on keywords)
        // makedepends (search for packages that makedepend on keywords)
        // optdepends (search for packages that optdepend on keywords)
        // checkdepends (search for packages that checkdepend on keywords)

        // return types: search or error

        RpcResponse result = await aurRpc.SearchAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);

        return Results.Ok(result);
    }

    private static async ValueTask<IResult> ExecuteLegacyRpcAsync(HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken)
    {
        IQueryCollection query = httpContext.Request.Query;

        if (!query.TryGetValue(key: "type", out StringValues queryType))
        {
            return Results.Ok(RpcResults.SearchNotFound);
        }

        if (queryType == "search")
        {
            string by = "name-desc";

            if (query.TryGetValue(key: "by", out StringValues byValue))
            {
                by = byValue.ToString();
            }

            if (query.TryGetValue(key: "arg", out StringValues keyword))
            {
                return await SearchUserAgentAsync(keyword.ToString(), by: by, aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken);
            }

            return Results.Ok(RpcResults.SearchNotFound);
        }

        if (queryType == "multiinfo")
        {
            if (query.TryGetValue(key: "arg", out StringValues package))
            {
                return await PackageInfoSingleAsync(package.ToString(), aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken);
            }

            if (query.TryGetValue(key: "arg[]", out StringValues packages))
            {
                return await PackageInfoMultiAsync(packages.ToString(), aurRpc: aurRpc, httpContext: httpContext, cancellationToken: cancellationToken);
            }

        }

        return Results.Ok(RpcResults.InfoNotFound);
    }

}