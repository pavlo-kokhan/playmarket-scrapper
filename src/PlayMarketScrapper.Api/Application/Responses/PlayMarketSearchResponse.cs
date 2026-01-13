namespace PlayMarketScrapper.Api.Application.Responses;

public record PlayMarketSearchResponse(
    string Keyword,
    string Country,
    IReadOnlyList<string> Packages,
    int PagesFetched,
    int Total);