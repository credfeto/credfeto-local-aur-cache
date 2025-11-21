using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Cache.Services;
using Credfeto.Aur.Mirror.Cache.Startup;
using Credfeto.Services.Startup.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.Aur.Mirror.Cache;

public static class CacheSetup
{
    public static IServiceCollection AddMetadataCache(this IServiceCollection services)
    {
        return services.AddSingleton<ILocalAurMetadata, LocalAurMetadata>()
                       .AddSingleton<IBackgroundMetadataUpdater, BackgroundMetadataUpdater>()
                       .AddRunOnStartupTask<LoadCachedMetadata>();
    }
}