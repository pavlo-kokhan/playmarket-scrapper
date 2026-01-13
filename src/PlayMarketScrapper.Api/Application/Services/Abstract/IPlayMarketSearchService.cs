namespace PlayMarketScrapper.Api.Application.Services.Abstract;

public interface IPlayMarketSearchService
{
    Task<string> SearchFirstRowAsync(string keyword, string country, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<string>> SearchPackagesAsync(string keyword, string country, int? limit = null, CancellationToken cancellationToken = default);
}