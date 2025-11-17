using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Models;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Credfeto.Date.Interfaces;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class AurRpc : IAurRpc
{
    private static readonly TimeSpan MaxAgeAccess = TimeSpan.FromHours(7);
    private static readonly TimeSpan MaxAgeRequest = TimeSpan.FromHours(14);
    private readonly ICurrentTimeSource _currentTimeSource;
    private readonly ILocalAurRpc _localAurRpc;
    private readonly ILogger<AurRpc> _logger;
    private readonly IRemoteAurRpc _remoteAurRpc;

    public AurRpc(IRemoteAurRpc remoteAurRpc, ILocalAurRpc localAurRpc, ICurrentTimeSource currentTimeSource, ILogger<AurRpc> logger)
    {
        this._remoteAurRpc = remoteAurRpc;
        this._localAurRpc = localAurRpc;
        this._currentTimeSource = currentTimeSource;
        this._logger = logger;

        // TASK: Look locally for everything and ONLY look in RPC if a significant amount of time has occured since the last query for that same data
    }

    public async ValueTask<RpcResponse> SearchAsync(string keyword, string by, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        this._logger.SearchingFor(keyword: keyword, by: by);

        try
        {
            RpcResponse upstream = await this._remoteAurRpc.SearchAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);

            await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToSearchUpstreamPackageInfo(keyword: keyword, by: by, message: exception.Message, exception: exception);

            IReadOnlyList<Package> results = await this._localAurRpc.SearchAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);

            return PackagesAsSearch(results);
        }
    }

    public async ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        this._logger.PackageInfo(packages);

        IReadOnlyList<Package>? localPackages = null;

        try
        {
            if (packages is [])
            {
                return RpcResults.InfoNotFound;
            }

            localPackages = await this._localAurRpc.InfoAsync(packages: packages, userAgent: userAgent, cancellationToken: cancellationToken);

            if (!this.NeedsUpstreamQuery(requestedPackages: packages, localPackages: localPackages))
            {
                this._logger.UsingLocalCache(packages);

                return PackagesAsInfo(localPackages);
            }

            RpcResponse upstream = await this._remoteAurRpc.InfoAsync(packages: packages, userAgent: userAgent, cancellationToken: cancellationToken);

            await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToGetUpstreamPackageInfo(packages: packages, message: exception.Message, exception: exception);

            return PackagesAsInfo(localPackages ?? []);
        }
    }

    private bool NeedsUpstreamQuery(IReadOnlyList<string> requestedPackages, IReadOnlyList<Package> localPackages)
    {
        ConditionContext context = new(RequestedPackages: requestedPackages, this._currentTimeSource.UtcNow());

        return localPackages.Any(package => this.NeedsUpstreamQuery(package: package, context: context));
    }

    private bool NeedsUpstreamQuery(Package package, in ConditionContext context)
    {
        return this.NotCachedLocally(package: package, context: context) || this.LastAccessedTooOld(package: package, context: context) || this.LastRequestedTooOld(package: package, context: context);
    }

    private bool NotCachedLocally(Package package, in ConditionContext context)
    {
        if (context.RequestedPackages.Contains(value: package.PackageName, comparer: StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        this._logger.NotCachedLocally(package.PackageName);

        return true;
    }

    private bool LastAccessedTooOld(Package package, in ConditionContext context)
    {
        TimeSpan lastAccessedAge = context.Now - package.LastAccessed;

        if (lastAccessedAge <= MaxAgeAccess)
        {
            return false;
        }

        this._logger.NotAccessedRecently(package: package.PackageName, lastAccessed: package.LastAccessed, age: lastAccessedAge, maxAge: MaxAgeAccess);

        return true;
    }

    private bool LastRequestedTooOld(Package package, in ConditionContext context)
    {
        TimeSpan lastRequestedAge = context.Now - package.LastRequestedUpstream;

        if (lastRequestedAge <= MaxAgeRequest)
        {
            return false;
        }

        this._logger.NotRequestedRecently(package: package.PackageName, lastRequested: package.LastRequestedUpstream, age: lastRequestedAge, maxAge: MaxAgeRequest);

        return true;
    }

    private static RpcResponse PackagesAsSearch(IReadOnlyList<Package> packages)
    {
        return new(count: packages.Count, [..packages.Select(item => item.SearchResult)], rpcType: "search", version: RpcResults.RpcVersion);
    }

    private static RpcResponse PackagesAsInfo(IReadOnlyList<Package> packages)
    {
        return new(count: packages.Count, [..packages.Select(item => item.SearchResult)], rpcType: "multiinfo", version: RpcResults.RpcVersion);
    }

    [DebuggerDisplay("{RequestedPackages} {Now}")]
    private readonly record struct ConditionContext(IReadOnlyList<string> RequestedPackages, DateTimeOffset Now);
}