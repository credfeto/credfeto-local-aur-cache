using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Git.Exceptions;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Rpc.Tests;

public sealed class AurRpcTests : LoggingTestBase
{
    private readonly IAurMetadataGz _aurMetadataGz;
    private readonly ILocalAurRpc _localAurRpc;
    private readonly IRemoteAurRpc _remoteAurRpc;
    private readonly AurRpc _sut;

    public AurRpcTests(ITestOutputHelper output)
        : base(output)
    {
        this._remoteAurRpc = GetSubstitute<IRemoteAurRpc>();
        this._localAurRpc = GetSubstitute<ILocalAurRpc>();
        this._aurMetadataGz = GetSubstitute<IAurMetadataGz>();
        ILogger<AurRpc> logger = this.GetTypedLogger<AurRpc>();

        this._sut = new AurRpc(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            timeProvider: MockDateTimeSources.Past,
            logger: logger
        );
    }

    [Fact]
    public async Task SearchAsync_WhenSyncThrowsGitException_ReturnsUpstreamResponseAsync()
    {
        RpcResponse expectedResponse = new(count: 1, [BuildSearchResult()], rpcType: "search", version: 5);

        _ = this
            ._remoteAurRpc.SearchAsync(
                keyword: Arg.Any<string>(),
                by: Arg.Any<string>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(expectedResponse);

        this._localAurRpc.When(async x =>
                await x.SyncUpstreamReposAsync(
                    upstream: Arg.Any<RpcResponse>(),
                    userAgent: Arg.Any<ProductInfoHeaderValue?>()
                )
            )
            .Do(_ => throw new GitException("Temporary git server unavailable"));

        RpcResponse result = await this._sut.SearchAsync(
            keyword: "test",
            by: "name",
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: expectedResponse, actual: result);
    }

    [Fact]
    public async Task InfoAsync_WhenSyncThrowsGitException_ReturnsUpstreamResponseAsync()
    {
        IReadOnlyList<string> packages = ["test-package"];
        RpcResponse expectedResponse = new(count: 1, [BuildSearchResult()], rpcType: "multiinfo", version: 5);

        _ = this
            ._localAurRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Package>>([]);

        _ = this
            ._remoteAurRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(expectedResponse);

        this._localAurRpc.When(async x =>
                await x.SyncUpstreamReposAsync(
                    upstream: Arg.Any<RpcResponse>(),
                    userAgent: Arg.Any<ProductInfoHeaderValue?>()
                )
            )
            .Do(_ => throw new GitException("Temporary git server unavailable"));

        RpcResponse result = await this._sut.InfoAsync(
            packages: packages,
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: expectedResponse, actual: result);
    }

    private static SearchResult BuildSearchResult()
    {
        return new SearchResult(
            description: "Test package",
            firstSubmitted: 0,
            id: 1,
            keywords: null,
            license: null,
            depends: null,
            makeDepends: null,
            optDepends: null,
            checkDepends: null,
            conflicts: null,
            replaces: null,
            groups: null,
            coMaintainers: null,
            lastModified: 0,
            maintainer: "test",
            name: "test-package",
            numVotes: 0,
            outOfDate: null,
            packageBase: "test-package",
            packageBaseId: 1,
            popularity: 0,
            url: "https://aur.archlinux.org/packages/test-package",
            urlPath: "/cgit/aur.git/snapshot/test-package.tar.gz",
            version: "1.0.0-1"
        );
    }
}
