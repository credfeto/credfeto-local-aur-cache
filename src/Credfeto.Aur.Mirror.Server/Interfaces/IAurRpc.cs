using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Models.AurRpc;
using Microsoft.Extensions.Primitives;

namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IAurRpc
{
    ValueTask<RpcResponse> SearchAsync(
        IReadOnlyDictionary<string, StringValues> query,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    );

    ValueTask<RpcResponse> InfoAsync(
        IReadOnlyDictionary<string, StringValues> query,
        ProductInfoHeaderValue? userAgent,
        CancellationToken cancellationToken
    );

}
