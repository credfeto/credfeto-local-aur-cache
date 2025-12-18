using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;

namespace Credfeto.Aur.Mirror.Cache.Services;

public sealed class BackgroundMetadataUpdater : IBackgroundMetadataUpdater
{
    private readonly ILocalAurMetadata _localAurMetadata;
    private readonly Channel<PackageRequest> _queue;

    public BackgroundMetadataUpdater(ILocalAurMetadata localAurMetadata)
    {
        this._localAurMetadata = localAurMetadata;
        this._queue = Channel.CreateUnbounded<PackageRequest>();
    }

    public IAsyncEnumerable<PackageRequest> GetAsync(CancellationToken cancellationToken)
    {
        return this._queue.Reader.ReadAllAsync(cancellationToken);
    }

    public async ValueTask RequestUpdateAsync(
        string packageName,
        Action<Package> update,
        CancellationToken cancellationToken
    )
    {
        Package? existing = this._localAurMetadata.Get(packageName);

        if (existing is not null)
        {
            await this._localAurMetadata.UpdateAsync(
                package: existing,
                onUpdate: update,
                cancellationToken: cancellationToken
            );

            return;
        }

        await this._queue.Writer.WriteAsync(
            new(PackageName: packageName, Update: update),
            cancellationToken: cancellationToken
        );
    }
}
