using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Cache.Interfaces;

[DebuggerDisplay("{PackageName}: Modified {LastModified} Requested {LastRequestedUpstream} Accessed {LastAccessed} Saved {LastSaved} Cloned: {LastCloned}")]
public sealed class Package
{
    [JsonConstructor]
    public Package(in DateTimeOffset lastSaved, in DateTimeOffset lastAccessed, in DateTimeOffset lastRequestedUpstream, SearchResult searchResult, DateTimeOffset? lastCloned)
    {
        this.LastSaved = lastSaved;
        this.LastAccessed = lastAccessed;
        this.LastRequestedUpstream = lastRequestedUpstream;
        this.SearchResult = searchResult;
        this.LastCloned = lastCloned;
    }

    public string PackageName => this.SearchResult.Name;

    public DateTimeOffset LastSaved { get; set; }

    public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds(this.SearchResult.LastModified);

    public DateTimeOffset LastAccessed { get; set; }

    public DateTimeOffset LastRequestedUpstream { get; set; }

    public SearchResult SearchResult { get; private set; }

    public DateTimeOffset? LastCloned { get; set;  }

    public void Update(SearchResult searchResult, in DateTimeOffset lastAccessed)
    {
        this.SearchResult = searchResult;
        this.LastAccessed = lastAccessed;
        this.LastRequestedUpstream = lastAccessed;
    }
}