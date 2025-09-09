using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IUpdateLock
{
    ValueTask<SemaphoreSlim> GetLockAsync(string fileName, CancellationToken cancellationToken);
}