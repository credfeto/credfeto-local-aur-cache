using Credfeto.Aur.Mirror.Cache.Interfaces;
using Credfeto.Aur.Mirror.Cache.Tests.Mocks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using FunFair.Test.Common.Mocks;

namespace Credfeto.Aur.Mirror.Cache.Tests;

internal static class MockReferenceData
{
    public static MockBase<Package> Package { get; } = new PackageMock();

    public static MockBase<SearchResult> SearchResult { get; } = new SearchResultMock();
}
