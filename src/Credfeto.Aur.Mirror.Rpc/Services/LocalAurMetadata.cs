using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Credfeto.Date.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class LocalAurMetadata : ILocalAurMetadata
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly ICurrentTimeSource _currentTimeSource;
    private readonly ILogger<LocalAurMetadata> _logger;
    private readonly ConcurrentDictionary<string, Tracking> _metadata;
    private readonly ServerConfig _serverConfig;
    private readonly IUpdateLock _updateLock;

    public LocalAurMetadata(IOptions<ServerConfig> config, IUpdateLock updateLock, ICurrentTimeSource currentTimeSource, ILogger<LocalAurMetadata> logger)
    {
        this._updateLock = updateLock;
        this._currentTimeSource = currentTimeSource;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._metadata = new(StringComparer.OrdinalIgnoreCase);

        // TASK: Store local config in a DB that's quick to search rather than filesystem
    }

    public async ValueTask LoadAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<string> files = this.GetMetadataFiles();

        foreach (string metadataFileName in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SearchResult? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

            if (existing is null)
            {
                continue;
            }

            _ = this._metadata.TryAdd(key: existing.Name, new(this._currentTimeSource.UtcNow(), this._currentTimeSource.UtcNow(), this._currentTimeSource.UtcNow(), item: existing));
        }
    }

    public ValueTask<IReadOnlyList<SearchResult>> SearchAsync(Func<SearchResult, bool> predicate, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchResult> results =
        [
            .. this._metadata.Values.Where(item => predicate(item.Item))
                   .Select(item =>
                           {
                               item.LastAccessed = this._currentTimeSource.UtcNow();

                               return item.Item;
                           })
        ];

        return ValueTask.FromResult(results);
    }

    public SearchResult? Get(string packageName)
    {
        if (this._metadata.TryGetValue(key: packageName, out Tracking? result))
        {
            result.LastAccessed = this._currentTimeSource.UtcNow();

            return result.Item;
        }

        return null;
    }

    public async ValueTask UpdateAsync(SearchResult package, Func<SearchResult, bool, ValueTask> onUpdate, CancellationToken cancellationToken)
    {
        bool save = false;
        bool changed = false;

        if (this._metadata.TryGetValue(key: package.Name, out Tracking? existing))
        {
            if (existing.Item.LastModified < package.LastModified)
            {
                existing.Item = package;
                existing.LastRequestedUpstream = this._currentTimeSource.UtcNow();
                existing.LastAccessed = this._currentTimeSource.UtcNow();
                save = true;
                changed = true;
            }
        }
        else
        {
            _ = this._metadata.TryAdd(key: package.Name, new(this._currentTimeSource.UtcNow(), this._currentTimeSource.UtcNow(), this._currentTimeSource.UtcNow(), item: package));
            save = true;
        }

        if (save)
        {
            // Future: Queue package update in background -> once its updated in cache will be reading from there from
            string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.Id}.json");
            await this.SavePackageToMetadataAsync(package: package, metadataFileName: metadataFileName, cancellationToken: DoNotCancelEarly);

            await onUpdate(arg1: package, arg2: changed);
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

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private IReadOnlyList<string> GetMetadataFiles()
    {
        try
        {
            EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

            return Directory.GetFiles(path: this._serverConfig.Storage.Metadata, searchPattern: "*.json");
        }
        catch (Exception exception)
        {
            this._logger.CouldNotFindMetadataFiles(directory: this._serverConfig.Storage.Metadata, message: exception.Message, exception: exception);

            return [];
        }
    }

    private sealed class Tracking
    {
        public Tracking(DateTimeOffset lastSaved, DateTimeOffset lastAccessed, DateTimeOffset lastRequestedUpstream, SearchResult item)
        {
            this.LastSaved = lastSaved;
            this.LastAccessed = lastAccessed;
            this.LastRequestedUpstream = lastRequestedUpstream;
            this.Item = item;
        }

        public DateTimeOffset LastSaved { get; set; }

        public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds(this.Item.LastModified);

        public DateTimeOffset LastAccessed { get; set; }

        public DateTimeOffset LastRequestedUpstream { get; set; }

        public SearchResult Item { get; set; }
    }
}