using System;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Credfeto.Aur.Mirror.Server.Integration.Tests;

internal sealed class TestServerApplication : WebApplicationFactory<ServerEntryPoint>
{
    private readonly Action<IServiceCollection> _configureTestServices;

    internal TestServerApplication(Action<IServiceCollection> configureTestServices)
    {
        this._configureTestServices = configureTestServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(Substitute.For<ILocalAurMetadata>());

            this._configureTestServices(services);
        });
    }
}
