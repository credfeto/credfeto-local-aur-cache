using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Git.Exceptions;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services;
using FunFair.Test.Common;
using FunFair.Test.Common.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Rpc.Tests;

public sealed class AurRpcTests : LoggingTestBase
{
    private readonly IAurMetadataGz _aurMetadataGz;
    private readonly IGitHubMirrorRpc _gitHubMirrorRpc;
    private readonly ILocalAurRpc _localAurRpc;
    private readonly IRemoteAurRpc _remoteAurRpc;
    private readonly AurRpc _sut;

    public AurRpcTests(ITestOutputHelper output)
        : base(output)
    {
        this._remoteAurRpc = GetSubstitute<IRemoteAurRpc>();
        this._localAurRpc = GetSubstitute<ILocalAurRpc>();
        this._aurMetadataGz = GetSubstitute<IAurMetadataGz>();
        this._gitHubMirrorRpc = GetSubstitute<IGitHubMirrorRpc>();
        ILogger<AurRpc> logger = this.GetTypedLogger<AurRpc>();

        this._sut = new AurRpc(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            gitHubMirrorRpc: this._gitHubMirrorRpc,
            config: Options.Create(new ServerConfig()),
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

    [Fact]
    public async Task InfoAsync_WhenAurUnavailableInFallbackMode_UsesMirrorResponseAsync()
    {
        IReadOnlyList<string> packages = ["test-package"];
        RpcResponse mirrorResponse = new(count: 1, [BuildSearchResult()], rpcType: "multiinfo", version: 5);

        AurRpc fallbackSut = new(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            gitHubMirrorRpc: this._gitHubMirrorRpc,
            config: Options.Create(new ServerConfig { Upstream = new UpstreamConfig { Mode = UpstreamMode.Fallback } }),
            timeProvider: MockDateTimeSources.Past,
            logger: this.GetTypedLogger<AurRpc>()
        );

        _ = this
            ._localAurRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Package>>([]);

        this._remoteAurRpc.When(async x =>
                await x.InfoAsync(
                    packages: Arg.Any<IReadOnlyList<string>>(),
                    userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                    cancellationToken: Arg.Any<CancellationToken>()
                )
            )
            .Do(_ => throw new HttpRequestException("AUR unavailable"));

        _ = this
            ._gitHubMirrorRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(mirrorResponse);

        RpcResponse result = await fallbackSut.InfoAsync(
            packages: packages,
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: mirrorResponse, actual: result);
    }

    [Fact]
    public async Task InfoAsync_WhenMirrorOnlyMode_BypassesAurAndUsesMirrorAsync()
    {
        IReadOnlyList<string> packages = ["test-package"];
        RpcResponse mirrorResponse = new(count: 1, [BuildSearchResult()], rpcType: "multiinfo", version: 5);

        AurRpc mirrorOnlySut = new(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            gitHubMirrorRpc: this._gitHubMirrorRpc,
            config: Options.Create(
                new ServerConfig { Upstream = new UpstreamConfig { Mode = UpstreamMode.MirrorOnly } }
            ),
            timeProvider: MockDateTimeSources.Past,
            logger: this.GetTypedLogger<AurRpc>()
        );

        _ = this
            ._gitHubMirrorRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(mirrorResponse);

        RpcResponse result = await mirrorOnlySut.InfoAsync(
            packages: packages,
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: mirrorResponse, actual: result);

        await this
            ._remoteAurRpc.DidNotReceive()
            .InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task SearchAsync_WhenMirrorOnlyMode_BypassesAurAndUsesLocalCacheAsync()
    {
        AurRpc mirrorOnlySut = new(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            gitHubMirrorRpc: this._gitHubMirrorRpc,
            config: Options.Create(
                new ServerConfig { Upstream = new UpstreamConfig { Mode = UpstreamMode.MirrorOnly } }
            ),
            timeProvider: MockDateTimeSources.Past,
            logger: this.GetTypedLogger<AurRpc>()
        );

        _ = this
            ._localAurRpc.SearchAsync(
                keyword: Arg.Any<string>(),
                by: Arg.Any<string>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Package>>([]);

        _ = this
            ._aurMetadataGz.SearchAsync(
                keyword: Arg.Any<string>(),
                by: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<SearchResult>>([]);

        RpcResponse result = await mirrorOnlySut.SearchAsync(
            keyword: "test",
            by: "name",
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 0, actual: result.Count);

        await this
            ._remoteAurRpc.DidNotReceive()
            .SearchAsync(
                keyword: Arg.Any<string>(),
                by: Arg.Any<string>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InfoAsync_WhenAurUnavailableInFallbackModeAndMirrorReturnsEmpty_FallsBackToLocalCacheAsync()
    {
        IReadOnlyList<string> packages = ["test-package"];
        RpcResponse emptyMirrorResponse = new(count: 0, [], rpcType: "multiinfo", version: 5);

        AurRpc fallbackSut = new(
            remoteAurRpc: this._remoteAurRpc,
            localAurRpc: this._localAurRpc,
            aurMetadataGz: this._aurMetadataGz,
            gitHubMirrorRpc: this._gitHubMirrorRpc,
            config: Options.Create(new ServerConfig { Upstream = new UpstreamConfig { Mode = UpstreamMode.Fallback } }),
            timeProvider: MockDateTimeSources.Past,
            logger: this.GetTypedLogger<AurRpc>()
        );

        _ = this
            ._localAurRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns<IReadOnlyList<Package>>([]);

        this._remoteAurRpc.When(async x =>
                await x.InfoAsync(
                    packages: Arg.Any<IReadOnlyList<string>>(),
                    userAgent: Arg.Any<ProductInfoHeaderValue?>(),
                    cancellationToken: Arg.Any<CancellationToken>()
                )
            )
            .Do(_ => throw new HttpRequestException("AUR unavailable"));

        _ = this
            ._gitHubMirrorRpc.InfoAsync(
                packages: Arg.Any<IReadOnlyList<string>>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(emptyMirrorResponse);

        RpcResponse result = await fallbackSut.InfoAsync(
            packages: packages,
            userAgent: null,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 0, actual: result.Count);
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
