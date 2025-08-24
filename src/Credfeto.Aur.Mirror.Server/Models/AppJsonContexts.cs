using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Server.Models;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    IncludeFields = false
)]
[JsonSerializable(typeof(PongDto))]
[JsonSerializable(typeof(RpcResponse))]
[JsonSerializable(typeof(SearchResult))]
public sealed partial class AppJsonContexts : JsonSerializerContext;
