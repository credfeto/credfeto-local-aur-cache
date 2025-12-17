using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Cache.Interfaces;

public interface ILocalAurMetadata
{
    ValueTask LoadAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<Package>> SearchAsync(Func<Package, bool> predicate, CancellationToken cancellationToken);

    Package? Get(string packageName);

    ValueTask UpdateAsync(
        SearchResult package,
        Func<SearchResult, bool, ValueTask> onUpdate,
        CancellationToken cancellationToken
    );

    ValueTask UpdateAsync(Package package, Action<Package> onUpdate, CancellationToken cancellationToken);
}
