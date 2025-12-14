# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ManufacturingDataSimulator.Worker/ManufacturingDataSimulator.Worker.csproj", "ManufacturingDataSimulator.Worker/"]
COPY ["src/ManufacturingDataSimulator.Application/ManufacturingDataSimulator.Application.csproj", "ManufacturingDataSimulator.Application/"]
COPY ["src/ManufacturingDataSimulator.Domain/ManufacturingDataSimulator.Domain.csproj", "ManufacturingDataSimulator.Domain/"]
COPY ["src/ManufacturingDataSimulator.Infrastructure/ManufacturingDataSimulator.Infrastructure.csproj", "ManufacturingDataSimulator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ManufacturingDataSimulator.Worker/ManufacturingDataSimulator.Worker.csproj"

# Copy source code
COPY src/ManufacturingDataSimulator.Worker/. ManufacturingDataSimulator.Worker/
COPY src/ManufacturingDataSimulator.Application/. ManufacturingDataSimulator.Application/
COPY src/ManufacturingDataSimulator.Domain/. ManufacturingDataSimulator.Domain/
COPY src/ManufacturingDataSimulator.Infrastructure/. ManufacturingDataSimulator.Infrastructure/

# Build
WORKDIR "/src/ManufacturingDataSimulator.Worker"
RUN dotnet build "ManufacturingDataSimulator.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingDataSimulator.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManufacturingDataSimulator.Worker.dll"]
