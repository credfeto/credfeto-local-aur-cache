using System;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Interfaces;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.Aur.Mirror.Git.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure) { }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services
            .AddSingleton<TimeProvider>(MockDateTimeSources.Past)
            .AddMockedService<IBackgroundMetadataUpdater>()
            .AddMockedService<ILocalAurMetadata>()
            .AddGitRepos();
    }

    [Fact]
    public void LocallyInstalledMustBeRegistered()
    {
        this.RequireService<ILocallyInstalled>();
    }

    [Fact]
    public void GitServerMustBeRegistered()
    {
        this.RequireService<IGitServer>();
    }

    [Fact]
    public void UpdateLockMustBeRegistered()
    {
        this.RequireService<IUpdateLock>();
    }
}
