using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface IGitHubMirrorRpc
{
    ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, CancellationToken cancellationToken);
}
