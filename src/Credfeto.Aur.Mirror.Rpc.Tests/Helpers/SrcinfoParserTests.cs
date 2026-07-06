using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Aur.Mirror.Rpc.Tests.Helpers;

public sealed class SrcinfoParserTests : LoggingTestBase
{
    public SrcinfoParserTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void Parse_WhenContentIsEmpty_ReturnsNull()
    {
        SearchResult? result = SrcinfoParser.Parse(content: string.Empty, packageName: "test");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhenContentIsWhitespace_ReturnsNull()
    {
        SearchResult? result = SrcinfoParser.Parse(content: "   ", packageName: "test");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhenPkgverMissing_ReturnsNull()
    {
        const string content = "pkgbase = test\n\tpkgrel = 1\n\turl = https://example.com\n\npkgname = test\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "test");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhenPkgrelMissing_ReturnsNull()
    {
        const string content = "pkgbase = test\n\tpkgver = 1.0.0\n\turl = https://example.com\n\npkgname = test\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "test");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhenWellFormedMinimalContent_ReturnsSearchResult()
    {
        const string content =
            "pkgbase = afetch\n\tpkgdesc = A fetch program\n\tpkgver = 2.2.0\n\tpkgrel = 1\n\turl = https://github.com/13-CF/afetch\n\npkgname = afetch\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "afetch");

        Assert.NotNull(result);
        Assert.Equal(expected: "afetch", actual: result.Name);
        Assert.Equal(expected: "afetch", actual: result.PackageBase);
        Assert.Equal(expected: "2.2.0-1", actual: result.Version);
        Assert.Equal(expected: "A fetch program", actual: result.Description);
        Assert.Equal(expected: "https://github.com/13-CF/afetch", actual: result.Url);
        Assert.Equal(expected: "/cgit/aur.git/snapshot/afetch.tar.gz", actual: result.UrlPath);
    }

    [Fact]
    public void Parse_WhenEpochPresent_IncludesEpochInVersion()
    {
        const string content =
            "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 2\n\tepoch = 3\n\turl = https://example.com\n\npkgname = mypackage\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.Equal(expected: "3:1.0.0-2", actual: result.Version);
    }

    [Fact]
    public void Parse_WhenDependsPresent_MapsToDependsList()
    {
        const string content =
            "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 1\n\turl = https://example.com\n\tdepends = glibc\n\tdepends = libfoo\n\npkgname = mypackage\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.NotNull(result.Depends);
        Assert.Equal(expected: 2, actual: result.Depends.Count);
        Assert.Contains("glibc", result.Depends);
        Assert.Contains("libfoo", result.Depends);
    }

    [Fact]
    public void Parse_WhenLicensePresent_MapsToLicenseList()
    {
        const string content =
            "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 1\n\turl = https://example.com\n\tlicense = MIT\n\npkgname = mypackage\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.NotNull(result.License);
        Assert.Single(result.License);
        Assert.Equal(expected: "MIT", actual: result.License[0]);
    }

    [Fact]
    public void Parse_WhenContentIsMalformed_ReturnsNull()
    {
        const string content = "not valid srcinfo content at all";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "test");

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WhenNoDepends_DependsIsNull()
    {
        const string content =
            "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 1\n\turl = https://example.com\n\npkgname = mypackage\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.Null(result.Depends);
        Assert.Null(result.MakeDepends);
        Assert.Null(result.OptDepends);
    }

    [Fact]
    public void Parse_WhenPkgnameAbsent_UsesPkgbaseAsName()
    {
        const string content = "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 1\n\turl = https://example.com\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.Equal(expected: "mypackage", actual: result.Name);
    }

    [Fact]
    public void Parse_DefaultFieldsAreZeroOrEmpty()
    {
        const string content =
            "pkgbase = mypackage\n\tpkgver = 1.0.0\n\tpkgrel = 1\n\turl = https://example.com\n\npkgname = mypackage\n";

        SearchResult? result = SrcinfoParser.Parse(content: content, packageName: "mypackage");

        Assert.NotNull(result);
        Assert.Equal(expected: 0, actual: result.Id);
        Assert.Equal(expected: 0L, actual: result.FirstSubmitted);
        Assert.Equal(expected: 0L, actual: result.LastModified);
        Assert.Equal(expected: 0, actual: result.NumVotes);
        Assert.Equal(expected: 0, actual: result.PackageBaseId);
        Assert.Equal(expected: 0.0, actual: result.Popularity);
        Assert.Equal(expected: string.Empty, actual: result.Maintainer);
        Assert.Null(result.OutOfDate);
        Assert.Null(result.Keywords);
        Assert.Null(result.CoMaintainers);
    }
}
