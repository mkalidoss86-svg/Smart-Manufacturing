# Smart-Manufacturing

The Smart Manufacturing Quality Platform is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## Services

### VisionFlow – Notification Service

A real-time notification service built with .NET 8 and SignalR for pushing inspection updates to clients.

**Features:**
- **Real-time Communication**: SignalR-based WebSocket connections for instant updates
- **Clean Architecture**: Separated into Domain, Application, Infrastructure, and API layers
- **Observer/Pub-Sub Pattern**: Decoupled event-driven architecture
- **Horizontal Scalability**: Redis backplane support for multi-instance deployments
- **Missed-Event Recovery**: Clients can retrieve events missed during disconnection
- **Connection Management**: Configurable connection limits and automatic reconnection handling
- **Health Monitoring**: Built-in health check endpoint
- **Structured Logging**: JSON-formatted logs for easy monitoring

**Documentation:**
- [Notification Service README](API.md)
- [Deployment Guide](DEPLOYMENT.md)

### Other Platform Services

- **DataIngestion API** (Port 5001): Ingests manufacturing data
- **QualityAnalytics API** (Port 5002): Analyzes quality metrics
- **AlertNotification API** (Port 5003): Manages alerts and notifications
- **Dashboard API** (Port 5004): Provides dashboard data

### Ingestion API
A .NET 8 Minimal API service for ingesting production quality events. See [Ingestion API Documentation](docs/INGESTION_API.md) for details.

**Features:**
- REST API for production quality events
- Input validation with FluentValidation
- Event enrichment (IDs, timestamps, lineId)
- RabbitMQ publishing with retry and circuit breaker patterns
- Structured logging with Serilog
- Health check endpoint
- Clean Architecture design

## Getting Started

### Prerequisites
- .NET 8 SDK
- RabbitMQ

### Quick Start

1. Clone the repository
2. Start RabbitMQ:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```
3. Build and run:
   ```bash
   dotnet build
   cd src/IngestionApi/VisionFlow.IngestionApi
   dotnet run
   ```

## Architecture

The platform follows Clean Architecture principles:
- **Domain Layer**: Core business entities
- **Application Layer**: Business logic and use cases
- **Infrastructure Layer**: External dependencies (RabbitMQ, etc.)
- **API Layer**: REST endpoints and presentation

## Documentation

- [Ingestion API Documentation](docs/INGESTION_API.md)

## License

Copyright © 2024 VisionFlow Smart Manufacturing Platform
## CI/CD Failure Analysis Agent

This repository includes an automated CI/CD monitoring system that:
- 🔍 Detects pipeline failures across all stages (Build, Test, Docker, Kubernetes)
- 📋 Automatically creates detailed GitHub Issues with error analysis
- 🏷️ Classifies failures and applies appropriate labels
- 💡 Provides root cause suggestions and remediation steps
- ✅ Auto-closes issues when subsequent pipeline runs succeed
- 🚫 Prevents duplicate issues for the same commit

### Quick Start

The failure analysis agent runs automatically on every push or pull request. To test it manually:

1. Go to **Actions** → **CI/CD Test - Simulated Failure**
2. Click **Run workflow**
3. Select which stage should fail (or "none" for success)
4. Observe automatic issue creation and analysis

### Documentation

- [CI/CD Implementation Details](IMPLEMENTATION_SUMMARY.md)
- [Workflow Configuration](.github/workflows/README.md)
- [CI/CD Pipeline](.github/workflows/ci-pipeline.yml)

### Features

- **Intelligent Failure Detection**: Monitors Build, Test, Docker, Docker Compose, and Kubernetes stages
- **Detailed Issue Reports**: Includes logs, links, status tables, and actionable insights
- **Smart Classification**: Automatically categorizes failures and applies relevant labels
- **Duplicate Prevention**: Checks for existing issues before creating new ones
- **Auto-Assignment**: Issues are automatically assigned to repository maintainers
- **Auto-Closure**: Resolved issues close automatically when pipeline recovers

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- Redis (for Notification Service)

### Running with Docker Compose

All services can be run together using Docker Compose:

```bash
# Run in detached mode
docker compose up -d --build

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

Services will be available at:
- DataIngestion API: http://localhost:5001
- QualityAnalytics API: http://localhost:5002
- AlertNotification API: http://localhost:5003
- Dashboard API: http://localhost:5004
- Notification Service: http://localhost:8080
- Redis: localhost:6379

### Running Notification Service Locally

```bash
cd src/NotificationService/NotificationService.Api
dotnet restore
dotnet run
```

The service will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- SignalR Hub: /hubs/inspections
- Health Check: /health
- Swagger UI: /swagger

### Running Individual Services

To run other services locally:

```bash
# Navigate to service directory
cd src/DataIngestion.API

# Restore dependencies
dotnet restore

# Run the service
dotnet run
```

## 🏥 Health Checks

Each service exposes a health endpoint at `/health`:

```bash
curl http://localhost:5001/health  # DataIngestion
curl http://localhost:5002/health  # QualityAnalytics
curl http://localhost:5003/health  # AlertNotification
curl http://localhost:5004/health  # Dashboard
curl http://localhost:8080/health  # Notification Service
```

## 📊 API Documentation

When running in Development mode, each service exposes Swagger UI:

- DataIngestion API: http://localhost:5001/swagger
- QualityAnalytics API: http://localhost:5002/swagger
- AlertNotification API: http://localhost:5003/swagger
- Dashboard API: http://localhost:5004/swagger
- Notification Service: http://localhost:8080/swagger

## 🐳 Docker

Each service has its own Dockerfile located in its respective directory:

```bash
# Build individual service image
docker build -t dataingestion-api:latest -f src/DataIngestion.API/Dockerfile src/DataIngestion.API

# Run individual service
docker run -p 5001:5001 -e ASPNETCORE_URLS=http://+:5001 dataingestion-api:latest
```

For Notification Service:

```bash
# Build
docker build -t notification-service:latest .

# Run with Redis
docker run -p 8080:8080 \
  -e ConnectionStrings__Redis=redis:6379 \
  notification-service:latest
```

## Architecture

The platform follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic, services, DTOs, and use cases
- **Infrastructure Layer**: External concerns (SignalR, Redis, etc.)
- **API Layer**: REST controllers and ASP.NET Core hosting

## License

[Your License Here]

## Support

For issues and questions, please open an issue in the repository.
