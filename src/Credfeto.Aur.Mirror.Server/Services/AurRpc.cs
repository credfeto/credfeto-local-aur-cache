using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Config;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Credfeto.Aur.Mirror.Server.Models;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class AurRpc : IAurRpc
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AurRpc> _logger;
    private readonly ServerConfig _serverConfig;
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _connections;

    public AurRpc(IHttpClientFactory httpClientFactory, IOptions<ServerConfig> config, ILogger<AurRpc> logger)
    {
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._connections = new(StringComparer.Ordinal);
    }

    public async ValueTask<RpcResponse> GetAsync(string query, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, query);
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
                    await using (Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken: DoNotCancelEarly))
                    {
                        this._logger.LogInformation("Retrieved from upstream");
                        return await JsonSerializer.DeserializeAsync<RpcResponse>(stream, AppJsonContexts.Default.RpcResponse, cancellationToken)
                            ?? throw new JsonException("Could not deserialize response");

                    }

                    // this._logger.UpstreamPackageOk(
                    //     upstream: requestUri,
                    //     statusCode: result.StatusCode,
                    //     length: bytes.Length
                    // );

                    // return bytes;
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

        return new(full, UriKind.Absolute);
    }

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Rpc, uriKind: UriKind.Absolute);

        return this
               ._httpClientFactory.CreateClient(nameof(AurRpc))
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
}