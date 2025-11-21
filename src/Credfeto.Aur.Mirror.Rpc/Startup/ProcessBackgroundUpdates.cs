using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Services.Startup.Interfaces;

namespace Credfeto.Aur.Mirror.Rpc.Startup;

public sealed class ProcessBackgroundUpdates : IRunOnStartup, IDisposable
{
    private readonly IAurRpc _aurRpc;
    private readonly IBackgroundMetadataUpdater _backgroundMetadataUpdater;
    private readonly ILocalAurMetadata _localAurMetadata;
    private IDisposable? _subscription;

    public ProcessBackgroundUpdates(IBackgroundMetadataUpdater backgroundMetadataUpdater, IAurRpc aurRpc, ILocalAurMetadata localAurMetadata)
    {
        this._backgroundMetadataUpdater = backgroundMetadataUpdater;
        this._aurRpc = aurRpc;
        this._localAurMetadata = localAurMetadata;
    }

    public void Dispose()
    {
        this._subscription?.Dispose();
    }

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        this._subscription = this._backgroundMetadataUpdater.GetAsync(cancellationToken)
                                 .ToObservable()
                                 .Select(request => Observable.FromAsync(ct => this.RequestCacheUpdateAsync(request: request, cancellationToken: ct)
                                                                                   .AsTask()))
                                 .Concat()
                                 .Subscribe();

        return ValueTask.CompletedTask;
    }

    private async ValueTask RequestCacheUpdateAsync(PackageRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> packages = [request.PackageName];
        RpcResponse search = await this._aurRpc.InfoAsync(packages: packages, userAgent: null, cancellationToken: cancellationToken);

        foreach (SearchResult result in search.Results)
        {
            Package? cached = this._localAurMetadata.Get(result.Name);

            if (cached is not null)
            {
                await this._localAurMetadata.UpdateAsync(package: cached, onUpdate: request.Update, cancellationToken: cancellationToken);
            }
        }
    }
}