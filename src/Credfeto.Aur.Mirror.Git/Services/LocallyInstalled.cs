using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Models.Cache;
using Credfeto.Date.Interfaces;

namespace Credfeto.Aur.Mirror.Git.Services;

public sealed class LocallyInstalled : ILocallyInstalled
{
    private readonly IBackgroundMetadataUpdater _backgroundMetadataUpdater;
    private readonly ICurrentTimeSource _dateTimeSource;
    private readonly ILocalAurMetadata _localAurMetadata;

    public LocallyInstalled(ICurrentTimeSource dateTimeSource,
                            IBackgroundMetadataUpdater backgroundMetadataUpdater,
                            ILocalAurMetadata localAurMetadata)
    {
        this._dateTimeSource = dateTimeSource;
        this._backgroundMetadataUpdater = backgroundMetadataUpdater;
        this._localAurMetadata = localAurMetadata;
    }

    public ValueTask MarkAsClonedAsync(string packageName, CancellationToken cancellationToken)
    {
        DateTimeOffset whenCloned = this._dateTimeSource.UtcNow();

        return this._backgroundMetadataUpdater.RequestUpdateAsync(packageName: packageName, update: p => p.LastCloned = whenCloned, cancellationToken: cancellationToken);
    }

    public async ValueTask<IReadOnlyList<RepoCloneInfo>> GetRecentlyClonedAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Package> packages = await this._localAurMetadata.SearchAsync(predicate: x => x.LastCloned is not null, cancellationToken: cancellationToken);

        return [.. packages.Select(BuildRepoEntry)];
    }

    private static RepoCloneInfo BuildRepoEntry(Package package)
    {
        // ! Last cloned should already be non null at this point
        return new(repo: package.PackageName, lastCloned: package.LastCloned!.Value);
    }
}