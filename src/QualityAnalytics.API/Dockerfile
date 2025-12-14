FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QualityAnalytics.API.csproj", "./"]
RUN dotnet restore "QualityAnalytics.API.csproj"
COPY . .
RUN dotnet build "QualityAnalytics.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QualityAnalytics.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QualityAnalytics.API.dll"]
