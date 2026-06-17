using System;
using System.Linq;
using Credfeto.Aur.Mirror.Models.AurRpc;

namespace Credfeto.Aur.Mirror.Rpc.Helpers;

internal static class SearchResultMatcher
{
    public static bool IsMatch(SearchResult result, string keyword, string by)
    {
        return by switch
        {
            "name" => result.Name.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase),
            "name-desc" => result.Name.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)
                || result.Description.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase),
            "maintainer" => result.Maintainer.Contains(
                value: keyword,
                comparisonType: StringComparison.OrdinalIgnoreCase
            ),
            "depends" => result.Depends?.Any(d =>
                d.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)
            ) == true,
            "makedepends" => result.MakeDepends?.Any(d =>
                d.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)
            ) == true,
            "optdepends" => result.OptDepends?.Any(d =>
                d.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)
            ) == true,
            "checkdepends" => result.CheckDepends?.Any(d =>
                d.Contains(value: keyword, comparisonType: StringComparison.OrdinalIgnoreCase)
            ) == true,
            _ => false,
        };
    }
}
