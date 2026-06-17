using System;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.Aur.Mirror.Rpc.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure) { }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services
            .AddSingleton<TimeProvider>(MockDateTimeSources.Past)
            .AddMockedService<IGitServer>()
            .AddMockedService<IUpdateLock>()
            .AddMockedService<ILocalAurMetadata>()
            .AddAurRpcApi();
    }

    [Fact]
    public void AurRpcMustBeRegistered()
    {
        this.RequireService<IAurRpc>();
    }

    [Fact]
    public void AurReposMustBeRegistered()
    {
        this.RequireService<IAurRepos>();
    }

    [Fact]
    public void AurMetadataGzMustBeRegistered()
    {
        this.RequireService<IAurMetadataGz>();
    }
}
