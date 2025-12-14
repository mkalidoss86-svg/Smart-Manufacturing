# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["VisionFlow.sln", "./"]
COPY ["src/NotificationService/NotificationService.Api/NotificationService.Api.csproj", "src/NotificationService/NotificationService.Api/"]
COPY ["src/NotificationService/NotificationService.Application/NotificationService.Application.csproj", "src/NotificationService/NotificationService.Application/"]
COPY ["src/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj", "src/NotificationService/NotificationService.Domain/"]
COPY ["src/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService/NotificationService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/NotificationService/NotificationService.Api/NotificationService.Api.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/src/NotificationService/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
