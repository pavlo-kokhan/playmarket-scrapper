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

    public string RpcIdSearch { get; init; } = string.Empty;
    public string RpcIdPagination { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;

    public HeaderOptions Headers { get; init; } = new();

    public int DefaultLimit { get; init; } = 250;

    public sealed class HeaderOptions
    {
        public string UserAgent { get; init; } = string.Empty;
        public string Accept { get; init; } = string.Empty;
        public string AcceptLanguage { get; init; } = string.Empty;
    }
}