using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Models;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Extensions;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Models;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class RemoteAurRpc : IRemoteAurRpc
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RemoteAurRpc> _logger;
    private readonly ServerConfig _serverConfig;

    public RemoteAurRpc(
        IHttpClientFactory httpClientFactory,
        IOptions<ServerConfig> config,
        ILogger<RemoteAurRpc> logger
    )
    {
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
        this._serverConfig = config.Value;

        // TASK: Store local config in a DB that's quick to search rather than filesystem
        // TASK: Look locally for everything and ONLY look in RPC if a significant amount of time has occured since the last query for that same data
    }

    public async ValueTask<RpcResponse> SearchAsync(
        string keyword,
        string by,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, $"/v5/search/{keyword}?by={by}");
        RpcResponse upstream = await this.RequestUpstreamCommonAsync(
            client: client,
            requestUri: requestUri,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<string> packageNames = [.. upstream.Results.Select(r => r.Name)];

        RpcResponse infoUpstream = await this.InfoAsync(
            packages: packageNames,
            userAgent: userAgent,
            cancellationToken: cancellationToken
        );

        return new(
            count: infoUpstream.Count,
            results: infoUpstream.Results,
            rpcType: upstream.RpcType,
            version: upstream.Version
        );
    }

    public async ValueTask<RpcResponse> InfoAsync(
        IReadOnlyList<string> packages,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        this._logger.PackageInfo(packages);

        if (packages is [])
        {
            return RpcResults.InfoNotFound;
        }

        HttpClient client = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeInfoUri(baseUri: baseUri, packages: packages);
        RpcResponse upstream = await this.RequestUpstreamCommonAsync(
            client: client,
            requestUri: requestUri,
            cancellationToken: cancellationToken
        );

        return upstream;
    }

    private async ValueTask<RpcResponse> RequestUpstreamCommonAsync(
        HttpClient client,
        Uri requestUri,
        CancellationToken cancellationToken
    )
    {
        this._logger.RequestingUpstream(requestUri);

        using (
            HttpResponseMessage result = await client.GetAsync(
                requestUri: requestUri,
                cancellationToken: cancellationToken
            )
        )
        {
            if (result.IsSuccessStatusCode)
            {
                this._logger.SuccessFromUpstream(uri: requestUri, statusCode: result.StatusCode);

                await using (
                    Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
                )
                {
                    return await JsonSerializer.DeserializeAsync<RpcResponse>(
                            utf8Json: stream,
                            jsonTypeInfo: RpcJsonContext.Default.RpcResponse,
                            cancellationToken: cancellationToken
                        ) ?? throw new JsonException("Could not deserialize response");
                }
            }

            return Failed(requestUri: requestUri, resultStatusCode: result.StatusCode);
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

        return this._httpClientFactory.CreateClient(nameof(AurRpc)).WithBaseAddress(baseUri).WithUserAgent(userAgent);
    }
}
