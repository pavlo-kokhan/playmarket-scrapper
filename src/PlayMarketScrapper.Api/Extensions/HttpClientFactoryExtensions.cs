using PlayMarketScrapper.Api.Application.Constants;

namespace PlayMarketScrapper.Api.Extensions;

public static class HttpClientFactoryExtensions
{
    public static HttpClient CreatePlayMarketClient(this IHttpClientFactory factory) 
        => factory.CreateClient(HttpClientNames.PlayMarket);
}