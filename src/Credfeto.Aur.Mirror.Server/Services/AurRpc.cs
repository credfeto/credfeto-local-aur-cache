using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
using Microsoft.Extensions.Primitives;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class AurRpc : IAurRpc
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _connections;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AurRpc> _logger;
    private readonly ServerConfig _serverConfig;

    public AurRpc(IHttpClientFactory httpClientFactory, IOptions<ServerConfig> config, ILogger<AurRpc> logger)
    {
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._connections = new(StringComparer.Ordinal);

        EnsureDirectoryExists(this._serverConfig.Storage.Metadata);
        EnsureDirectoryExists(this._serverConfig.Storage.Repos);
    }

    public async ValueTask<RpcResponse> SearchAsync(
        IReadOnlyDictionary<string, StringValues> query,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        try
        {
            RpcResponse upstream = await this.RequestUpstreamAsync(
                query: query,
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            await this.SyncUpstreamReposAsync(upstream);

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            Debug.WriteLine(exception.Message);
            return await this.ExecuteLocalSearchQueryAsync(query: query, cancellationToken: cancellationToken);
        }
    }

    public async ValueTask<RpcResponse> InfoAsync(IReadOnlyDictionary<string, StringValues> query, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ValueTask.CompletedTask;

        return RpcResults.InfoNotFound;
    }

    private async ValueTask<RpcResponse> ExecuteLocalSearchQueryAsync(
        IReadOnlyDictionary<string, StringValues> query,
        CancellationToken cancellationToken
    )
    {
        bool multi = query.ContainsKey("args[]");
        int version = int.Parse(query["v"].ToString(), CultureInfo.InvariantCulture);

        List<SearchResult> results = [];
        string[] files = Directory.GetFiles("*.json");

        foreach (string metadataFileName in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

            if (existing is not null)
            {
                // Check filtering
                results.Add(existing);
            }
        }

        return new(count: results.Count, results: results, rpcType: multi ? "multiinfo" : "search", version: version);
    }

    private async ValueTask SyncUpstreamReposAsync(RpcResponse upstream)
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
                EnsureRepositoryHasBeenCloned(repoPath: repoPath, upstreamRepo: upstreamRepo, changed: changed);

                if (changed)
                {
                    await this.SavePackageToMetadataAsync(package: package, metadataFileName: metadataFileName);
                }
            }
            else
            {
                EnsureRepositoryHasBeenCloned(repoPath: repoPath, upstreamRepo: upstreamRepo, changed: true);

                await this.SavePackageToMetadataAsync(package: package, metadataFileName: metadataFileName);
            }
        }
    }

    private async ValueTask<SearchResult?> ReadPackageFromMetadataAsync(string metadataFileName)
    {
        try
        {
            string json = await File.ReadAllTextAsync(
                path: metadataFileName,
                encoding: Encoding.UTF8,
                cancellationToken: DoNotCancelEarly
            );

            return JsonSerializer.Deserialize(json, jsonTypeInfo: AppJsonContexts.Default.SearchResult);
        }
        catch (Exception exception)
        {
            this._logger.FailedToReadSavedMetadata(metadataFileName, exception.Message, exception);
            File.Delete(metadataFileName);

            return null;
        }
    }

    private static void EnsureRepositoryHasBeenCloned(string repoPath, string upstreamRepo, bool changed)
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

    private static void UpdateRepository(string repoFolder)
    {
        using (Repository repo = new(repoFolder))
        {
            FetchOptions options = new() { Prune = true, TagFetchMode = TagFetchMode.Auto };

            Remote? remote = repo.Network.Remotes["origin"];
            const string msg = "Fetching remote";
            IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(
                repository: repo,
                remote: remote.Name,
                refspecs: refSpecs,
                options: options,
                logMessage: msg
            );
        }
    }

    private static void CloneRepository(string upstreamRepo, string repoPath)
    {
        Repository.Clone(sourceUrl: upstreamRepo, workdirPath: repoPath, new() { IsBare = true });
    }

    private async ValueTask SavePackageToMetadataAsync(SearchResult package, string metadataFileName)
    {
        try
        {
            EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

            string json = JsonSerializer.Serialize(value: package, jsonTypeInfo: AppJsonContexts.Default.SearchResult);
            await File.WriteAllTextAsync(
                path: metadataFileName,
                contents: json,
                encoding: Encoding.UTF8,
                cancellationToken: DoNotCancelEarly
            );
        }
        catch (Exception exception)
        {
            this._logger.SaveMetadataFailed(
                filename: metadataFileName,
                message: exception.Message,
                exception: exception
            );
        }
    }

    private async ValueTask<RpcResponse> RequestUpstreamAsync(
        IReadOnlyDictionary<string, StringValues> query,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, query: query);
        this._logger.RequestingUpstream(requestUri);
        SemaphoreSlim? wait = await this.GetSemaphoreAsync(baseUri: requestUri, cancellationToken: cancellationToken);

        try
        {
            using (
                HttpResponseMessage result = await client.GetAsync(
                    requestUri: requestUri,
                    cancellationToken: DoNotCancelEarly
                )
            )
            {
                if (result.IsSuccessStatusCode)
                {
                    this._logger.SuccessFromUpstream(uri: requestUri, statusCode: result.StatusCode);

                    await using (
                        Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken: DoNotCancelEarly)
                    )
                    {
                        return await JsonSerializer.DeserializeAsync<RpcResponse>(
                                utf8Json: stream,
                                jsonTypeInfo: AppJsonContexts.Default.RpcResponse,
                                cancellationToken: cancellationToken
                            ) ?? throw new JsonException("Could not deserialize response");
                    }
                }

                return Failed(requestUri: requestUri, resultStatusCode: result.StatusCode);
            }
        }
        finally
        {
            wait?.Release();
        }
    }

    [DoesNotReturn]
    private static RpcResponse Failed(Uri requestUri, HttpStatusCode resultStatusCode)
    {
        throw new HttpRequestException(
            $"Failed to download {requestUri}: {resultStatusCode.GetName()}",
            inner: null,
            statusCode: resultStatusCode
        );
    }

    private static Uri MakeUri(Uri baseUri, IReadOnlyDictionary<string, StringValues> query)
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

        string full = urlBase + "?" + string.Join(separator: '&', query.Select(q => $"{q.Key}={q.Value}"));

        return new(uriString: full, uriKind: UriKind.Absolute);
    }

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Rpc, uriKind: UriKind.Absolute);

        return this._httpClientFactory.CreateClient(nameof(AurRpc)).WithBaseAddress(baseUri).WithUserAgent(userAgent);
    }

    private async ValueTask<SemaphoreSlim?> GetSemaphoreAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        if (!baseUri.PathAndQuery.EndsWith(value: ".gz", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string key = baseUri.DnsSafeHost;

        if (this._connections.TryGetValue(key: key, out SemaphoreSlim? semaphore))
        {
            await semaphore.WaitAsync(cancellationToken);

            return semaphore;
        }

        semaphore = this._connections.GetOrAdd(key: key, new SemaphoreSlim(1));
        await semaphore.WaitAsync(cancellationToken);

        return semaphore;
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}