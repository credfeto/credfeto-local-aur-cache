using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Git.Services.LoggingExtensions;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.Cache;
using Credfeto.Date.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Git.Services;

public sealed class LocallyInstalled : ILocallyInstalled
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _cloned;
    private readonly ICurrentTimeSource _dateTimeSource;
    private readonly ILogger<LocallyInstalled> _logger;
    private readonly ServerConfig _serverConfig;

    public LocallyInstalled(
        IOptions<ServerConfig> config,
        ICurrentTimeSource dateTimeSource,
        ILogger<LocallyInstalled> logger
    )
    {
        this._dateTimeSource = dateTimeSource;
        this._logger = logger;
        this._serverConfig = config.Value;

        this._cloned = new(StringComparer.OrdinalIgnoreCase);
    }

    public async ValueTask MarkAsClonedAsync(string repo, CancellationToken cancellationToken)
    {
        DateTimeOffset whenCloned = this._dateTimeSource.UtcNow();

        bool exists = this._cloned.TryRemove(key: repo, value: out _);

        if (this._cloned.TryAdd(key: repo, value: whenCloned))
        {
            if (exists)
            {
                this._logger.UpdatingCloneCache(repo: repo, timestamp: whenCloned);
            }
            else
            {
                this._logger.AddingToCloneCache(repo: repo, timestamp: whenCloned);
            }
        }

        string fileName = Path.Combine(path1: this._serverConfig.Storage.Repos, $"{repo}.cloned");

        await File.WriteAllTextAsync(
            path: fileName,
            contents: "{}",
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask<IReadOnlyList<RepoCloneInfo>> GetRecentlyClonedAsync(CancellationToken cancellationToken)
    {
        IEnumerable<string> files = Directory.EnumerateFiles(
            path: this._serverConfig.Storage.Repos,
            searchPattern: "*.cloned"
        );

        IReadOnlyList<RepoCloneInfo> cloneInfo =
        [
            .. files
                .Select(file =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string repo = Path.GetFileNameWithoutExtension(file);

                    return this.BuildRepoEntry(repo: repo, fileName: file);
                })
                .OrderBy(x => x.LastCloned),
        ];

        return ValueTask.FromResult(cloneInfo);
    }

    private RepoCloneInfo BuildRepoEntry(string repo, string fileName)
    {
        if (!this._cloned.TryGetValue(key: repo, out DateTimeOffset lastCloned))
        {
            lastCloned = File.GetLastWriteTimeUtc(fileName).AsDateTimeOffset();

            if (this._cloned.TryAdd(key: repo, value: lastCloned))
            {
                this._logger.AddingToCloneCache(repo: repo, timestamp: lastCloned);
            }
        }

        return new(repo: repo, lastCloned: lastCloned);
    }
}
