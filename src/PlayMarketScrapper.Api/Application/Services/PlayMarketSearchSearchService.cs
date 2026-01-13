using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using PlayMarketScrapper.Api.Application.Options;
using PlayMarketScrapper.Api.Application.Services.Abstract;
using PlayMarketScrapper.Api.Extensions;

namespace PlayMarketScrapper.Api.Application.Services;

public class PlayMarketSearchSearchService : IPlayMarketSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPlayMarketResponseParser _parser;
    private readonly PlayMarketOptions _opt;

    private const int BatchSize = 50;

    public PlayMarketSearchSearchService(IHttpClientFactory httpClientFactory, IPlayMarketResponseParser parser, IOptions<PlayMarketOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _parser = parser;
        _opt = options.Value;
    }
    
    // todo: mayme should serialize objects instead of doing strings

    public async Task<IReadOnlyList<string>> SearchPackagesAsync(string keyword, string country, int? limit = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreatePlayMarketClient();
        var targetLimit = limit ?? _opt.DefaultLimit;
        
        var resultPackages = new List<string>();
        var seenPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var url = BuildUrl(country, _opt.RpcIdsFirst);
        var body = BuildFirstFReq(_opt.RpcIdsFirst, keyword);

        var response = await PostBatchExecutedAsync(client, url, body, cancellationToken);

        var packages = _parser.ExtractPackages(response);
        var token = _parser.ExtractToken(response); // todo: returns null even if token exists

        AddUnique(packages, seenPackages, resultPackages, targetLimit);

        while (!string.IsNullOrEmpty(token) && resultPackages.Count < targetLimit)
        {
            url = BuildUrl(country, _opt.RpcIdsNext);
            body = BuildNextFReq(_opt.RpcIdsNext, keyword, token);

            response = await PostBatchExecutedAsync(client, url, body, cancellationToken);

            packages = _parser.ExtractPackages(response);

            if (packages.Count == 0)
                break;
            
            var nextToken = _parser.ExtractToken(response);

            AddUnique(packages, seenPackages, resultPackages, targetLimit);

            if (string.Equals(token, nextToken, StringComparison.Ordinal))
                break;

            token = nextToken;
        }

        return resultPackages;
    }

    private async Task<string> PostBatchExecutedAsync(HttpClient client, string url, string freq, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("f.req", freq)
        ]);

        var response = await client.PostAsync(url, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // todo: need better way to handle errors
        response.EnsureSuccessStatusCode();

        return body;
    }
    
    private string BuildUrl(string country, string rpcIds)
    {
        var queryParameters = new Dictionary<string, string?>
        {
            ["rpcids"] = rpcIds,
            ["source-path"] = _opt.SourcePath,
            ["bl"] = _opt.Bl,
            ["hl"] = _opt.Hl,
            ["gl"] = country.ToUpperInvariant(),
            ["authuser"] = _opt.AuthUser,
            ["rt"] = _opt.Rt,
        };

        return QueryHelpers.AddQueryString(_opt.EndpointPath, queryParameters);
    }
    
    private static string BuildFirstFReq(string rpcId, string keyword)
    {
        return
            $"[[[\"{rpcId}\",\"[[[null,null,null,null,[null,1]],[[10,[10,50]],null,null,[96,108,72,100,27,177,183,222,8,57,169,110,11,184,16,1,139,152,194,165,68,163,211,9,71,31,195,12,64,151,150,148,113,104,55,56,145,32,34,10,122]],[\\\"{Escape(keyword)}\\\"],4,null,null,null,[null,1]]]\",null,\"1\"]]]";
    }
    
    private static string BuildNextFReq(string rpcId, string keyword, string token)
    {
        return
            $"[[[\"{rpcId}\",\"[[[null,null,null,null,[null,1]],[[10,[10,50]],\\\"{Escape(token)}\\\",null,[96,108,72,100,27,177,183,222,8,57,169,110,11,184,16,1,139,152,194,165,68,163,211,9,71,31,195,12,64,151,150,148,113,104,55,56,145,32,34,10,122]],[\\\"{Escape(keyword)}\\\"],4,null,null,null,[null,1]]]\",null,\"1\"]]]";
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) 
            return string.Empty;
        
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
    
    private void AddUnique(IEnumerable<string> source, HashSet<string> seen, List<string> dest, int limit)
    {
        foreach (var item in source)
        {
            if (dest.Count >= limit) 
                return;
            
            if (seen.Add(item)) 
                dest.Add(item);
        }
    }
}