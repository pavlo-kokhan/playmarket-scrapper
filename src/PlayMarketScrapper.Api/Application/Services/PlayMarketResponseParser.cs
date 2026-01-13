using System.Text.RegularExpressions;
using PlayMarketScrapper.Api.Application.Services.Abstract;

namespace PlayMarketScrapper.Api.Application.Services;

public partial class PlayMarketResponseParser : IPlayMarketResponseParser
{
    public IReadOnlyList<string> ExtractPackages(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return Array.Empty<string>();

        var s = StripTransportJunk(rawResponse);

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in PackageEntryRegex().Matches(s))
        {
            var pkg = m.Groups["pkg"].Value;
            if (seen.Add(pkg))
                result.Add(pkg);
        }

        return result;
    }

    public string? ExtractToken(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return null;

        var s = StripTransportJunk(rawResponse);

        s = s.Replace("\\u003d", "=", StringComparison.Ordinal)
             .Replace("\\u0026", "&", StringComparison.Ordinal);

        return TokenRegex().Matches(s)
            .Select(m => m.Groups["token"].Value)
            .OrderByDescending(x => x.Length)
            .FirstOrDefault();
    }

    private static string StripTransportJunk(string raw)
    {
        raw = raw.Replace(")]}'", "", StringComparison.Ordinal);

        raw = DigitsOnlyLineRegex().Replace(raw, "");

        return raw;
    }

    [GeneratedRegex(@"\[\s*(?:\\?"")(?<pkg>[A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z0-9_]+)+)(?:\\?"")\s*,\s*\d+\s*\]", RegexOptions.Compiled)]
    private static partial Regex PackageEntryRegex();

    [GeneratedRegex(@"""(?<token>[A-Za-z0-9\+\/=_\-]{80,})""", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"(?m)^\s*\d+\s*$\r?\n?", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyLineRegex();
}
