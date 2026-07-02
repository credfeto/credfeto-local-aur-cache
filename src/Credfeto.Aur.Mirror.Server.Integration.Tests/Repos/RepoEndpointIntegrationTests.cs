using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Git.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.Aur.Mirror.Server.Integration.Tests.Repos;

public sealed class RepoEndpointIntegrationTests : IntegrationTestBase
{
    public RepoEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task RetrieveFileCommonAsync_WhenGetFileAsyncReturnsNull_ShouldReturnNotFound()
    {
        IGitServer gitServer = GetSubstitute<IGitServer>();

        await using TestServerApplication factory = new(services => services.AddSingleton(gitServer));

        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            requestUri: new Uri(uriString: "/repos/test-repo.git/objects/pack/test-file", uriKind: UriKind.Relative),
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task GitInfoRefsAsync_WhenGetFileAsyncReturnsNullAndNoServiceParam_ShouldReturnNotFound()
    {
        IGitServer gitServer = GetSubstitute<IGitServer>();

        await using TestServerApplication factory = new(services => services.AddSingleton(gitServer));

        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            requestUri: new Uri(uriString: "/repos/test-repo.git/info/refs", uriKind: UriKind.Relative),
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }
}
