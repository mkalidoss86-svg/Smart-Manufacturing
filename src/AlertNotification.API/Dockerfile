FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AlertNotification.API.csproj", "./"]
RUN dotnet restore "AlertNotification.API.csproj"
COPY . .
RUN dotnet build "AlertNotification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlertNotification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlertNotification.API.dll"]
