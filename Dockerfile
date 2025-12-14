# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["VisionFlow.sln", "./"]
COPY ["src/InspectionWorker/InspectionWorker.csproj", "src/InspectionWorker/"]
COPY ["src/InspectionWorker.Application/InspectionWorker.Application.csproj", "src/InspectionWorker.Application/"]
COPY ["src/InspectionWorker.Infrastructure/InspectionWorker.Infrastructure.csproj", "src/InspectionWorker.Infrastructure/"]
COPY ["src/InspectionWorker.Domain/InspectionWorker.Domain.csproj", "src/InspectionWorker.Domain/"]

# Restore dependencies
RUN dotnet restore "src/InspectionWorker/InspectionWorker.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/InspectionWorker"
RUN dotnet build "InspectionWorker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InspectionWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 worker && chown -R worker:worker /app
USER worker

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "InspectionWorker.dll"]
