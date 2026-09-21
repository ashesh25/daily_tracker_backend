# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Daily_Tracker.Domain/Daily_Tracker.Domain.csproj src/Daily_Tracker.Domain/
COPY src/Daily_Tracker.Infrastructure/Daily_Tracker.Infrastructure.csproj src/Daily_Tracker.Infrastructure/
COPY src/Daily_Tracker.Api/Daily_Tracker.Api.csproj src/Daily_Tracker.Api/
RUN dotnet restore src/Daily_Tracker.Api/Daily_Tracker.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/Daily_Tracker.Api/Daily_Tracker.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --property:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Render provides PORT; use 8080 locally when PORT is not set.
ENTRYPOINT ["sh", "-c", "dotnet Daily_Tracker.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
