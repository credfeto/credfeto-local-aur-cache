using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.Aur.Mirror.Server.Integration.Tests;

internal sealed class TestServerApplication : WebApplicationFactory<ServerEntryPoint>
{
    private readonly IAurMetadataGz _aurMetadataGz;
    private readonly IAurRepos _aurRepos;
    private readonly IGitServer _gitServer;
    private readonly ILocalAurMetadata _localAurMetadata;

    internal TestServerApplication(
        IGitServer gitServer,
        ILocalAurMetadata localAurMetadata,
        IAurRepos aurRepos,
        IAurMetadataGz aurMetadataGz
    )
    {
        this._gitServer = gitServer;
        this._localAurMetadata = localAurMetadata;
        this._aurRepos = aurRepos;
        this._aurMetadataGz = aurMetadataGz;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(this._gitServer);
            services.AddSingleton(this._localAurMetadata);
            services.AddSingleton(this._aurRepos);
            services.AddSingleton(this._aurMetadataGz);
        });
    }
}
