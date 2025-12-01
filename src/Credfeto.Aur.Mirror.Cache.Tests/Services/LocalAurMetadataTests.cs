using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Cache.Services;
using Credfeto.Aur.Mirror.Config;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Date.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Credfeto.Aur.Mirror.Cache.Tests.Services;

public sealed class LocalAurMetadataTests : LoggingFolderCleanupTestBase
{
    private readonly LocalAurMetadata _localAurMetadata;
    private readonly SemaphoreSlim _semaphore;

    [SuppressMessage(category: "FunFair.CodeAnalsys", checkId: "FFS0004: Used mocked dates", Justification = "Unit Test")]
    public LocalAurMetadataTests(ITestOutputHelper output)
        : base(output)
    {
        this._semaphore = new(1);
        ICurrentTimeSource currentTimeSource = GetSubstitute<ICurrentTimeSource>();
        _ = currentTimeSource.UtcNow()
                             .Returns(DateTimeOffset.Now);

        IUpdateLock updateLock = GetSubstitute<IUpdateLock>();
        _ = updateLock.GetLockAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                      .Returns(this._semaphore);

        ILogger<LocalAurMetadata> logger = this.GetTypedLogger<LocalAurMetadata>();

        IOptions<ServerConfig> config = GetSubstitute<IOptions<ServerConfig>>();

        ServerConfig serverConfig = new();

        _ = config.Value.Returns(serverConfig);

        this._localAurMetadata = new(config: config, updateLock: updateLock, currentTimeSource: currentTimeSource, logger: logger);
    }

    [Fact]
    public async Task UpdateAsync_ShouldQueueAndSavePackageWhenFirstRegistered()
    {
        UpdateContext updateContext = new();

        SearchResult newSearchResult = MockReferenceData.SearchResult;

        await this._localAurMetadata.UpdateAsync(package: newSearchResult, onUpdate: updateContext.UpdateAsync, this.CancellationToken());

        ShouldBeSet(updateContext: updateContext, newSearchResult: newSearchResult, expectedChanged: false);
    }

    [Fact]
    public async Task UpdateAsync_ShouldQueueAndSavePackageWhenNotChanged()
    {
        UpdateContext updateContext = new();

        SearchResult newSearchResult = MockReferenceData.SearchResult;

        await this._localAurMetadata.UpdateAsync(package: newSearchResult, onUpdate: updateContext.UpdateAsync, this.CancellationToken());

        ShouldBeSet(updateContext: updateContext, newSearchResult: newSearchResult, expectedChanged: false);

        updateContext.Clear();

        await this._localAurMetadata.UpdateAsync(package: newSearchResult, onUpdate: updateContext.UpdateAsync, this.CancellationToken());

        ShouldNotBeSet(updateContext);
    }

    [Fact]
    public async Task UpdateAsync_ShouldQueueAndSavePackageWhenChanged()
    {
        UpdateContext updateContext = new();

        SearchResult newSearchResult = MockReferenceData.SearchResult;

        await this._localAurMetadata.UpdateAsync(package: newSearchResult, onUpdate: updateContext.UpdateAsync, this.CancellationToken());

        ShouldBeSet(updateContext: updateContext, newSearchResult: newSearchResult, expectedChanged: false);

        updateContext.Clear();

        SearchResult newSearchResult2 = MockReferenceData.SearchResult.Next();
        await this._localAurMetadata.UpdateAsync(package: newSearchResult2, onUpdate: updateContext.UpdateAsync, this.CancellationToken());

        ShouldBeSet(updateContext: updateContext, newSearchResult: newSearchResult, expectedChanged: true);
    }

    private static void ShouldNotBeSet(UpdateContext updateContext)
    {
        Assert.False(condition: updateContext.Set, userMessage: "Should not be set");
    }

    private static void ShouldBeSet(UpdateContext updateContext, SearchResult newSearchResult, bool expectedChanged)
    {
        Assert.True(condition: updateContext.Set, userMessage: "Should not be set");
        bool changed = Assert.NotNull(updateContext.Changed);
        Assert.Equal(expected: expectedChanged, actual: changed);
        Assert.NotNull(updateContext.SearchResult);

        Assert.Equal(expected: newSearchResult.Id, actual: updateContext.SearchResult.Id);
    }

    private sealed class UpdateContext
    {
        public UpdateContext()
        {
            this.Clear();
        }

        public bool? Changed { get; private set; }

        public SearchResult? SearchResult { get; private set; }

        public bool Set { get; private set; }

        public void Clear()
        {
            this.Set = false;
            this.Changed = null;
            this.SearchResult = null;
        }

        public ValueTask UpdateAsync(SearchResult searchResult, bool changed)
        {
            if (this.Set)
            {
                throw new UnreachableException("Should not be able to set twice");
            }

            this.Set = true;

            this.SearchResult = searchResult;
            this.Changed = changed;

            return ValueTask.CompletedTask;
        }
    }
}