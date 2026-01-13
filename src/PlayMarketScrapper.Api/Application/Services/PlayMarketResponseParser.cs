using System.Text.RegularExpressions;
using PlayMarketScrapper.Api.Application.Services.Abstract;

namespace PlayMarketScrapper.Api.Application.Services;

public partial class PlayMarketResponseParser : IPlayMarketResponseParser
{
    public IReadOnlyList<string> ExtractPackages(string rawResponse)
    {
        var normalized = Normalize(rawResponse);

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in PackageEntryRegex().Matches(normalized))
        {
            var pkg = m.Groups["pkg"].Value;
            
            if (seen.Add(pkg))
                result.Add(pkg);
        }

        return result;
    }

    public string? ExtractToken(string rawResponse)
    {
        var s = Normalize(rawResponse);

        return TokenRegex().Matches(s)
            .Select(m => m.Groups["token"].Value)
            .OrderByDescending(x => x.Length)
            .FirstOrDefault();
    }

    private static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        raw = raw.Replace(")]}'", "", StringComparison.Ordinal);

        raw = DigitsOnlyLineRegex().Replace(raw, "");

        raw = raw.Replace("\\\"", "\"", StringComparison.Ordinal);

        raw = raw.Replace("\\u003d", "=", StringComparison.Ordinal);
        raw = raw.Replace("\\u0026", "&", StringComparison.Ordinal);

        return raw;
    }

    [GeneratedRegex(@"\[\s*""(?<pkg>[a-zA-Z][a-zA-Z0-9_]*(?:\.[a-zA-Z0-9_]+)+)""\s*,\s*7\s*\]", RegexOptions.Compiled)]
    private static partial Regex PackageEntryRegex();

    [GeneratedRegex(@"""(?<token>[A-Za-z0-9\+\/=_\-]{100,})""", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"(?m)^\s*\d+\s*$\r?\n?", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyLineRegex();
}