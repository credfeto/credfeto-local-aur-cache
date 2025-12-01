using System;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Cache.Services;
using FunFair.Test.Common;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Cache.Tests.Services;

public sealed class BackgroundMetadataUpdaterTests : TestBase
{
    private readonly Package? _examplePackage;

    private readonly ILocalAurMetadata _localAurMetadata;

    private readonly Package? _packageDoesNotExist;
    private readonly BackgroundMetadataUpdater _updater;

    public BackgroundMetadataUpdaterTests()
    {
        this._localAurMetadata = Substitute.For<ILocalAurMetadata>();
        this._updater = new(this._localAurMetadata);

        this._packageDoesNotExist = null;
        this._examplePackage = new(lastSaved: DateTimeOffset.MinValue,
                                   lastAccessed: DateTimeOffset.MinValue,
                                   lastRequestedUpstream: DateTimeOffset.MinValue,
                                   new(description: "",
                                       firstSubmitted: 123456789,
                                       id: 44,
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
                                       lastModified: 1234567890,
                                       maintainer: "example",
                                       name: "example-package",
                                       numVotes: 42,
                                       outOfDate: null,
                                       packageBase: "example-base",
                                       packageBaseId: 41,
                                       popularity: 184,
                                       url: "https://example.com",
                                       urlPath: "/package",
                                       version: "1.23"),
                                   lastCloned: null);
    }

    [Fact]
    public async Task RequestUpdateAsync_ValidPackageName_UpdateCalled()
    {
        // Arrange

        _ = this._localAurMetadata.Get("example-package")
                .Returns(this._examplePackage);

        const string packageName = "example-package";

        // Act
        await this._updater.RequestUpdateAsync(packageName: packageName, update: Update, this.CancellationToken());

        // Assert
        _ = this._localAurMetadata.Received(1)
                .Get("example-package");
        await this._localAurMetadata.Received(1)
                  .UpdateAsync(Arg.Is<Package>(p => StringComparer.Ordinal.Equals(x: p.PackageName, y: "example-package")), Arg.Any<Action<Package>>(), Arg.Any<CancellationToken>());

        return;

        static void Update(Package _)
        {
            /* mock update logic */
        }
    }

    [Fact]
    public async Task RequestUpdateAsync_InvalidPackageName_WritesToQueue()
    {
        // Arrange
        _ = this._localAurMetadata.Get("invalid-package")
                .Returns(this._packageDoesNotExist);

        const string packageName = "invalid-package";

        // Act
        await this._updater.RequestUpdateAsync(packageName: packageName, update: Update, this.CancellationToken());

        // Assert
        await this._localAurMetadata.DidNotReceive()
                  .UpdateAsync(Arg.Any<Package>(), Arg.Any<Action<Package>>(), Arg.Any<CancellationToken>());
        _ = this._localAurMetadata.Received(1)
                .Get("invalid-package");

        return;

        static void Update(Package _)
        {
            /* mock update logic */
        }
    }
}