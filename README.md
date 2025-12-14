# Smart-Manufacturing

The Smart Manufacturing Quality Platform is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## Project Structure

### Inspection Worker Service

A .NET 8 worker service that processes quality inspection requests from a message queue.

**Key Features:**
- Clean Architecture (Domain, Application, Infrastructure, Worker layers)
- Event-driven processing with RabbitMQ
- Strategy pattern for inspection logic
- Retry and dead-letter queue patterns
- Idempotent and stateless processing
- Graceful shutdown support
- Health checks and structured logging
- Horizontally scalable

**Documentation:** [Inspection Worker Documentation](docs/INSPECTION-WORKER.md)

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

- [Full Implementation Details](IMPLEMENTATION_SUMMARY.md)
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
- RabbitMQ (local or remote) - for Inspection Worker
- Docker & Docker Compose

### Quick Start with Docker Compose

```bash
# Run in detached mode
docker compose up -d --build

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

Services will be available at:
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- Inspection Worker: Running in background
- DataIngestion API: http://localhost:5001
- QualityAnalytics API: http://localhost:5002
- AlertNotification API: http://localhost:5003
- Dashboard API: http://localhost:5004

### Running Individual Services

To run a specific service locally:

```bash
# Navigate to service directory
cd src/DataIngestion.API

# Restore dependencies
dotnet restore

# Run the service
dotnet run
```

For the Inspection Worker:

```bash
cd src/InspectionWorker
dotnet run
```

## 🏥 Health Checks

Each service exposes a health endpoint at `/health`:

```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

## 📊 API Documentation

When running in Development mode, each service exposes Swagger UI:

- DataIngestion API: http://localhost:5001/swagger
- QualityAnalytics API: http://localhost:5002/swagger
- AlertNotification API: http://localhost:5003/swagger
- Dashboard API: http://localhost:5004/swagger

## 🐳 Docker

Each service has its own Dockerfile located in its respective directory:

```bash
# Build individual service image
docker build -t dataingestion-api:latest -f src/DataIngestion.API/Dockerfile src/DataIngestion.API

# Run individual service
docker run -p 5001:5001 -e ASPNETCORE_URLS=http://+:5001 dataingestion-api:latest
```

## ☸️ Kubernetes Deployment

Kubernetes manifests are located in `infrastructure/k8s/`:

```bash
# Create namespace
kubectl apply -f infrastructure/k8s/namespace.yaml

# Apply ConfigMap
kubectl apply -f infrastructure/k8s/configmap.yaml

# Deploy services
kubectl apply -f infrastructure/k8s/deployments.yaml

# Create services
kubectl apply -f infrastructure/k8s/services.yaml

# Check deployment status
kubectl get pods -n visionflow
kubectl get services -n visionflow
```

## 🔧 Development

### Building All Services

```bash
# From repository root
dotnet build src/InspectionWorker/InspectionWorker.csproj
dotnet build src/DataIngestion.API/DataIngestion.API.csproj
dotnet build src/QualityAnalytics.API/QualityAnalytics.API.csproj
dotnet build src/AlertNotification.API/AlertNotification.API.csproj
dotnet build src/Dashboard.API/Dashboard.API.csproj
```

### Testing

```bash
# Run tests for Inspection Worker
dotnet test tests/InspectionWorker.Tests/InspectionWorker.Tests.csproj
```

### Project Structure

```
Smart-Manufacturing/
├── .github/
│   └── workflows/
│       └── ci.yml                    # GitHub Actions CI workflow
├── src/
│   ├── InspectionWorker/             # Inspection worker service
│   │   ├── InspectionWorker.csproj
│   │   ├── Worker.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── InspectionWorker.Domain/      # Domain layer
│   ├── InspectionWorker.Application/ # Application layer
│   ├── InspectionWorker.Infrastructure/ # Infrastructure layer
│   ├── DataIngestion.API/            # Data ingestion service
│   ├── QualityAnalytics.API/         # Quality analytics service
│   ├── AlertNotification.API/        # Alert notification service
│   └── Dashboard.API/                # Dashboard API service
├── tests/
│   └── InspectionWorker.Tests/       # Unit tests
├── infrastructure/
│   ├── k8s/                          # Kubernetes manifests
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── deployments.yaml
│   │   └── services.yaml
│   └── docker/                       # Docker configurations
├── docs/
│   ├── ARCHITECTURE.md               # Detailed architecture documentation
│   ├── INSPECTION-WORKER.md          # Inspection Worker documentation
│   └── MESSAGE-PRODUCER-SAMPLES.md   # Message producer examples
├── docker-compose.yml                # Local development compose file
├── VisionFlow.sln                    # Solution file
├── global.json                       # .NET SDK version
└── README.md                         # This file
```

## Architecture

The platform follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic and use cases
- **Infrastructure Layer**: External concerns (messaging, databases, etc.)
- **Worker Layer**: Background service hosting

## Technologies

- .NET 8
- RabbitMQ
- xUnit
- Structured Logging (JSON)
- Health Checks
- Docker & Docker Compose
- Kubernetes

## 🔄 CI/CD

The project uses GitHub Actions for continuous integration. The CI pipeline:

1. Builds all services using .NET 8
2. Runs tests (if available)
3. Builds Docker images for each service
4. Validates Docker images

See `.github/workflows/ci.yml` for details.

## 📖 Documentation

- [Architecture Documentation](docs/ARCHITECTURE.md)
- [Inspection Worker Service](docs/INSPECTION-WORKER.md)
- [Message Producer Samples](docs/MESSAGE-PRODUCER-SAMPLES.md)
- [Docker Testing](docs/DOCKER_TESTING.md)
- [CI/CD Documentation](docs/CI-CD.md)

## 🤝 Contributing

This is a foundational scaffold for the VisionFlow platform. Business logic and features will be added in subsequent iterations.

## 📝 License

Copyright © 2024 VisionFlow Platform
