using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Aur.Mirror.Cache.Interfaces;

public interface IBackgroundMetadataUpdater
{
    IAsyncEnumerable<PackageRequest> GetAsync(CancellationToken cancellationToken);

    ValueTask RequestUpdateAsync(string packageName, Action<Package> update, CancellationToken cancellationToken);
}