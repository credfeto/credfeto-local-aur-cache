using System.Collections.Generic;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Models;

public sealed class SearchTracking
{
    private readonly List<SearchResult> _toSave;

    public SearchTracking()
    {
        this._toSave = [];
    }

    public IReadOnlyList<SearchResult> ToSave => this._toSave;

    public void AppendRepoSyncSearchResult(SearchResult package)
    {
        this._toSave.Add(package);
    }
}