using System.Text.Json.Serialization;
using SportsBook.Api.Features.Bets;
using SportsBook.Api.Features.OperatorMatches;
using SportsBook.Api.Features.PlayerMatches;
using SportsBook.Api.Features.Wallets;
using SportsBook.Api.Middleware;
using SportsBook.Application;
using SportsBook.Infrastructure;
using SportsBook.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.EnsureCreatedAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "SportsBook.Api",
    utcNow = DateTimeOffset.UtcNow
}));

app.MapOperatorMatchEndpoints();
app.MapPlayerMatchEndpoints();
app.MapBetEndpoints();
app.MapWalletEndpoints();

app.Run();
