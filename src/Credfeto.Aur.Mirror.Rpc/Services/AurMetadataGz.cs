using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Extensions;
using Credfeto.Aur.Mirror.Rpc.Helpers;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class AurMetadataGz : IAurMetadataGz
{
    private const string GzFileName = "packages-meta-ext-v1.json.gz";
    private const string RemotePath = "/packages-meta-ext-v1.json.gz";
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AurMetadataGz> _logger;
    private readonly ServerConfig _serverConfig;
    private readonly SemaphoreSlim _refreshSemaphore;
    private readonly TimeProvider _timeProvider;
    private readonly IUpdateLock _updateLock;

    private volatile IReadOnlyList<SearchResult> _parsedResults;
    private long _lastDownloadedAtTicks;

    public AurMetadataGz(
        IOptions<ServerConfig> serverConfig,
        IHttpClientFactory httpClientFactory,
        IUpdateLock updateLock,
        TimeProvider timeProvider,
        ILogger<AurMetadataGz> logger
    )
    {
        this._serverConfig = serverConfig.Value;
        this._httpClientFactory = httpClientFactory;
        this._updateLock = updateLock;
        this._timeProvider = timeProvider;
        this._logger = logger;
        this._parsedResults = [];
        this._lastDownloadedAtTicks = DateTimeOffset.MinValue.UtcTicks;
        this._refreshSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
    }

    private DateTimeOffset LastDownloadedAt =>
        new(ticks: Interlocked.Read(ref this._lastDownloadedAtTicks), offset: TimeSpan.Zero);

    public async ValueTask<byte[]?> GetPackagesAsync(
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        string filename = Path.Combine(path1: this._serverConfig.Storage.Repos, path2: GzFileName);

        try
        {
            byte[] fileContent = await this.DownloadFromUpstreamAsync(
                userAgent: userAgent,
                cancellationToken: cancellationToken
            );

            await this.SaveAndCacheAsync(
                filename: filename,
                fileContent: fileContent,
                cancellationToken: cancellationToken
            );

            return fileContent;
        }
        catch (Exception exception)
        {
            this._logger.FailedToDownloadMetadataGz(message: exception.Message, exception: exception);

            if (File.Exists(filename))
            {
                byte[] fileContent = await File.ReadAllBytesAsync(path: filename, cancellationToken: cancellationToken);
                this._logger.LoadedMetadataGzFromDisk(byteCount: fileContent.Length);

                await this.ParseAndUpdateCacheAsync(fileContent);

                return fileContent;
            }

            return null;
        }
    }

    public ValueTask<IReadOnlyList<SearchResult>> SearchAsync(
        string keyword,
        string by,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<SearchResult> snapshot = this._parsedResults;
        IReadOnlyList<SearchResult> results =
        [
            .. snapshot.Where(r => SearchResultMatcher.IsMatch(result: r, keyword: keyword, by: by)),
        ];

        return ValueTask.FromResult(results);
    }

    public async ValueTask TriggerRefreshIfNewerAsync(
        long lastModifiedUnixTimestamp,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset lastModified = DateTimeOffset.FromUnixTimeSeconds(lastModifiedUnixTimestamp);
        DateTimeOffset cachedAt = this.LastDownloadedAt;

        if (lastModified <= cachedAt)
        {
            this._logger.SkippingRefresh(lastModified: lastModified, cachedAt: cachedAt);

            return;
        }

        this._logger.TriggeringRefresh(lastModified: lastModified, cachedAt: cachedAt);

        await this.RefreshAsync(cancellationToken);
    }

    private async ValueTask RefreshAsync(CancellationToken cancellationToken)
    {
        bool acquired = await this._refreshSemaphore.WaitAsync(
            timeout: TimeSpan.Zero,
            cancellationToken: cancellationToken
        );

        if (!acquired)
        {
            return;
        }

        try
        {
            await this.GetPackagesAsync(userAgent: null, cancellationToken: DoNotCancelEarly);
        }
        finally
        {
            this._refreshSemaphore.Release();
        }
    }

    private async ValueTask SaveAndCacheAsync(string filename, byte[] fileContent, CancellationToken cancellationToken)
    {
        SemaphoreSlim wait = await this._updateLock.GetLockAsync(
            fileName: filename,
            cancellationToken: cancellationToken
        );

        try
        {
            await File.WriteAllBytesAsync(path: filename, bytes: fileContent, cancellationToken: DoNotCancelEarly);
        }
        finally
        {
            wait.Release();
        }

        await this.ParseAndUpdateCacheAsync(fileContent);
    }

    private async ValueTask ParseAndUpdateCacheAsync(byte[] gzContent)
    {
        try
        {
            await using MemoryStream compressedStream = new(gzContent);
            await using GZipStream gzipStream = new(stream: compressedStream, mode: CompressionMode.Decompress);

            SearchResult[]? results = await JsonSerializer.DeserializeAsync(
                utf8Json: gzipStream,
                jsonTypeInfo: RpcJsonContext.Default.SearchResultArray,
                cancellationToken: DoNotCancelEarly
            );

            IReadOnlyList<SearchResult> parsed = results ?? [];
            this._parsedResults = parsed;
            Interlocked.Exchange(ref this._lastDownloadedAtTicks, this._timeProvider.GetUtcNow().UtcTicks);

            this._logger.DownloadedMetadataGz(byteCount: gzContent.Length, packageCount: parsed.Count);
        }
        catch (Exception exception)
        {
            this._logger.FailedToParseMetadataGz(message: exception.Message, exception: exception);
        }
    }

    private async ValueTask<byte[]> DownloadFromUpstreamAsync(
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    )
    {
        this._logger.DownloadingMetadataGz();

        HttpClient httpClient = this.GetClient(userAgent: userAgent, out Uri baseUri);

        Uri requestUri = MakeUri(baseUri: baseUri, pathAndQuery: RemotePath);

        using HttpResponseMessage result = (
            await httpClient.GetAsync(requestUri: requestUri, cancellationToken: cancellationToken)
        ).EnsureSuccessStatusCode();

        return await result.Content.ReadAsByteArrayAsync(cancellationToken);
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

        return new(uriString: urlBase + pathAndQuery, uriKind: UriKind.Absolute);
    }

    private HttpClient GetClient(ProductInfoHeaderValue? userAgent, out Uri baseUri)
    {
        baseUri = new(uriString: this._serverConfig.Upstream.Repos, uriKind: UriKind.Absolute);

        return this
            ._httpClientFactory.CreateClient(nameof(AurMetadataGz))
            .WithBaseAddress(baseUri)
            .WithUserAgent(userAgent);
    }
}
