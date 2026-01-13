using PlayMarketScrapper.Api.Application.Services;
using PlayMarketScrapper.Api.Application.Services.Abstract;
using PlayMarketScrapper.Api.Application.Validators;
using PlayMarketScrapper.Api.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddOpenApi()
    .AddControllers()
    .Services
    .AddCors()
    .AddScoped<IPlayMarketSearchService, PlayMarketSearchSearchService>()
    .AddSingleton<IPlayMarketResponseParser, PlayMarketResponseParser>()
    .AddSingleton<PlayMarketSearchValidator>()
    .AddPlayMarketHttpClient(builder.Configuration);

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .Build();
});

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Debug"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();