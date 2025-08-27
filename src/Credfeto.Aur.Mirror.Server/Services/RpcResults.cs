using Credfeto.Aur.Mirror.Server.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Server.Services;

internal static class RpcResults
{
    public const int RpcVersion = 5;

    public static RpcResponse SearchNotFound { get; } = new(count: 0, [], rpcType: "search", version: RpcVersion);

    public static RpcResponse InfoNotFound { get; } = new(count: 0, [], rpcType: "multiinfo", version: RpcVersion);
}