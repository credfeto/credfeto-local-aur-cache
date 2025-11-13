using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Server.Config;
using Credfeto.Aur.Mirror.Server.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class AurRepos : IAurRepos
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AurRepos> _logger;
    private readonly ServerConfig _serverConfig;
    private readonly IUpdateLock _updateLock;

    public AurRepos(
        IOptions<ServerConfig> serverConfig,
        IHttpClientFactory httpClientFactory,
        IUpdateLock updateLock,
        ILogger<AurRepos> logger
    )
    {
        this._serverConfig = serverConfig.Value;
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
        this._updateLock = updateLock;
    }

    public async ValueTask<byte[]?> GetPackagesAsync(
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        string filename = Path.Combine(path1: this._serverConfig.Storage.Repos, path2: "packages.gz");

        try
        {
            byte[] fileContent = await this.RequestPackagesUpstreamAsync(
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            SemaphoreSlim wait = await this._updateLock.GetLockAsync(
                fileName: filename,
                cancellationToken: cancellationToken
            );

            try
            {
                await File.WriteAllBytesAsync(path: filename, bytes: fileContent, cancellationToken: DoNotCancelEarly);

                return fileContent;
            }
            finally
            {
                wait.Release();
            }
        }
        catch (Exception exception)
        {
            this._logger.LogError(exception: exception, message: "Failed to read packages.gz");
            Debug.WriteLine(exception.Message);

            if (File.Exists(filename))
            {
                return await File.ReadAllBytesAsync(path: filename, cancellationToken: cancellationToken);
            }

            return null;
        }
    }

    private async ValueTask<byte[]> RequestPackagesUpstreamAsync(
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        HttpClient httpClient = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, pathAndQuery: "/packages.gz");

        using (
            HttpResponseMessage result = (
                await httpClient.GetAsync(requestUri: requestUri, cancellationToken: cancellationToken)
            ).EnsureSuccessStatusCode()
        )
        {
            return await result.Content.ReadAsByteArrayAsync(cancellationToken);
        }
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

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Repos, uriKind: UriKind.Absolute);

        return this._httpClientFactory.CreateClient(nameof(AurRpc)).WithBaseAddress(baseUri).WithUserAgent(userAgent);
    }
}
