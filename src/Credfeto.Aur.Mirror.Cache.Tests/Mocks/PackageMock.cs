using System;
using Credfeto.Aur.Mirror.Cache.Interfaces;
using FunFair.Test.Common.Mocks;

namespace Credfeto.Aur.Mirror.Cache.Tests.Mocks;

internal sealed class PackageMock : MockBase<Package>
{
    public PackageMock()
        : base(
            new(
                lastSaved: DateTimeOffset.MinValue,
                lastAccessed: DateTimeOffset.MinValue,
                lastRequestedUpstream: DateTimeOffset.MinValue,
                new(
                    description: "",
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
                    version: "1.23"
                ),
                lastCloned: null
            )
        ) { }

    public override Package Next()
    {
        return new(
            lastSaved: DateTimeOffset.MinValue,
            lastAccessed: DateTimeOffset.MinValue,
            lastRequestedUpstream: DateTimeOffset.MinValue,
            new(
                description: "",
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
                version: "1.23"
            ),
            lastCloned: null
        );
    }
}
