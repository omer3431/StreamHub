# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["StreamHub.csproj", "./"]
RUN dotnet restore "StreamHub.csproj"

COPY . .
RUN dotnet publish "StreamHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

# Render sets the PORT env var at runtime and expects the app to listen on it.
# Defaults to 8080 for local `docker run`.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet StreamHub.dll"]
