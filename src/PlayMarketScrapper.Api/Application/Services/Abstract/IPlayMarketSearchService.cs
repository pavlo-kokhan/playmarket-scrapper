namespace PlayMarketScrapper.Api.Application.Services.Abstract;

public interface IPlayMarketSearchService
{
    Task<IReadOnlyList<string>> SearchPackagesAsync(string keyword, string country, int? limit = null, CancellationToken cancellationToken = default);
}