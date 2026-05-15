FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY SportsBook.Api/SportsBook.Api.csproj SportsBook.Api/
COPY SportsBook.Application/SportsBook.Application.csproj SportsBook.Application/
COPY SportsBook.Domain/SportsBook.Domain.csproj SportsBook.Domain/
COPY SportsBook.Infrastructure/SportsBook.Infrastructure.csproj SportsBook.Infrastructure/
COPY SportsBook.Pricing/SportsBook.Pricing.csproj SportsBook.Pricing/

RUN dotnet restore SportsBook.Api/SportsBook.Api.csproj

COPY . .

RUN dotnet publish SportsBook.Api/SportsBook.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SportsBook.Api.dll"]
