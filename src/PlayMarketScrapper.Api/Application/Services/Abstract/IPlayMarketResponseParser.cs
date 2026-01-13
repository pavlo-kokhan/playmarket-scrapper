namespace PlayMarketScrapper.Api.Application.Services.Abstract;

public interface IPlayMarketResponseParser
{
    IReadOnlyList<string> ExtractPackages(string rawResponse);

    string? ExtractToken(string rawResponse);
}