# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ManufacturingDataSimulator.Api/ManufacturingDataSimulator.Api.csproj", "ManufacturingDataSimulator.Api/"]
COPY ["src/ManufacturingDataSimulator.Application/ManufacturingDataSimulator.Application.csproj", "ManufacturingDataSimulator.Application/"]
COPY ["src/ManufacturingDataSimulator.Domain/ManufacturingDataSimulator.Domain.csproj", "ManufacturingDataSimulator.Domain/"]
COPY ["src/ManufacturingDataSimulator.Infrastructure/ManufacturingDataSimulator.Infrastructure.csproj", "ManufacturingDataSimulator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ManufacturingDataSimulator.Api/ManufacturingDataSimulator.Api.csproj"

# Copy source code
COPY src/ManufacturingDataSimulator.Api/. ManufacturingDataSimulator.Api/
COPY src/ManufacturingDataSimulator.Application/. ManufacturingDataSimulator.Application/
COPY src/ManufacturingDataSimulator.Domain/. ManufacturingDataSimulator.Domain/
COPY src/ManufacturingDataSimulator.Infrastructure/. ManufacturingDataSimulator.Infrastructure/

# Build
WORKDIR "/src/ManufacturingDataSimulator.Api"
RUN dotnet build "ManufacturingDataSimulator.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingDataSimulator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManufacturingDataSimulator.Api.dll"]
