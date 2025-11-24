using System;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Interfaces;
using NonBlocking;

namespace Credfeto.Aur.Mirror.Git.Services;

public sealed class UpdateLock : IUpdateLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

    public UpdateLock()
    {
        this._locks = new(StringComparer.Ordinal);
    }

    public async ValueTask<SemaphoreSlim> GetLockAsync(string fileName, CancellationToken cancellationToken)
    {
        if (this._locks.TryGetValue(key: fileName, out SemaphoreSlim? semaphore))
        {
            await semaphore.WaitAsync(cancellationToken);

            return semaphore;
        }

        semaphore = this._locks.GetOrAdd(key: fileName, new SemaphoreSlim(1));
        await semaphore.WaitAsync(cancellationToken);

        return semaphore;
    }
}
