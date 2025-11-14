using System.Collections.Generic;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Models;

public sealed class SearchTracking
{
    public SearchTracking()
    {
        this.ToSave = [];
    }

    public List<SearchResult> ToSave { get; }

    public void AppendRepoSyncSearchResult(SearchResult package)
    {
        this.ToSave.Add(package);
    }
}