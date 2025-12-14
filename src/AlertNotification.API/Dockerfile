# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AlertNotification.API.csproj", "./"]
RUN dotnet restore "AlertNotification.API.csproj"
COPY . .
RUN dotnet build "AlertNotification.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AlertNotification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 5003

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
RUN chown -R appuser:appuser /app

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:5003/health || exit 1

ENTRYPOINT ["dotnet", "AlertNotification.API.dll"]
