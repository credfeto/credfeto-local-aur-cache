using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Models;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Credfeto.Date.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class LocalAurMetadata : ILocalAurMetadata, IDisposable
{
    private static readonly CancellationToken DoNotCancelEarly = CancellationToken.None;
    private readonly ICurrentTimeSource _currentTimeSource;
    private readonly ILogger<LocalAurMetadata> _logger;
    private readonly ConcurrentDictionary<string, Package> _metadata;
    private readonly Channel<Package> _saveQueue;
    private readonly ServerConfig _serverConfig;
    private readonly IDisposable _subscription;
    private readonly IUpdateLock _updateLock;

    public LocalAurMetadata(IOptions<ServerConfig> config, IUpdateLock updateLock, ICurrentTimeSource currentTimeSource, ILogger<LocalAurMetadata> logger)
    {
        this._updateLock = updateLock;
        this._currentTimeSource = currentTimeSource;
        this._logger = logger;
        this._serverConfig = config.Value;
        this._metadata = new(StringComparer.OrdinalIgnoreCase);
        this._saveQueue = Channel.CreateUnbounded<Package>();

        this._subscription = this.SubscribeToPackageSaveQueue();

        // TASK: Store local config in a DB that's quick to search rather than filesystem
    }

    public void Dispose()
    {
        this._subscription.Dispose();
    }

    public async ValueTask LoadAsync(CancellationToken cancellationToken)
    {
        await foreach (Package package in this.GetMetadataAsync(cancellationToken))
        {
            _ = this._metadata.TryAdd(key: package.PackageName, value: package);
        }
    }

    public async ValueTask<IReadOnlyList<Package>> SearchAsync(Func<SearchResult, bool> predicate, CancellationToken cancellationToken)
    {
        return await Task.WhenAll(this._metadata.Values.Where(item => predicate(item.SearchResult))
                                      .Select(QueueUpdateAndReturnAsync));

        async Task<Package> QueueUpdateAndReturnAsync(Package item)
        {
            item.LastAccessed = this._currentTimeSource.UtcNow();

            await this.QueueUpdateAsync(packageToSave: item, cancellationToken: cancellationToken);

            return item;
        }
    }

    public Package? Get(string packageName)
    {
        if (!this._metadata.TryGetValue(key: packageName, out Package? result))
        {
            return null;
        }

        result.LastAccessed = this._currentTimeSource.UtcNow();

        return result;
    }

    public async ValueTask UpdateAsync(SearchResult package, Func<SearchResult, bool, ValueTask> onUpdate, CancellationToken cancellationToken)
    {
        Package toSave = this.ShouldIssueUpdate(package: package, out bool issueUpdate, out bool changed);

        await this.QueueUpdateAsync(packageToSave: toSave, cancellationToken: cancellationToken);

        if (issueUpdate)
        {
            await onUpdate(arg1: package, arg2: changed);
        }
    }

    private IDisposable SubscribeToPackageSaveQueue()
    {
        return this._saveQueue.Reader.ReadAllAsync(DoNotCancelEarly)
                   .ToObservable()
                   .Select(package => Observable.FromAsync(cancellationToken => this.SavePackageToMetadataAsync(package: package, cancellationToken: cancellationToken)
                                                                                    .AsTask()))
                   .Concat()
                   .Subscribe();
    }

    private Package ShouldIssueUpdate(SearchResult package, out bool issueUpdate, out bool changed)
    {
        if (this._metadata.TryGetValue(key: package.Name, out Package? existing))
        {
            return this.OnPackageChanged(candidate: package, existing: existing, issueUpdate: out issueUpdate, changed: out changed);
        }

        Package toSave = this.Wrap(package);
        Package current = this._metadata.GetOrAdd(key: package.Name, value: toSave);

        if (ReferenceEquals(objA: current, objB: toSave))
        {
            changed = false;
            issueUpdate = true;

            return current;
        }

        return this.OnPackageChanged(candidate: package, existing: current, issueUpdate: out issueUpdate, changed: out changed);
    }

    private Package Wrap(SearchResult package)
    {
        DateTimeOffset now = this._currentTimeSource.UtcNow();

        return new(lastSaved: now, lastAccessed: now, lastRequestedUpstream: now, searchResult: package);
    }

    private Package OnPackageChanged(SearchResult candidate, Package existing, out bool issueUpdate, out bool changed)
    {
        DateTimeOffset now = this._currentTimeSource.UtcNow();
        existing.LastRequestedUpstream = now;
        bool modified = existing.SearchResult.LastModified < candidate.LastModified;

        if (modified)
        {
            existing.Update(searchResult: candidate, lastAccessed: now);
        }

        changed = modified;
        issueUpdate = modified;

        return existing;
    }

    private ValueTask QueueUpdateAsync(Package packageToSave, in CancellationToken cancellationToken)
    {
        // queue the save to the disk - the important cache update where things have been written has already occurred here.
        return this._saveQueue.Writer.WriteAsync(item: packageToSave, cancellationToken: cancellationToken);
    }

    private async ValueTask SavePackageToMetadataAsync(Package package, CancellationToken cancellationToken)
    {
        string metadataFileName = Path.Combine(path1: this._serverConfig.Storage.Metadata, $"{package.PackageName}.json");
        SemaphoreSlim wait = await this._updateLock.GetLockAsync(fileName: metadataFileName, cancellationToken: cancellationToken);

        try
        {
            EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

            package.LastSaved = this._currentTimeSource.UtcNow();
            string json = JsonSerializer.Serialize(value: package, jsonTypeInfo: RpcJsonContext.Default.Package);
            await File.WriteAllTextAsync(path: metadataFileName, contents: json, encoding: Encoding.UTF8, cancellationToken: DoNotCancelEarly);
        }
        catch (Exception exception)
        {
            this._logger.SaveMetadataFailed(filename: metadataFileName, message: exception.Message, exception: exception);
        }
        finally
        {
            _ = wait.Release();
        }
    }

    private async ValueTask<Package?> ReadPackageFromMetadataAsync(string metadataFileName)
    {
        try
        {
            string json = await File.ReadAllTextAsync(path: metadataFileName, encoding: Encoding.UTF8, cancellationToken: DoNotCancelEarly);

            return JsonSerializer.Deserialize(json: json, jsonTypeInfo: RpcJsonContext.Default.Package);
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
            _ = Directory.CreateDirectory(directory);
        }
    }

    private async IAsyncEnumerable<Package> GetMetadataAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        EnsureDirectoryExists(this._serverConfig.Storage.Metadata);

        foreach (string metadataFileName in Directory.EnumerateFiles(path: this._serverConfig.Storage.Metadata, searchPattern: "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Package? existing = await this.ReadPackageFromMetadataAsync(metadataFileName);

            if (existing is null)
            {
                continue;
            }

            yield return existing;
        }
    }
}