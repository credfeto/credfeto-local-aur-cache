using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface IAurMetadataGz
{
    ValueTask<byte[]?> GetPackagesAsync(ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<SearchResult>> SearchAsync(string keyword, string by, CancellationToken cancellationToken);

    ValueTask TriggerRefreshIfNewerAsync(long lastModifiedUnixTimestamp, CancellationToken cancellationToken);
}
