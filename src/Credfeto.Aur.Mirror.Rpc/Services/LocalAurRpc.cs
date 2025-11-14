using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Models;
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
    private readonly IUpdateLock _updateLock;

    public LocalAurRpc(IOptions<ServerConfig> config, IGitServer gitServer, IUpdateLock updateLock, ILocalAurMetadata localAurMetadata, ILogger<LocalAurRpc> logger)
    {
        this._gitServer = gitServer;
        this._updateLock = updateLock;
        this._localAurMetadata = localAurMetadata;
        this._logger = logger;
        this._gitServer = gitServer;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._updateLock = updateLock;

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
        SearchTracking tracking = new();

        foreach (SearchResult package in items)
        {
            string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.Id}.json");
            string upstreamRepo = this._serverConfig.Upstream.Repos + "/" + package.Name + ".git";

            this._logger.CheckingPackage(packageId: package.Id, packageName: package.Name, metadataFileName: metadataFileName, upstreamRepo: upstreamRepo);

            if (File.Exists(metadataFileName))
            {
                this._logger.FoundMetadata(packageId: package.Id, packageName: package.Name, metadataFileName: metadataFileName);
                SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

                bool changed = existing is null || existing.LastModified != package.LastModified;
                await this._gitServer.EnsureRepositoryHasBeenClonedAsync(repoName: package.Name, upstreamRepo: upstreamRepo, changed: changed, cancellationToken: DoNotCancelEarly);

                if (changed)
                {
                    tracking.AppendRepoSyncSearchResult(package);
                }
            }
            else
            {
                this._logger.NoMetadata(packageId: package.Id, packageName: package.Name, metadataFileName: metadataFileName);

                await this._gitServer.EnsureRepositoryHasBeenClonedAsync(repoName: package.Name, upstreamRepo: upstreamRepo, changed: true, cancellationToken: DoNotCancelEarly);

                tracking.AppendRepoSyncSearchResult(package);
            }
        }

        if (tracking.ToSave is not [])
        {
            await this.SaveSearchResultsAsync(tracking);
        }
    }

    private async ValueTask SaveSearchResultsAsync(SearchTracking searchTracking)
    {
        foreach (SearchResult package in searchTracking.ToSave)
        {
            string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.Id}.json");
            await this.SavePackageToMetadataAsync(package: package, metadataFileName: metadataFileName, cancellationToken: DoNotCancelEarly);
        }
    }

    private async ValueTask<SearchResult?> ReadPackageFromMetadataAsync(string metadataFileName)
    {
        try
        {
            string json = await File.ReadAllTextAsync(path: metadataFileName, encoding: Encoding.UTF8, cancellationToken: DoNotCancelEarly);

            return JsonSerializer.Deserialize(json: json, jsonTypeInfo: AppJsonContexts.Default.SearchResult);
        }
        catch (Exception exception)
        {
            this._logger.FailedToReadSavedMetadata(filename: metadataFileName, message: exception.Message, exception: exception);
            File.Delete(metadataFileName);

            return null;
        }
    }

    private async ValueTask SavePackageToMetadataAsync(SearchResult package, string metadataFileName, CancellationToken cancellationToken)
    {
        SemaphoreSlim wait = await this._updateLock.GetLockAsync(fileName: metadataFileName, cancellationToken: cancellationToken);

        try
        {
            EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

            string json = JsonSerializer.Serialize(value: package, jsonTypeInfo: AppJsonContexts.Default.SearchResult);
            await File.WriteAllTextAsync(path: metadataFileName, contents: json, encoding: Encoding.UTF8, cancellationToken: DoNotCancelEarly);
        }
        catch (Exception exception)
        {
            this._logger.SaveMetadataFailed(filename: metadataFileName, message: exception.Message, exception: exception);
        }
        finally
        {
            wait.Release();
        }
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

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }


}