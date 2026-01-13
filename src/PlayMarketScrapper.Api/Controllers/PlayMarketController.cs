using Microsoft.AspNetCore.Mvc;
using PlayMarketScrapper.Api.Application.Services.Abstract;
using PlayMarketScrapper.Api.Application.Validators;

namespace PlayMarketScrapper.Api.Controllers;

[ApiController]
[Route("play-market")]
public class PlayMarketController : ControllerBase
{
    private readonly IPlayMarketSearchService _playMarketSearchService;
    private readonly PlayMarketSearchValidator _playMarketSearchValidator;

    public PlayMarketController(IPlayMarketSearchService playMarketSearchService, PlayMarketSearchValidator playMarketSearchValidator)
    {
        _playMarketSearchService = playMarketSearchService;
        _playMarketSearchValidator = playMarketSearchValidator;
    }

    [HttpGet]
    public async Task<IActionResult> TestAsync(string keyword, string country, CancellationToken cancellationToken = default)
    {
        var result = await _playMarketSearchService.SearchFirstRowAsync(keyword, country, cancellationToken);
        return Ok(result);
    }

    [HttpGet("search-packages")]
    public async Task<IActionResult> SearchAsync(string keyword, string country, int limit, CancellationToken cancellationToken = default)
    {
        var valid = _playMarketSearchValidator.Validate(keyword, country, limit);
        
        if (!valid)
            return BadRequest("Invalid parameters");
        
        var result = await _playMarketSearchService.SearchPackagesAsync(keyword, country, limit, cancellationToken);
        return Ok(result);
    }
}