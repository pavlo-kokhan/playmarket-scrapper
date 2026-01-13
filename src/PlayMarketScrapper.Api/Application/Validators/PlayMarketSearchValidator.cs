namespace PlayMarketScrapper.Api.Application.Validators;

public class PlayMarketSearchValidator
{
    public bool Validate(string keyword, string country, int limit)
    {
        // todo: find out reasonable limit validation
        return !string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(country) && limit is > 0 and <= 500;
    }
}