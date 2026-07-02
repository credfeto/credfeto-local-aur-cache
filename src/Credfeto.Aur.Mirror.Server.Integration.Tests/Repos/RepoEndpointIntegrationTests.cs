using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Git.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Server.Integration.Tests.Repos;

public sealed class RepoEndpointIntegrationTests : IntegrationTestBase
{
    public RepoEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("/repos/test-repo.git/objects/pack/test-file")]
    [InlineData("/repos/test-repo.git/info/refs")]
    public async Task GetAsync_WhenGetFileAsyncReturnsNull_ShouldReturnNotFound(string requestUri)
    {
        IGitServer gitServer = GetSubstitute<IGitServer>();

        await using TestServerApplication factory = new(services => services.AddSingleton(gitServer));

        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            requestUri: new Uri(uriString: requestUri, uriKind: UriKind.Relative),
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);

        await gitServer
            .Received(1)
            .GetFileAsync(
                repoName: "test-repo",
                path: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }
}
