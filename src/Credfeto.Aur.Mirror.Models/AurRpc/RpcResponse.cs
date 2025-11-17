using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Credfeto.Aur.Mirror.Models.AurRpc;

[DebuggerDisplay("RpcType: {RpcType} Count: {Count} Version: {Version}")]
public sealed class RpcResponse
{
    [JsonConstructor]
    public RpcResponse(int count, IReadOnlyList<SearchResult> results, string rpcType, int version)
    {
        this.Count = count;
        this.Results = results;
        this.RpcType = rpcType;
        this.Version = version;
    }

    [JsonPropertyName("resultcount")]
    public int Count { get; }

    [JsonPropertyName("results")]
    public IReadOnlyList<SearchResult> Results { get; }

    [JsonPropertyName("type")]
    public string RpcType { get; }

    [JsonPropertyName("version")]
    public int Version { get; }
}