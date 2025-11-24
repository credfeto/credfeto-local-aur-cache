using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Models;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface ILocalAurMetadata
{
    ValueTask LoadAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<Package>> SearchAsync(
        Func<SearchResult, bool> predicate,
        CancellationToken cancellationToken
    );

    Package? Get(string packageName);

    ValueTask UpdateAsync(
        SearchResult package,
        Func<SearchResult, bool, ValueTask> onUpdate,
        CancellationToken cancellationToken
    );
}
