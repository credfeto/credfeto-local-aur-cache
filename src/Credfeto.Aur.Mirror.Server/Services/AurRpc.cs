using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    }

    public async ValueTask<RpcResponse> GetAsync(string query, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        RpcResponse upstream = await this.RequestUpstreamAsync(query: query, userAgent: userAgent, cancellationToken: cancellationToken);

        foreach (SearchResult package in upstream.Results)
        {
            string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.Id}.json");
            string repoPath = Path.Combine(path1: this._serverConfig.Storage.Repos, $"{package.Name}.git");
            string upstreamRepo = this._serverConfig.Upstream.Repos + "/" + package.Name + ".git";

            if (File.Exists(metadataFileName))
            {
                // Read file
                // if updated then update the local repo
                // Save package over the top of the metadata
            }
            else
            {
                // Clone Repo
                Repository.Clone(sourceUrl: upstreamRepo, workdirPath: repoPath, new() { IsBare = true });

                // Save package to metadata.

                await this.SavePackageToMetadataAsync(package: package, metadataFileName: metadataFileName);
            }
        }

        return upstream;
    }

    private async ValueTask SavePackageToMetadataAsync(SearchResult package, string metadataFileName)
    {
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
    }

    private async ValueTask<RpcResponse> RequestUpstreamAsync(string query, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, query: query);
        this._logger.RequestingUpstream(requestUri);
        SemaphoreSlim? wait = await this.GetSemaphoreAsync(baseUri: requestUri, cancellationToken: cancellationToken);

        try
        {
            using (HttpResponseMessage result = await client.GetAsync(requestUri: requestUri, cancellationToken: DoNotCancelEarly))
            {
                if (result.IsSuccessStatusCode)
                {
                    this._logger.SuccessFromUpstream(uri: requestUri, statusCode: result.StatusCode);

                    await using (Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken: DoNotCancelEarly))
                    {
                        return await JsonSerializer.DeserializeAsync<RpcResponse>(utf8Json: stream, jsonTypeInfo: AppJsonContexts.Default.RpcResponse, cancellationToken: cancellationToken) ??
                               throw new JsonException("Could not deserialize response");
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
        throw new HttpRequestException($"Failed to download {requestUri}: {resultStatusCode.GetName()}", inner: null, statusCode: resultStatusCode);
    }

    private static Uri MakeUri(Uri baseUri, string query)
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

        string full = urlBase + "?" + query;

        return new(uriString: full, uriKind: UriKind.Absolute);
    }

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Rpc, uriKind: UriKind.Absolute);

        return this._httpClientFactory.CreateClient(nameof(AurRpc))
                   .WithBaseAddress(baseUri)
                   .WithUserAgent(userAgent);
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