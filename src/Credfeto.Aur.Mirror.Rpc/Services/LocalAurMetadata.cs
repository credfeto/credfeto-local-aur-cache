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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class LocalAurMetadata : ILocalAurMetadata
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly ILogger<LocalAurMetadata> _logger;
    private readonly ConcurrentDictionary<string, SearchResult> _metadata;
    private readonly ServerConfig _serverConfig;
    private readonly IUpdateLock _updateLock;


    public LocalAurMetadata(IOptions<ServerConfig> config, IUpdateLock updateLock,ILogger<LocalAurMetadata> logger)
    {
        this._updateLock = updateLock;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._metadata = new(StringComparer.OrdinalIgnoreCase);
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

            _ = this._metadata.TryAdd(key: existing.Name, value: existing);
        }
    }

    public ValueTask<IReadOnlyList<SearchResult>> SearchAsync(Func<SearchResult, bool> predicate, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchResult> results = [.. this._metadata.Values.Where(predicate)];

        return ValueTask.FromResult(results);
    }

    public SearchResult? Get(string packageName)
    {
        if (this._metadata.TryGetValue(key: packageName, out SearchResult? result))
        {
            return result;
        }

        return null;
    }

    public async ValueTask UpdateAsync(IReadOnlyList<SearchResult> items)
    {
        foreach (SearchResult item in items)
        {
            bool save = false;

            if (this._metadata.TryGetValue(key: item.Name, out SearchResult? existing))
            {
                if (existing.LastModified < item.LastModified)
                {
                    // Should make this atomic
                    _ = this._metadata.TryRemove(key: item.Name, value: out _);
                    _ = this._metadata.TryAdd(key: item.Name, value: item);
                    save = true;
                }
            }
            else
            {
                _ = this._metadata.TryAdd(key: item.Name, value: item);
                save = true;
            }

            if (save)
            {
                string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{item.Id}.json");
                await this.SavePackageToMetadataAsync(package: item, metadataFileName: metadataFileName, cancellationToken: DoNotCancelEarly);
            }
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
}