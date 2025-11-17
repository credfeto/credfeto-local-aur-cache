using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Credfeto.Aur.Mirror.Models.AurRpc;

[DebuggerDisplay("ID: {Id} Name: {Name} LastModified: {LastModified} Version: {Version}")]
public sealed class SearchResult
{
    [JsonConstructor]
    public SearchResult(string description,
                        long firstSubmitted,
                        int id,
                        IReadOnlyList<string>? keywords,
                        IReadOnlyList<string>? license,
                        IReadOnlyList<string>? depends,
                        IReadOnlyList<string>? makeDepends,
                        IReadOnlyList<string>? optDepends,
                        IReadOnlyList<string>? checkDepends,
                        IReadOnlyList<string>? conflicts,
                        IReadOnlyList<string>? replaces,
                        IReadOnlyList<string>? groups,
                        IReadOnlyList<string>? coMaintainers,
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
                        string version)
    {
        this.Description = description;
        this.FirstSubmitted = firstSubmitted;
        this.Id = id;
        this.Keywords = keywords;
        this.License = license;
        this.Depends = depends;
        this.MakeDepends = makeDepends;
        this.OptDepends = optDepends;
        this.CheckDepends = checkDepends;
        this.Conflicts = conflicts;
        this.Replaces = replaces;
        this.Groups = groups;
        this.CoMaintainers = coMaintainers;
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? Keywords { get; }

    [JsonPropertyName("License")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? License { get; }

    [JsonPropertyName("Depends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? Depends { get; }

    [JsonPropertyName("MakeDepends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? MakeDepends { get; }

    [JsonPropertyName("OptDepends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? OptDepends { get; }

    [JsonPropertyName("CheckDepends")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? CheckDepends { get; }

    [JsonPropertyName("Conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? Conflicts { get; }

    [JsonPropertyName("Replaces")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? Replaces { get; }

    [JsonPropertyName("Groups")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? Groups { get; }

    [JsonPropertyName("CoMaintainers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyList<string>? CoMaintainers { get; }

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

    [SuppressMessage(category: "Microsoft.Naming", checkId: "CA1056: Uri properties should not be strings", Justification = "RPC Property")]
    [JsonPropertyName("URL")]
    public string Url { get; }

    [SuppressMessage(category: "Microsoft.Naming", checkId: "CA1056: Uri properties should not be strings", Justification = "RPC Property")]
    [JsonPropertyName("URLPath")]
    public string UrlPath { get; }

    [JsonPropertyName("Version")]
    public string Version { get; }
}