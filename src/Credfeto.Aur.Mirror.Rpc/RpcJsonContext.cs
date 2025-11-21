using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
                             PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             WriteIndented = false,
                             IncludeFields = false)]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(RpcResponse))]
internal sealed partial class RpcJsonContext : JsonSerializerContext;