using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using FunFair.Test.Common;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Server.Integration.Tests.Repos;

public sealed class RepoEndpointIntegrationTests : IntegrationTestBase
{
    public RepoEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("/repos/test-repo.git/objects/pack/test-file", "objects" + "/" + "pack" + "/" + "test-file")]
    [InlineData("/repos/test-repo.git/info/refs", "info/refs")]
    public async Task GetAsync_WhenGetFileAsyncReturnsNull_ShouldReturnNotFound(string requestUri, string expectedPath)
    {
        IGitServer gitServer = GetSubstitute<IGitServer>();
        ILocalAurMetadata localAurMetadata = GetSubstitute<ILocalAurMetadata>();
        IAurRepos aurRepos = GetSubstitute<IAurRepos>();
        IAurMetadataGz aurMetadataGz = GetSubstitute<IAurMetadataGz>();

        await using TestServerApplication factory = new(
            gitServer: gitServer,
            localAurMetadata: localAurMetadata,
            aurRepos: aurRepos,
            aurMetadataGz: aurMetadataGz
        );

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
                path: expectedPath.Replace(oldChar: '/', newChar: Path.DirectorySeparatorChar),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }
}
