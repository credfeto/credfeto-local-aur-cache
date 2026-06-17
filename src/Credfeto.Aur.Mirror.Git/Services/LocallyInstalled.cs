using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Models.Cache;

namespace Credfeto.Aur.Mirror.Git.Services;

public sealed class LocallyInstalled : ILocallyInstalled
{
    private readonly IBackgroundMetadataUpdater _backgroundMetadataUpdater;
    private readonly ILocalAurMetadata _localAurMetadata;
    private readonly TimeProvider _timeProvider;

    public LocallyInstalled(
        TimeProvider timeProvider,
        IBackgroundMetadataUpdater backgroundMetadataUpdater,
        ILocalAurMetadata localAurMetadata
    )
    {
        this._timeProvider = timeProvider;
        this._backgroundMetadataUpdater = backgroundMetadataUpdater;
        this._localAurMetadata = localAurMetadata;
    }

    public ValueTask MarkAsClonedAsync(string packageName, CancellationToken cancellationToken)
    {
        DateTimeOffset whenCloned = this._timeProvider.GetUtcNow();

        return this._backgroundMetadataUpdater.RequestUpdateAsync(
            packageName: packageName,
            update: p => p.LastCloned = whenCloned,
            cancellationToken: cancellationToken
        );
    }

    public async ValueTask<IReadOnlyList<RepoCloneInfo>> GetRecentlyClonedAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Package> packages = await this._localAurMetadata.SearchAsync(
            predicate: x => x.LastCloned is not null,
            cancellationToken: cancellationToken
        );

        return [.. packages.Select(BuildRepoEntry)];
    }

    private static RepoCloneInfo BuildRepoEntry(Package package)
    {
        // ! LastCloned is not null here — we filtered for packages where LastCloned is not null
        return new(
            Repo: package.PackageName,
            LastCloned: package.LastCloned!.Value,
            LastAccessed: package.LastAccessed,
            LastModifiedUpstream: package.LastModified
        );
    }
}
