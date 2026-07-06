using System;
using System.Collections.Generic;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Helpers;

public static class SrcinfoParser
{
    public static SearchResult? Parse(string content, string packageName)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        Dictionary<string, List<string>> fields = new(StringComparer.OrdinalIgnoreCase);

        foreach (string line in content.Split('\n'))
        {
            ReadOnlySpan<char> trimmed = line.AsSpan().Trim();

            if (trimmed.IsEmpty || trimmed[0] == '#')
            {
                continue;
            }

            int equalsIndex = trimmed.IndexOf('=');

            if (equalsIndex <= 0)
            {
                continue;
            }

            string key = trimmed[..equalsIndex].Trim().ToString();
            string value = trimmed[(equalsIndex + 1)..].Trim().ToString();

            if (!fields.TryGetValue(key, out List<string>? values))
            {
                values = [];
                fields[key] = values;
            }

            values.Add(value);
        }

        return BuildSearchResult(fields: fields, packageName: packageName);
    }

    private static SearchResult? BuildSearchResult(Dictionary<string, List<string>> fields, string packageName)
    {
        string pkgver = GetSingle(fields, "pkgver");
        string pkgrel = GetSingle(fields, "pkgrel");

        if (string.IsNullOrEmpty(pkgver) || string.IsNullOrEmpty(pkgrel))
        {
            return null;
        }

        string epoch = GetSingle(fields, "epoch");
        string version = BuildVersion(pkgver: pkgver, pkgrel: pkgrel, epoch: epoch);

        string pkgbase = GetSingle(fields, "pkgbase");
        string pkgname = GetSingle(fields, "pkgname");
        string name = ResolveName(pkgname: pkgname, pkgbase: pkgbase, packageName: packageName);
        string packageBase = string.IsNullOrEmpty(pkgbase) ? name : pkgbase;

        return new SearchResult(
            description: GetSingle(fields, "pkgdesc"),
            firstSubmitted: 0,
            id: 0,
            keywords: null,
            license: GetList(fields, "license"),
            depends: GetList(fields, "depends"),
            makeDepends: GetList(fields, "makedepends"),
            optDepends: GetList(fields, "optdepends"),
            checkDepends: GetList(fields, "checkdepends"),
            conflicts: GetList(fields, "conflicts"),
            replaces: GetList(fields, "replaces"),
            groups: GetList(fields, "groups"),
            coMaintainers: null,
            lastModified: 0,
            maintainer: string.Empty,
            name: name,
            numVotes: 0,
            outOfDate: null,
            packageBase: packageBase,
            packageBaseId: 0,
            popularity: 0,
            url: GetSingle(fields, "url"),
            urlPath: $"/cgit/aur.git/snapshot/{name}.tar.gz",
            version: version
        );
    }

    private static string BuildVersion(string pkgver, string pkgrel, string epoch)
    {
        if (string.IsNullOrEmpty(epoch))
        {
            return $"{pkgver}-{pkgrel}";
        }

        return $"{epoch}:{pkgver}-{pkgrel}";
    }

    private static string ResolveName(string pkgname, string pkgbase, string packageName)
    {
        if (!string.IsNullOrEmpty(pkgname))
        {
            return pkgname;
        }

        if (!string.IsNullOrEmpty(pkgbase))
        {
            return pkgbase;
        }

        return packageName;
    }

    private static string GetSingle(Dictionary<string, List<string>> fields, string key)
    {
        return fields.TryGetValue(key, out List<string>? values) && values.Count > 0 ? values[0] : string.Empty;
    }

    private static IReadOnlyList<string>? GetList(Dictionary<string, List<string>> fields, string key)
    {
        return fields.TryGetValue(key, out List<string>? values) && values.Count > 0 ? values : null;
    }
}
