using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Config;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Credfeto.Aur.Mirror.Server.Services.LoggingExtensions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class AurRpc : IAurRpc
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AurRpc> _logger;
    private readonly ServerConfig _serverConfig;
    private readonly IUpdateLock _updateLock;

    public AurRpc(IHttpClientFactory httpClientFactory, IOptions<ServerConfig> config, IUpdateLock updateLock, ILogger<AurRpc> logger)
    {
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._updateLock = updateLock;

        EnsureDirectoryExists(this._serverConfig.Storage.Metadata);
        EnsureDirectoryExists(this._serverConfig.Storage.Repos);
    }

    public async ValueTask<RpcResponse> SearchAsync(string keyword, string by, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        try
        {
            RpcResponse upstream = await this.RequestSearchUpstreamAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);

            await this.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent, new(false));

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            Debug.WriteLine(exception.Message);

            return await this.ExecuteLocalSearchQueryAsync(keyword: keyword, by: by, cancellationToken: cancellationToken);
        }
    }

    public async ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        try
        {
            if (packages is [])
            {
                return RpcResults.InfoNotFound;
            }

            RpcResponse upstream = await this.RequestInfoUpstreamAsync(packages: packages, userAgent: userAgent, cancellationToken: cancellationToken);

            await this.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent, new(true));

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            Debug.WriteLine(exception.Message);

            return await this.ExecuteLocalInfoQueryAsync(package: packages, cancellationToken: cancellationToken);
        }
    }

    private async Task<RpcResponse> ExecuteLocalInfoQueryAsync(IReadOnlyList<string> package, CancellationToken cancellationToken)
    {
        List<SearchResult> results = [];
        IReadOnlyList<string> files = this.GetMetadataFiles();

        foreach (string metadataFileName in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

            if (existing is null)
            {
                continue;
            }

            if (package.Any(p => StringComparer.OrdinalIgnoreCase.Equals(x: existing.Name, y: p)))
            {
                results.Add(existing);
            }
        }

        return new(count: results.Count, results: results, rpcType: "multiinfo", version: RpcResults.RpcVersion);
    }

    private IReadOnlyList<string> GetMetadataFiles()
    {
        try
        {
            EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

            return Directory.GetFiles(this._serverConfig.Storage.Metadata, "*.json");
        }
        catch (Exception exception)
        {
            this._logger.CouldNotFindMetadataFiles(
                this._serverConfig.Storage.Metadata,
                exception.Message,
                exception: exception
            );
            return [];
        }
    }

    private async ValueTask<RpcResponse> ExecuteLocalSearchQueryAsync(string keyword, string by, CancellationToken cancellationToken)
    {
        List<SearchResult> results = [];
        IReadOnlyList<string> files = this.GetMetadataFiles();

        foreach (string metadataFileName in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

            if (existing is not null && IsSearchMatch(existing: existing, keyword: keyword, by: by))
            {
                // Check filtering
                results.Add(existing);
            }
        }

        return new(count: results.Count, results: results, rpcType: "search", version: RpcResults.RpcVersion);
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

    private async ValueTask SyncUpstreamReposAsync(RpcResponse upstream, ProductInfoHeaderValue? userAgent, SearchTracking tracking)
    {
        foreach (SearchResult package in upstream.Results)
        {
            string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.Id}.json");
            string repoPath = Path.Combine(path1: this._serverConfig.Storage.Repos, $"{package.Name}.git");
            string upstreamRepo = this._serverConfig.Upstream.Repos + "/" + package.Name + ".git";

            if (File.Exists(metadataFileName))
            {
                SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

                bool changed = existing is null || existing.LastModified != package.LastModified;
                await this.EnsureRepositoryHasBeenClonedAsync(repoPath: repoPath, upstreamRepo: upstreamRepo, changed: changed, cancellationToken: DoNotCancelEarly);

                if (changed)
                {
                    tracking.AppendRepoSyncSearchResult(package);
                }
            }
            else
            {
                await this.EnsureRepositoryHasBeenClonedAsync(repoPath: repoPath, upstreamRepo: upstreamRepo, changed: true, cancellationToken: DoNotCancelEarly);

                tracking.AppendRepoSyncSearchResult(package);
            }
        }

        if (tracking.ToSave is not [])
        {
            await this.SaveSearchResultsAsync(tracking.ToSave);

            return;
        }

        if (tracking.Packages is not [])
        {
            await this.SaveUpstreamAsync(packages: tracking.Packages, userAgent: userAgent);
        }
    }

    private async ValueTask SaveUpstreamAsync(List<string> packages, ProductInfoHeaderValue? userAgent)
    {
        RpcResponse packageInfo = await this.RequestInfoUpstreamAsync(packages: packages, userAgent: userAgent, cancellationToken: DoNotCancelEarly);

        await this.SaveSearchResultsAsync(packageInfo.Results);
    }

    private async ValueTask SaveSearchResultsAsync(IReadOnlyList<SearchResult> packagesToSave)
    {
        foreach (SearchResult package in packagesToSave)
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

    private async ValueTask EnsureRepositoryHasBeenClonedAsync(string repoPath, string upstreamRepo, bool changed, CancellationToken cancellationToken)
    {
        SemaphoreSlim wait = await this._updateLock.GetLockAsync(fileName: repoPath, cancellationToken: cancellationToken);

        try
        {
            if (Directory.Exists(repoPath))
            {
                string? repoFolder = Repository.Discover(repoPath);

                if (repoFolder is null)
                {
                    CloneRepository(upstreamRepo: upstreamRepo, repoPath: repoPath);
                }
                else if (changed)
                {
                    UpdateRepository(repoFolder);
                }
            }
            else
            {
                CloneRepository(upstreamRepo: upstreamRepo, repoPath: repoPath);
            }
        }
        finally
        {
            wait.Release();
        }
    }

    private static void UpdateRepository(string repoFolder)
    {
        using (Repository repo = new(repoFolder))
        {
            FetchOptions options = new() { Prune = true, TagFetchMode = TagFetchMode.Auto };

            Remote? remote = repo.Network.Remotes["origin"];
            const string msg = "Fetching remote";
            IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repository: repo, remote: remote.Name, refspecs: refSpecs, options: options, logMessage: msg);
        }
    }

    private static void CloneRepository(string upstreamRepo, string repoPath)
    {
        Repository.Clone(sourceUrl: upstreamRepo, workdirPath: repoPath, new() { IsBare = true });
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

    private ValueTask<RpcResponse> RequestInfoUpstreamAsync(IReadOnlyList<string> packages, ProductInfoHeaderValue? userAgent, in CancellationToken cancellationToken)
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeInfoUri(baseUri: baseUri, packages: packages);

        return this.RequestUpstreamCommonAsync(client: client, requestUri: requestUri, cancellationToken: cancellationToken);
    }

    private async ValueTask<RpcResponse> RequestUpstreamCommonAsync(HttpClient client, Uri requestUri, CancellationToken cancellationToken)
    {
        this._logger.RequestingUpstream(requestUri);

        using (HttpResponseMessage result = await client.GetAsync(requestUri: requestUri, cancellationToken: cancellationToken))
        {
            if (result.IsSuccessStatusCode)
            {
                this._logger.SuccessFromUpstream(uri: requestUri, statusCode: result.StatusCode);

                await using (Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken: cancellationToken))
                {
                    return await JsonSerializer.DeserializeAsync<RpcResponse>(utf8Json: stream, jsonTypeInfo: AppJsonContexts.Default.RpcResponse, cancellationToken: cancellationToken) ??
                           throw new JsonException("Could not deserialize response");
                }
            }

            return Failed(requestUri: requestUri, resultStatusCode: result.StatusCode);
        }
    }

    private ValueTask<RpcResponse> RequestSearchUpstreamAsync(string keyword, string by, ProductInfoHeaderValue? userAgent, in CancellationToken cancellationToken)
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, $"/v5/search/{keyword}?by={by}");

        return this.RequestUpstreamCommonAsync(client: client, requestUri: requestUri, cancellationToken: cancellationToken);
    }

    [DoesNotReturn]
    private static RpcResponse Failed(Uri requestUri, HttpStatusCode resultStatusCode)
    {
        throw new HttpRequestException($"Failed to download {requestUri}: {resultStatusCode.GetName()}", inner: null, statusCode: resultStatusCode);
    }

    private static Uri MakeUri(Uri baseUri, string pathAndQuery)
    {
        string urlBase = baseUri.ToString();

        if (urlBase.EndsWith('?'))
        {
            urlBase = urlBase[..^1];
        }

        if (urlBase.EndsWith('/'))
        {
            urlBase = urlBase[..^1];
        }

        string full = urlBase + pathAndQuery;

        return new(uriString: full, uriKind: UriKind.Absolute);
    }

    private static Uri MakeInfoUri(Uri baseUri, IReadOnlyList<string> packages)
    {
        if (packages.Count == 1)
        {
            return MakeUri(baseUri: baseUri, $"/v5/info/{packages[0]}");
        }

        return MakeUri(baseUri: baseUri, "/v5/info?" + string.Join(separator: '&', packages.Select(p => $"arg[]={p}")));
    }

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Rpc, uriKind: UriKind.Absolute);

        return this._httpClientFactory.CreateClient(nameof(AurRpc))
                   .WithBaseAddress(baseUri)
                   .WithUserAgent(userAgent);
    }



    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private sealed class SearchTracking
    {
        private readonly bool _fromInfo;

        public SearchTracking(bool fromInfo)
        {
            this._fromInfo = fromInfo;
            this.Packages = [];
            this.ToSave = [];
        }

        public List<string> Packages { get; }

        public List<SearchResult> ToSave { get; }

        public void AppendRepoSyncSearchResult(SearchResult package)
        {
            if (this._fromInfo)
            {
                this.ToSave.Add(package);
            }
            else
            {
                this.Packages.Add(package.Name);
            }
        }
    }
}