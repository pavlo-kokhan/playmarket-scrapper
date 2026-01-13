using Microsoft.Extensions.Options;
using PlayMarketScrapper.Api.Application.Constants;
using PlayMarketScrapper.Api.Application.Options;

namespace PlayMarketScrapper.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayMarketHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<PlayMarketOptions>()
            .Bind(configuration.GetSection(PlayMarketOptions.SectionName));
        
        return services.AddHttpClient(HttpClientNames.PlayMarket, (sp, client) => 
            {
                var options = sp.GetRequiredService<IOptions<PlayMarketOptions>>().Value;
                
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.Headers.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd(options.Headers.Accept);
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(options.Headers.AcceptLanguage);
            })
        .Services;
    }
}