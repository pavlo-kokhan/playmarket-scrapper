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

    public PlayMarketSearchSearchService(IHttpClientFactory httpClientFactory, IPlayMarketResponseParser parser, IOptions<PlayMarketOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _parser = parser;
        _opt = options.Value;
    }

    public async Task<string> SearchFirstRowAsync(string keyword, string country, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreatePlayMarketClient();
        
        var queryParameters = new Dictionary<string, string?>
        {
            ["source-path"] = _opt.SourcePath,
            ["bl"] = _opt.Bl,
            ["hl"] = _opt.Hl,
            ["gl"] = country.ToUpperInvariant(),
            ["authuser"] = _opt.AuthUser,
            ["rt"] = _opt.Rt,
            ["rpcids"] = _opt.RpcIdsFirst,
        };

        var url = QueryHelpers.AddQueryString(_opt.EndpointPath, queryParameters);
        
        var fReq = $"[[[\"lGYRle\",\"[[[null,null,null,null,[null,1]],[[10,[10,50]],null,null,[96,108,72,100,27,177,183,222,8,57,169,110,11,184,16,1,139,152,194,165,68,163,211,9,71,31,195,12,64,151,150,148,113,104,55,56,145,32,34,10,122]],[\\\"{keyword}\\\"],4,null,null,null,[null,1]]]\",null,\"1\"]]]";
        
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("f.req", fReq)
        });

        using var resp = await client.PostAsync(url, content, cancellationToken);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> SearchPackagesAsync(string keyword, string country, int? limit = null, CancellationToken cancellationToken = default)
    {
        var firstRow = await SearchFirstRowAsync(keyword, country, cancellationToken);

        var packages = _parser.ExtractPackages(firstRow);

        if (limit is null)
            return packages;

        if (limit.Value <= 0)
            return [];

        return packages.Take(limit.Value).ToList();
    }

    
}