using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Cache;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
                             PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             WriteIndented = false,
                             IncludeFields = false)]
[JsonSerializable(typeof(Package))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(RpcResponse))]
internal sealed partial class CacheJsonContext : JsonSerializerContext;