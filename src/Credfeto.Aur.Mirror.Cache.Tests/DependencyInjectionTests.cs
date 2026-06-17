using System;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Cache.Startup;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Services.Startup.Interfaces;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.Aur.Mirror.Cache.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure) { }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services
            .AddSingleton<TimeProvider>(MockDateTimeSources.Past)
            .AddMockedService<IUpdateLock>()
            .AddMetadataCache();
    }

    [Fact]
    public void LocalAurMetadataMustBeRegistered()
    {
        this.RequireService<ILocalAurMetadata>();
    }

    [Fact]
    public void LocalCachedMetadataMustBeRegistered()
    {
        this.RequireServiceInCollectionFor<IRunOnStartup, LoadCachedMetadata>();
    }
}
