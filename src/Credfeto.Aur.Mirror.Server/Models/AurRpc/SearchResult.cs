using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Credfeto.Aur.Mirror.Server.Models.AurRpc;

[DebuggerDisplay("ID: {Id} Name: {Name} LastModified: {LastModified} Version: {Version}")]
public sealed class SearchResult
{
    [JsonConstructor]
    public SearchResult(
        string description,
        long firstSubmitted,
        int id,
        IReadOnlyList<string>? keywords,
        IReadOnlyList<string>? license,
        long lastModified,
        string maintainer,
        string name,
        int numVotes,
        long? outOfDate,
        string packageBase,
        int packageBaseId,
        double popularity,
        string url,
        string urlPath,
        string version
    )
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

    [JsonPropertyName("Description")]
    public string Description { get; }

    [JsonPropertyName("FirstSubmitted")]
    public long FirstSubmitted { get; }

    [JsonPropertyName("ID")]
    public int Id { get; }

    [JsonPropertyName("Keywords")]
    public IReadOnlyList<string>? Keywords { get; }

    [JsonPropertyName("License")]
    public IReadOnlyList<string>? License { get; }

    [JsonPropertyName("LastModified")]
    public long LastModified { get; }

    [JsonPropertyName("Maintainer")]
    public string Maintainer { get; }

    [JsonPropertyName("Name")]
    public string Name { get; }

    [JsonPropertyName("NumVotes")]
    public int NumVotes { get; }

    [JsonPropertyName("OutOfDate")]
    public long? OutOfDate { get; }

    [JsonPropertyName("PackageBase")]
    public string PackageBase { get; }

    [JsonPropertyName("PackageBaseID")]
    public int PackageBaseId { get; }

    [JsonPropertyName("Popularity")]
    public double Popularity { get; }

    [SuppressMessage(
        category: "Microsoft.Naming",
        checkId: "CA1056: Uri properties should not be strings",
        Justification = "RPC Property"
    )]
    [JsonPropertyName("URL")]
    public string Url { get; }

    [SuppressMessage(
        category: "Microsoft.Naming",
        checkId: "CA1056: Uri properties should not be strings",
        Justification = "RPC Property"
    )]
    [JsonPropertyName("URLPath")]
    public string UrlPath { get; }

    [JsonPropertyName("Version")]
    public string Version { get; }
}
