using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Aur.Mirror.Rpc.Interfaces;

public interface IAurRepos
{
    ValueTask<byte[]?> GetPackagesAsync(ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken);
}
