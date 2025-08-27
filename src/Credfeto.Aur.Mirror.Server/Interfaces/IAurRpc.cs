using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IAurRpc
{
    ValueTask<RpcResponse> SearchAsync(
        string keyword,
        string by,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    );

    ValueTask<RpcResponse> InfoAsync(
        IReadOnlyList<string> packages,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    );
}