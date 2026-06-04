# Base image for runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["services/Wasel.NotificationService/Wasel.NotificationService.csproj", "services/Wasel.NotificationService/"]
RUN dotnet restore "services/Wasel.NotificationService/Wasel.NotificationService.csproj"

# Copy everything else and build
COPY ["services/Wasel.NotificationService/", "services/Wasel.NotificationService/"]
WORKDIR "/src/services/Wasel.NotificationService"
RUN dotnet build "Wasel.NotificationService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Wasel.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wasel.NotificationService.dll"]
