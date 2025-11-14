using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Credfeto.Extensions.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class LocalAurRpc : ILocalAurRpc
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly IGitServer _gitServer;
    private readonly ILocalAurMetadata _localAurMetadata;
    private readonly ILogger<LocalAurRpc> _logger;
    private readonly ServerConfig _serverConfig;


    public LocalAurRpc(IOptions<ServerConfig> config, IGitServer gitServer,  ILocalAurMetadata localAurMetadata, ILogger<LocalAurRpc> logger)
    {
        this._gitServer = gitServer;
        this._localAurMetadata = localAurMetadata;
        this._logger = logger;
        this._gitServer = gitServer;
        this._logger = logger;
        this._serverConfig = config.Value;

        // TASK: Store local config in a DB that's quick to search rather than filesystem
        // TASK: Look locally for everything and ONLY look in RPC if a significant amount of time has occured since the last query for that same data
    }

    public async ValueTask<RpcResponse> SearchAsync(string keyword, string by, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchResult> results =
            await this._localAurMetadata.SearchAsync(predicate: item => IsSearchMatch(existing: item, keyword: keyword, by: by), cancellationToken: cancellationToken);

        return new(count: results.Count, results: results, rpcType: "search", version: RpcResults.RpcVersion);
    }

    public ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchResult> results =
        [
            .. packages.Select(this._localAurMetadata.Get)
                       .RemoveNulls()
        ];

        RpcResponse response = new(count: results.Count, results: results, rpcType: "multiinfo", version: RpcResults.RpcVersion);

        return ValueTask.FromResult(response);
    }

    public ValueTask SyncUpstreamReposAsync(RpcResponse upstream, ProductInfoHeaderValue? userAgent)
    {
        return this.SyncUpstreamReposAsync(upstream.Results);
    }

    private async ValueTask SyncUpstreamReposAsync(IReadOnlyList<SearchResult> items)
    {
        foreach (SearchResult package in items)
        {
            await this._localAurMetadata.UpdateAsync(package, onUpdate: this.OnRepoChangedAsync, CancellationToken.None);
        }
    }

    private ValueTask OnRepoChangedAsync(SearchResult package, bool changed)
    {
        string upstreamRepo = this._serverConfig.Upstream.Repos + "/" + package.Name + ".git";

        this._logger.CheckingPackage(packageId: package.Id, packageName: package.Name, upstreamRepo: upstreamRepo);

        return this._gitServer.EnsureRepositoryHasBeenClonedAsync(
            repoName: package.Name,
            upstreamRepo: upstreamRepo,
            changed: changed,
            cancellationToken: DoNotCancelEarly);
    }

    private static bool IsSearchMatch(SearchResult existing, string keyword, string by)
    {
        return by switch
        {
            "name" => // (search by package name only)
                existing.Name.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase),
            "name-desc" => // (search by package name and description)
                existing.Name.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase) ||
                existing.Description.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase),
            "maintainer" => // (search by package maintainer)
                existing.Maintainer.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase),
            "depends" => // (search for packages that depend on keywords)
                existing.Depends?.Any(depend => depend.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)) == true,
            "makedepends" => // (search for packages that makedepend on keywords)
                existing.MakeDepends?.Any(depend => depend.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)) == true,
            "optdepends" => // (search for packages that optdepend on keywords)
                existing.OptDepends?.Any(depend => depend.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)) == true,
            "checkdepends" => // (search for packages that checkdepend on keywords)
                existing.CheckDepends?.Any(depend => depend.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)) == true,
            _ => false
        };
    }
}