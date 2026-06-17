using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class AurRpc : IAurRpc
{
    private static readonly TimeSpan MaxAgeAccess = TimeSpan.FromHours(7);
    private static readonly TimeSpan MaxAgeRequest = TimeSpan.FromHours(14);
    private readonly IAurMetadataGz _aurMetadataGz;
    private readonly ILocalAurRpc _localAurRpc;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AurRpc> _logger;
    private readonly IRemoteAurRpc _remoteAurRpc;

    public AurRpc(
        IRemoteAurRpc remoteAurRpc,
        ILocalAurRpc localAurRpc,
        IAurMetadataGz aurMetadataGz,
        TimeProvider timeProvider,
        ILogger<AurRpc> logger
    )
    {
        this._remoteAurRpc = remoteAurRpc;
        this._localAurRpc = localAurRpc;
        this._aurMetadataGz = aurMetadataGz;
        this._timeProvider = timeProvider;
        this._logger = logger;
    }

    public async ValueTask<RpcResponse> SearchAsync(
        string keyword,
        string by,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        this._logger.SearchingFor(keyword: keyword, by: by);

        try
        {
            RpcResponse upstream = await this._remoteAurRpc.SearchAsync(
                keyword: keyword,
                by: by,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            try
            {
                await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);
            }
            catch (Exception syncException)
            {
                this._logger.FailedToSyncUpstreamReposForSearch(
                    keyword: keyword,
                    by: by,
                    message: syncException.Message,
                    exception: syncException
                );
            }

            await this.TriggerMetadataGzRefreshIfNeededAsync(
                results: upstream.Results,
                cancellationToken: cancellationToken
            );

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToSearchUpstreamPackageInfo(
                keyword: keyword,
                by: by,
                message: exception.Message,
                exception: exception
            );

            IReadOnlyList<Package> localResults = await this._localAurRpc.SearchAsync(
                keyword: keyword,
                by: by,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            IReadOnlyList<SearchResult> gzResults = await this._aurMetadataGz.SearchAsync(
                keyword: keyword,
                by: by,
                cancellationToken: cancellationToken
            );

            return MergeSearchResults(localPackages: localResults, gzResults: gzResults);
        }
    }

    public async ValueTask<RpcResponse> InfoAsync(
        IReadOnlyList<string> packages,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        this._logger.PackageInfo(packages);

        IReadOnlyList<Package>? localPackages = null;

        try
        {
            if (packages is [])
            {
                return RpcResults.InfoNotFound;
            }

            localPackages = await this._localAurRpc.InfoAsync(
                packages: packages,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            if (!this.NeedsUpstreamQuery(requestedPackages: packages, localPackages: localPackages))
            {
                this._logger.UsingLocalCache(packages);

                return PackagesAsInfo(localPackages);
            }

            return await this.FetchUpstreamInfoAsync(
                packages: packages,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToGetUpstreamPackageInfo(
                packages: packages,
                message: exception.Message,
                exception: exception
            );

            return PackagesAsInfo(localPackages ?? []);
        }
    }

    private async ValueTask<RpcResponse> FetchUpstreamInfoAsync(
        IReadOnlyList<string> packages,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        RpcResponse upstream = await this._remoteAurRpc.InfoAsync(
            packages: packages,
            userAgent: userAgent,
            cancellationToken: cancellationToken
        );

        try
        {
            await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);
        }
        catch (Exception syncException)
        {
            this._logger.FailedToSyncUpstreamReposForInfo(
                packages: packages,
                message: syncException.Message,
                exception: syncException
            );
        }

        await this.TriggerMetadataGzRefreshIfNeededAsync(
            results: upstream.Results,
            cancellationToken: cancellationToken
        );

        return upstream;
    }

    private bool NeedsUpstreamQuery(IReadOnlyList<string> requestedPackages, IReadOnlyList<Package> localPackages)
    {
        if (localPackages is [])
        {
            return true;
        }

        ConditionContext context = new(RequestedPackages: requestedPackages, this._timeProvider.GetUtcNow());

        return localPackages.Any(package => this.NeedsUpstreamQuery(package: package, context: context));
    }

    private bool NeedsUpstreamQuery(Package package, in ConditionContext context)
    {
        return this.NotCachedLocally(package: package, context: context)
            || this.LastAccessedTooOld(package: package, context: context)
            || this.LastRequestedTooOld(package: package, context: context);
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

        this._logger.NotAccessedRecently(
            package: package.PackageName,
            lastAccessed: package.LastAccessed,
            age: lastAccessedAge,
            maxAge: MaxAgeAccess
        );

        return true;
    }

    private bool LastRequestedTooOld(Package package, in ConditionContext context)
    {
        TimeSpan lastRequestedAge = context.Now - package.LastRequestedUpstream;

        if (lastRequestedAge <= MaxAgeRequest)
        {
            return false;
        }

        this._logger.NotRequestedRecently(
            package: package.PackageName,
            lastRequested: package.LastRequestedUpstream,
            age: lastRequestedAge,
            maxAge: MaxAgeRequest
        );

        return true;
    }

    private async ValueTask TriggerMetadataGzRefreshIfNeededAsync(
        IReadOnlyList<SearchResult> results,
        CancellationToken cancellationToken
    )
    {
        if (results is [])
        {
            return;
        }

        long maxLastModified = results.Max(r => r.LastModified);

        await this._aurMetadataGz.TriggerRefreshIfNewerAsync(
            lastModifiedUnixTimestamp: maxLastModified,
            cancellationToken: cancellationToken
        );
    }

    private static RpcResponse MergeSearchResults(
        IReadOnlyList<Package> localPackages,
        IReadOnlyList<SearchResult> gzResults
    )
    {
        HashSet<string> localNames = new(localPackages.Select(p => p.PackageName), StringComparer.OrdinalIgnoreCase);
        IReadOnlyList<SearchResult> uniqueGzResults = [.. gzResults.Where(r => !localNames.Contains(r.Name))];

        SearchResult[] allResults = [.. localPackages.Select(p => p.SearchResult), .. uniqueGzResults];

        return new(count: allResults.Length, allResults, rpcType: "search", version: RpcResults.RpcVersion);
    }

    private static RpcResponse PackagesAsInfo(IReadOnlyList<Package> packages)
    {
        return new(
            count: packages.Count,
            [.. packages.Select(item => item.SearchResult)],
            rpcType: "multiinfo",
            version: RpcResults.RpcVersion
        );
    }

    [DebuggerDisplay("{RequestedPackages} {Now}")]
    private readonly record struct ConditionContext(IReadOnlyList<string> RequestedPackages, DateTimeOffset Now);
}
