using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services;
using Credfeto.Aur.Mirror.Rpc.Startup;
using Credfeto.Services.Startup.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.Aur.Mirror.Rpc;

public static class RpcSetup
{
    public static IServiceCollection AddAurRpcApi(this IServiceCollection services)
    {
        return services.AddRpcClient()
                       .AddReposClient()
                       .AddSingleton<IAurRpc, AurRpc>()
                       .AddSingleton<IAurRepos, AurRepos>()
                       .AddSingleton<ILocalAurRpc, LocalAurRpc>()
                       .AddSingleton<IRemoteAurRpc, RemoteAurRpc>()
                       .AddRunOnStartupTask<ProcessBackgroundUpdates>();
    }
}