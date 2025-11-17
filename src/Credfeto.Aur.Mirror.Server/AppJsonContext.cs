using System.Collections.Generic;
using System.Text.Json.Serialization;
using Credfeto.Aur.Mirror.Models;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Models.Cache;

namespace Credfeto.Aur.Mirror.Server;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
                             PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             WriteIndented = false,
                             IncludeFields = false)]
[JsonSerializable(typeof(PongDto))]
[JsonSerializable(typeof(RpcResponse))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(RepoCloneInfo))]
[JsonSerializable(typeof(IReadOnlyList<RepoCloneInfo>))]
internal sealed partial class AppJsonContext : JsonSerializerContext;