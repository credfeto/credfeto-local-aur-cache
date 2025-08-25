using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IAurRpc
{
    ValueTask<RpcResponse> GetAsync(
        string query,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    );
}
