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
        this._examplePackage = MockReferenceData.Package;
    }

    [Fact]
    public async Task RequestUpdateAsync_ValidPackageName_UpdateCalled()
    {
        _ = this._localAurMetadata.Get("example-package")
                .Returns(this._examplePackage);

        const string packageName = "example-package";

        await this._updater.RequestUpdateAsync(packageName: packageName, update: Update, this.CancellationToken());

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
        _ = this._localAurMetadata.Get("invalid-package")
                .Returns(this._packageDoesNotExist);

        const string packageName = "invalid-package";

        await this._updater.RequestUpdateAsync(packageName: packageName, update: Update, this.CancellationToken());

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