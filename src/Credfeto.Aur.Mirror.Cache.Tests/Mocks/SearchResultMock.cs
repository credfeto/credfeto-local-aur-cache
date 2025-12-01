using System.Threading;
using Credfeto.Aur.Mirror.Models.AurRpc;
using FunFair.Test.Common.Mocks;

namespace Credfeto.Aur.Mirror.Cache.Tests.Mocks;

internal sealed class SearchResultMock : MockBase<SearchResult>
{
    private long _lastModified;

    public SearchResultMock()
        : base(new(description: "",
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
                   version: "1.23"))
    {
        SearchResult current = this;
        this._lastModified = current.LastModified;
    }

    public override SearchResult Next()
    {
        long lastModified = Interlocked.Increment(ref this._lastModified);

        return new(description: "",
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
                   lastModified: lastModified,
                   maintainer: "example",
                   name: "example-package",
                   numVotes: 42,
                   outOfDate: null,
                   packageBase: "example-base",
                   packageBaseId: 41,
                   popularity: 184,
                   url: "https://example.com",
                   urlPath: "/package",
                   version: "1.23");
    }
}