using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SportsBook.Api.Features.Auth;
using SportsBook.Api.Features.Bets;
using SportsBook.Api.Features.OperatorMatches;
using SportsBook.Api.Features.PlayerMatches;
using SportsBook.Api.Features.Users;
using SportsBook.Api.Features.Wallets;
using SportsBook.Api.Middleware;
using SportsBook.Application;
using SportsBook.Domain.Enums;
using SportsBook.Infrastructure;
using SportsBook.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("JWT issuer is missing.");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("JWT audience is missing.");

if (string.IsNullOrWhiteSpace(jwtSigningKey) || jwtSigningKey.Length < 32)
    throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlayerOnly", policy =>
        policy.RequireRole(UserRole.Player.ToString()));

    options.AddPolicy("OperatorOnly", policy =>
        policy.RequireRole(UserRole.Operator.ToString()));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "SportsBook.Api",
    utcNow = DateTimeOffset.UtcNow
}));

app.MapAuthEndpoints();
app.MapUserEndpoints();

app.MapOperatorMatchEndpoints();
app.MapPlayerMatchEndpoints();
app.MapBetEndpoints();
app.MapWalletEndpoints();

app.Run();
