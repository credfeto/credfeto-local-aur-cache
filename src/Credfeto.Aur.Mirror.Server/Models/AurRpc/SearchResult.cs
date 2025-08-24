using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Credfeto.Aur.Mirror.Server.Models.AurRpc;

[DebuggerDisplay("ID: {Id} Name: {Name} LastModified: {LastModified} Version: {Version}")]
public sealed class SearchResult
{
    [JsonConstructor]
    public SearchResult(string description, long firstSubmitted, int id,
                        IReadOnlyList<string>? keywords,
                        IReadOnlyList<string>? license,

    long lastModified, string maintainer, string name, int numVotes, long? outOfDate, string packageBase, int packageBaseId, double popularity, string url, string urlPath, string version)
    {
        this.Description = description;
        this.FirstSubmitted = firstSubmitted;
        this.Id = id;
        this.Keywords = keywords;
        this.License = license;
        this.LastModified = lastModified;
        this.Maintainer = maintainer;
        this.Name = name;
        this.NumVotes = numVotes;
        this.OutOfDate = outOfDate;
        this.PackageBase = packageBase;
        this.PackageBaseId = packageBaseId;
        this.Popularity = popularity;
        this.Url = url;
        this.UrlPath = urlPath;
        this.Version = version;
    }

    // "Description":"Fast and simple system info written in C, that can be configured at compile time by editing the config.h file",
    [JsonPropertyName("Description")]
    public string Description { get; }

    // "FirstSubmitted":1603171383,
    [JsonPropertyName("FirstSubmitted")]
    public long FirstSubmitted { get; }

    // "ID":849486
    [JsonPropertyName("ID")]
    public int Id { get; }

    [JsonPropertyName("Keywords")]
    public IReadOnlyList<string>? Keywords { get; }

    [JsonPropertyName("License")]
    public IReadOnlyList<string>? License { get; }



    // "LastModified":1610998028
    [JsonPropertyName("LastModified")]
    public long LastModified { get; }

    // "Maintainer":"McFranko"
    [JsonPropertyName("Maintainer")]
    public string Maintainer { get; }

    // "Name":"afetch-git"
    [JsonPropertyName("Name")]
    public string Name { get; }

    // "NumVotes":3
    [JsonPropertyName("NumVotes")]
    public int NumVotes { get; }

    // "OutOfDate":1687880376
    [JsonPropertyName("OutOfDate")]
    public long? OutOfDate { get; }

    // "PackageBase":"afetch-git"
    [JsonPropertyName("PackageBase")]
    public string PackageBase { get; }

    // "PackageBaseID":158933
    [JsonPropertyName("PackageBaseID")]
    public int PackageBaseId { get; }


    // "Popularity":0
    [JsonPropertyName("Popularity")]
    public double Popularity { get; }

    // "URL":"https://github.com/13-CF/afetch"
    [SuppressMessage(
        category: "Microsoft.Naming",
        checkId: "CA1056: Uri properties should not be strings",
        Justification = "RPC Property"
    )]
    [JsonPropertyName("URL")]
    public string Url { get; }

    // "URLPath":"/cgit/aur.git/snapshot/afetch-git.tar.gz"
    [SuppressMessage(
        category: "Microsoft.Naming",
        checkId: "CA1056: Uri properties should not be strings",
        Justification = "RPC Property"
    )]
    [JsonPropertyName("URLPath")]
    public string UrlPath { get; }

    // "Version":"1-1"
    [JsonPropertyName("Version")]
    public string Version { get; }
}