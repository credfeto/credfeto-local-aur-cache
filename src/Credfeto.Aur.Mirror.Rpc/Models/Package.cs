using System;
using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Models;

internal sealed class Package
{
    [JsonConstructor]
    public Package(in DateTimeOffset lastSaved, in DateTimeOffset lastAccessed, in DateTimeOffset lastRequestedUpstream, SearchResult searchResult)
    {
        this.LastSaved = lastSaved;
        this.LastAccessed = lastAccessed;
        this.LastRequestedUpstream = lastRequestedUpstream;
        this.SearchResult = searchResult;
    }

    public string PackageName => this.SearchResult.Name;

    public DateTimeOffset LastSaved { get; set; }

    public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeSeconds(this.SearchResult.LastModified);

    public DateTimeOffset LastAccessed { get; set; }

    public DateTimeOffset LastRequestedUpstream { get; set; }

    public SearchResult SearchResult { get; set; }
}