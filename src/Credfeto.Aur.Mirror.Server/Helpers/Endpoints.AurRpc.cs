using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Server.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    private static WebApplication ConfigureAurRpcEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur RPC Endpoint");

        // https://wiki.archlinux.org/title/Aurweb_RPC_interface

        // note Yay is using the old interface
        // note Paru is using the old interface
        // https://aur.archlinux.org/rpc/olddoc.html so ideally need to understand that and forward to the new interface

        RouteGroupBuilder group = app.MapGroup("/rpc");

        RegisterLegacyRpcGroup(group);

        RouteGroupBuilder v5Group = group.MapGroup("v5");

        RegisterV5RpcGroup(v5Group);

        return app;
    }

    private static void RegisterLegacyRpcGroup(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "",
            handler: static (HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                ExecuteLegacyRpcQueryAsync(
                    httpContext: httpContext,
                    aurRpc: aurRpc,
                    cancellationToken: cancellationToken
                )
        );

        group.MapPost(
            pattern: "",
            handler: static (HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
                ExecuteLegacyRpcPostAsync(
                    httpContext: httpContext,
                    aurRpc: aurRpc,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterV5RpcGroup(IEndpointRouteBuilder v5Group)
    {
        RegisterV5SearchEndpoint(v5Group);
        RegisterV5InfoSingleEndpoint(v5Group);
        RegisterV5InfoMultiGetEndpoint(v5Group);
        RegisterV5InfoMultiPostEndpoint(v5Group);
    }

    private static void RegisterV5SearchEndpoint(IEndpointRouteBuilder v5Group)
    {
        // https://aur.archlinux.org/rpc/v5/search/fetch?by=name
        v5Group.MapGet(
            pattern: "search/{keyword}",
            handler: static async (
                string keyword,
                [FromQuery] string by,
                HttpContext httpContext,
                IAurRpc aurRpc,
                CancellationToken cancellationToken
            ) =>
                await SearchUserAgentAsync(
                    keyword: keyword,
                    by: by,
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterV5InfoSingleEndpoint(IEndpointRouteBuilder v5Group)
    {
        // Single
        // https://aur.archlinux.org/rpc/v5/info/afetch-git
        v5Group.MapGet(
            pattern: "info/{package}",
            handler: static async (
                string package,
                HttpContext httpContext,
                IAurRpc aurRpc,
                CancellationToken cancellationToken
            ) =>
            {
                return await PackageInfoSingleAsync(
                    package: package,
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                );
            }
        );
    }

    private static void RegisterV5InfoMultiGetEndpoint(IEndpointRouteBuilder v5Group)
    {
        // Multiple
        // 'https://aur.archlinux.org/rpc/v5/info?arg%5B%5D=afetch-git&arg%5B%5D=brave-bin' \
        v5Group.MapGet(
            pattern: "info",
            handler: static (
                [FromQuery(Name = "arg[]")] string packages,
                HttpContext httpContext,
                IAurRpc aurRpc,
                CancellationToken cancellationToken
            ) =>
                PackageInfoMultiAsync(
                    packages: packages,
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterV5InfoMultiPostEndpoint(IEndpointRouteBuilder v5Group)
    {
        v5Group
            .MapPost(
                pattern: "info",
                handler: static (
                    [FromForm(Name = "arg[]")] string packages,
                    HttpContext httpContext,
                    IAurRpc aurRpc,
                    CancellationToken cancellationToken
                ) =>
                    PackageInfoMultiAsync(
                        packages: packages,
                        aurRpc: aurRpc,
                        httpContext: httpContext,
                        cancellationToken: cancellationToken
                    )
            )
            .DisableAntiforgery();
    }

    private static async ValueTask<IResult> PackageInfoMultiAsync(
        string packages,
        IAurRpc aurRpc,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        // Multi
        // curl -X 'GET' \
        // 'https://aur.archlinux.org/rpc/v5/info?arg%5B%5D=afetch-git&arg%5B%5D=brave-bin' \
        // -H 'accept: application/json'

        //?arg%5B%5D=pkg1&arg%5B%5D=pkg2&…

        // return types: multiInfo or error

        IReadOnlyList<string> splitPackages = [.. packages.Split(',').Distinct(StringComparer.OrdinalIgnoreCase)];

        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        RpcResponse result = await aurRpc.InfoAsync(
            packages: splitPackages,
            userAgent: userAgent,
            cancellationToken: cancellationToken
        );

        return Results.Ok(result);
    }

    private static async Task<IResult> PackageInfoSingleAsync(
        string package,
        IAurRpc aurRpc,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        RpcResponse result = await aurRpc.InfoAsync(
            [package],
            userAgent: userAgent,
            cancellationToken: cancellationToken
        );

        return Results.Ok(result);
    }

    private static async Task<IResult> SearchUserAgentAsync(
        string keyword,
        string by,
        IAurRpc aurRpc,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
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

        RpcResponse result = await aurRpc.SearchAsync(
            keyword: keyword,
            by: by,
            userAgent: userAgent,
            cancellationToken: cancellationToken
        );

        return Results.Ok(result);
    }

    private static ValueTask<IResult> ExecuteLegacyRpcQueryAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        in CancellationToken cancellationToken
    )
    {
        return ExecuteLegacyRpcQueryCommonAsync(
            httpContext: httpContext,
            aurRpc: aurRpc,
            query: ExtractQuery(httpContext),
            cancellationToken: cancellationToken
        );
    }

    private static async ValueTask<IResult> ExecuteLegacyRpcQueryCommonAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        IDictionary<string, StringValues> query,
        CancellationToken cancellationToken
    )
    {
        if (!query.TryGetValue(key: "type", out StringValues queryType))
        {
            return Results.Ok(RpcResults.SearchNotFound);
        }

        if (IsSearchQuery(queryType))
        {
            string by = "name-desc";

            if (query.TryGetValue(key: "by", out StringValues byValue))
            {
                by = byValue.ToString();
            }

            if (query.TryGetValue(key: "arg", out StringValues keyword))
            {
                return await SearchUserAgentAsync(
                    keyword.ToString(),
                    by: by,
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                );
            }

            return Results.Ok(RpcResults.SearchNotFound);
        }

        if (IsInfoQuery(queryType))
        {
            if (query.TryGetValue(key: "arg", out StringValues package))
            {
                return await PackageInfoSingleAsync(
                    package.ToString(),
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                );
            }

            if (query.TryGetValue(key: "arg[]", out StringValues packages))
            {
                return await PackageInfoMultiAsync(
                    packages.ToString(),
                    aurRpc: aurRpc,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                );
            }
        }

        return Results.Ok(RpcResults.InfoNotFound);
    }

    private static IDictionary<string, StringValues> ExtractQuery(HttpContext httpContext)
    {
        return httpContext
            .Request.Query.Keys.Select(key => (Key: key, Value: httpContext.Request.Query[key]))
            .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static IDictionary<string, StringValues> ExtractForm(HttpContext httpContext)
    {
        return httpContext
            .Request.Form.Keys.Select(key => (Key: key, Value: httpContext.Request.Form[key]))
            .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static ValueTask<IResult> ExecuteLegacyRpcPostAsync(
        HttpContext httpContext,
        IAurRpc aurRpc,
        in CancellationToken cancellationToken
    )
    {
        return ExecuteLegacyRpcQueryCommonAsync(
            httpContext: httpContext,
            aurRpc: aurRpc,
            query: ExtractForm(httpContext),
            cancellationToken: cancellationToken
        );
    }

    static bool IsSearchQuery(in StringValues queryType)
    {
        return IsMatch(queryType: queryType, match: "search");
    }

    static bool IsInfoQuery(in StringValues queryType)
    {
        return IsMatch(queryType: queryType, match: "info") || IsMatch(queryType: queryType, match: "multiinfo");
    }

    static bool IsMatch(in StringValues queryType, string match)
    {
        return queryType == match;
    }
}
