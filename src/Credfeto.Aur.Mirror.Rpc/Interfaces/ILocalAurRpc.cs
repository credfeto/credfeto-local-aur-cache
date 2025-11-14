using System.Net.Http.Headers;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface ILocalAurRpc : IAurRpc
{
    ValueTask SyncUpstreamReposAsync(RpcResponse upstream, ProductInfoHeaderValue? userAgent);
}
