FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Dashboard.API.csproj", "./"]
RUN dotnet restore "Dashboard.API.csproj"
COPY . .
RUN dotnet build "Dashboard.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dashboard.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dashboard.API.dll"]
