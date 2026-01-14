using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using PlayMarketScrapper.Api.Application.Options;
using PlayMarketScrapper.Api.Application.Services.Abstract;
using PlayMarketScrapper.Api.Extensions;

namespace PlayMarketScrapper.Api.Application.Services;

public class PlayMarketSearchService : IPlayMarketSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPlayMarketResponseParser _parser;
    private readonly PlayMarketOptions _opt;

    private const int BatchSize = 50;

    public PlayMarketSearchService(
        IHttpClientFactory httpClientFactory, 
        IPlayMarketResponseParser parser, 
        IOptions<PlayMarketOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _parser = parser;
        _opt = options.Value;
    }

    public async Task<IReadOnlyList<string>> SearchPackagesAsync(string keyword, string country, int? limit = null, CancellationToken cancellationToken = default)
    {
        var targetCount = limit is null || limit <= 0 ? _opt.DefaultLimit : limit.Value;
        var client = _httpClientFactory.CreatePlayMarketClient();

        var resultPackages = new List<string>();
        var seenPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var url = BuildUrl(country, _opt.RpcIdSearch);
        var body = BuildFirstPagePayload(keyword);

        var rawResponse = await PostBatchexecuteAsync(client, url, body, cancellationToken);
        
        // todo: still getting no packages
        var packages = _parser.ExtractPackages(rawResponse);
        var token = _parser.ExtractToken(rawResponse);

        AddUnique(packages, seenPackages, resultPackages, targetCount);

        while (!string.IsNullOrEmpty(token) && resultPackages.Count < targetCount)
        {
            url = BuildUrl(country, _opt.RpcIdPagination);

            body = BuildNextPagePayload(keyword, token);

            rawResponse = await PostBatchexecuteAsync(client, url, body, cancellationToken);

            packages = _parser.ExtractPackages(rawResponse);
            
            if (packages.Count == 0) break;

            AddUnique(packages, seenPackages, resultPackages, targetCount);

            var nextToken = _parser.ExtractToken(rawResponse);

            if (string.Equals(token, nextToken, StringComparison.Ordinal)) break;
            
            token = nextToken;
        }

        return resultPackages;
    }

    private string BuildFirstPagePayload(string keyword)
    {
        var innerArray = new object[]
        {
            new object[] { null, null, null, null, new object[] { null, 1 } },
            new object[] { 
                new object[] { 10, new[] { 10, BatchSize } }, 
                null, 
                null, 
                null
            },
            new object[] { keyword },
            4, null, null, null, 
            new object[] { null, 1 }
        };

        return SerializeRequest(_opt.RpcIdSearch, innerArray);
    }

    private string BuildNextPagePayload(string keyword, string token)
    {
        var innerArray = new object[]
        {
            new object[] { null, null, null, null, new object[] { null, 1 } },
            new object[] { 
                new object[] { 10, new[] { 10, BatchSize } }, 
                token, 
                null, 
                null
            },
            new object[] { keyword },
            4, null, null, null, 
            new object[] { null, 1 }
        };

        return SerializeRequest(_opt.RpcIdPagination, innerArray);
    }

    private string SerializeRequest(string rpcId, object innerData)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var innerJsonString = JsonSerializer.Serialize(innerData, jsonOptions);

        var payload = new object[]
        {
            new object[]
            {
                new object[]
                {
                    rpcId,
                    innerJsonString,
                    null,
                    "1"
                }
            }
        };

        return JsonSerializer.Serialize(payload, jsonOptions);
    }

    private async Task<string> PostBatchexecuteAsync(HttpClient client, string url, string freq, CancellationToken ct)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("f.req", freq)
        });
        
        content.Headers.ContentType!.CharSet = "UTF-8";

        var response = await client.PostAsync(url, content, ct);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync(ct);
    }

    private string BuildUrl(string country, string rpcId)
    {
         var queryParameters = new Dictionary<string, string?>
        {
            ["rpcids"] = rpcId,
            ["source-path"] = _opt.SourcePath,
            ["bl"] = _opt.Bl,
            ["hl"] = _opt.Hl,
            ["gl"] = country.ToUpperInvariant(),
            ["authuser"] = _opt.AuthUser,
            ["rt"] = _opt.Rt
        };
         
        return QueryHelpers.AddQueryString(_opt.EndpointPath, queryParameters);
    }

    private void AddUnique(IEnumerable<string> source, HashSet<string> seen, List<string> dest, int limit)
    {
        foreach (var item in source)
        {
            if (dest.Count >= limit) return;
            if (seen.Add(item)) dest.Add(item);
        }
    }
}