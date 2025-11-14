using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Date.Interfaces;
using FunFair.Test.Common;
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
               .AddMockedService<ICurrentTimeSource>()
            .AddGitRepos();
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
