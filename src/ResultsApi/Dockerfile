# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ResultsApi.csproj", "./"]
RUN dotnet restore "ResultsApi.csproj"
COPY . .
RUN dotnet build "ResultsApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ResultsApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install curl for healthchecks (alpine uses apk)
RUN apk add --no-cache curl

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
USER appuser

EXPOSE 8080
EXPOSE 8443

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ResultsApi.dll"]
