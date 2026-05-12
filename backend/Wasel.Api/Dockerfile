# ──────────────────────────────────────────────
# Stage 1: Build
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY Wasel.Api.csproj .
RUN dotnet restore

# Copy source code and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ──────────────────────────────────────────────
# Stage 2: Runtime
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Launch
ENTRYPOINT ["dotnet", "Wasel.Api.dll"]
