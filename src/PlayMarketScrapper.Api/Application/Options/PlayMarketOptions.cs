namespace PlayMarketScrapper.Api.Application.Options;

public class PlayMarketOptions
{
    public const string SectionName = nameof(PlayMarketOptions);
    
    public string BaseAddress { get; init; } = string.Empty;
    public string EndpointPath { get; init; } = string.Empty;

    public string SourcePath { get; init; } = string.Empty;
    public string Bl { get; init; } = string.Empty;
    public string Hl { get; init; } = string.Empty;
    public string AuthUser { get; init; } = string.Empty;
    public string Rt { get; init; } = string.Empty;

    public string RpcIdsFirst { get; init; } = string.Empty;
    public string RpcIdsNext { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;

    public HeaderOptions Headers { get; init; } = new();

    public int DefaultLimit { get; init; } = 250;

    public sealed class HeaderOptions
    {
        public string UserAgent { get; init; } = "Mozilla/5.0";
        public string Accept { get; init; } = "*/*";
        public string AcceptLanguage { get; init; } = "en-US,en;q=0.9";
    }
}