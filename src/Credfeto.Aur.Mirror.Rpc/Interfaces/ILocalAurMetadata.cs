using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface ILocalAurMetadata
{
    ValueTask LoadAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<SearchResult>> SearchAsync(Func<SearchResult, bool> predicate, CancellationToken cancellationToken);

    SearchResult? Get(string packageName);

    ValueTask UpdateAsync(IReadOnlyList<SearchResult> items);
}