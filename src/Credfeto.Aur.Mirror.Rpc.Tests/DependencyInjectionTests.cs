using Credfeto.Aur.Mirror.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.Aur.Mirror.Rpc.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure)
    {
    }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services.AddMockedService<IGitServer>()
                       .AddMockedService<IUpdateLock>()
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
}