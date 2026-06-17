using System;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Services.Startup.Interfaces;

namespace Credfeto.Aur.Mirror.Rpc.Startup;

public sealed class RefreshMetadataGzPeriodically : IRunOnStartup
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    private readonly IAurMetadataGz _aurMetadataGz;

    public RefreshMetadataGzPeriodically(IAurMetadataGz aurMetadataGz)
    {
        this._aurMetadataGz = aurMetadataGz;
    }

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => this.PeriodicRefreshAsync(cancellationToken), cancellationToken: CancellationToken.None);

        return ValueTask.CompletedTask;
    }

    private async Task PeriodicRefreshAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(period: RefreshInterval);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await this._aurMetadataGz.GetPackagesAsync(userAgent: null, cancellationToken: CancellationToken.None);
        }
    }
}
