using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Git.Services;
using Credfeto.Aur.Mirror.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.Aur.Mirror.Git;

public static class GitSetup
{
    public static IServiceCollection AddGitRepos(this IServiceCollection services)
    {
        return services.AddSingleton<IGitServer, GitServer>()
                       .AddSingleton<ILocallyInstalled, LocallyInstalled>()
                       .AddSingleton<IUpdateLock, UpdateLock>();
    }
}