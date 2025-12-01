using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Services.Startup.Interfaces;

namespace Credfeto.Aur.Mirror.Cache.Startup;

public sealed class LoadCachedMetadata : IRunOnStartup
{
    private readonly ILocalAurMetadata _localAurMetadata;

    public LoadCachedMetadata(ILocalAurMetadata localAurMetadata)
    {
        this._localAurMetadata = localAurMetadata;
    }

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        return this._localAurMetadata.LoadAsync(cancellationToken: cancellationToken);
    }
}
