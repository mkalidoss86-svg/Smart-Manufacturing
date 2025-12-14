# VisionFlow – Smart Manufacturing Quality Platform

VisionFlow is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real-time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## 🏗️ Architecture Overview

This platform is built using a microservices architecture with the following core services:

- **DataIngestion.API** (Port 5001): Handles ingestion of manufacturing data from various sources
- **QualityAnalytics.API** (Port 5002): Processes and analyzes quality metrics
- **AlertNotification.API** (Port 5003): Manages alerts and notifications
- **Dashboard.API** (Port 5004): Provides API for dashboard and visualization

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Running Locally with Docker Compose

The easiest way to run all services locally is using Docker Compose:

```bash
# Build and start all services
docker compose up --build

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
dotnet build src/DataIngestion.API/DataIngestion.API.csproj
dotnet build src/QualityAnalytics.API/QualityAnalytics.API.csproj
dotnet build src/AlertNotification.API/AlertNotification.API.csproj
dotnet build src/Dashboard.API/Dashboard.API.csproj
```

### Project Structure

```
Smart-Manufacturing/
├── .github/
│   └── workflows/
│       └── ci.yml                    # GitHub Actions CI workflow
├── src/
│   ├── DataIngestion.API/            # Data ingestion service
│   ├── QualityAnalytics.API/         # Quality analytics service
│   ├── AlertNotification.API/        # Alert notification service
│   └── Dashboard.API/                # Dashboard API service
├── infrastructure/
│   ├── k8s/                          # Kubernetes manifests
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── deployments.yaml
│   │   └── services.yaml
│   └── docker/                       # Docker configurations
├── docs/
│   └── ARCHITECTURE.md               # Detailed architecture documentation
├── docker-compose.yml                # Local development compose file
├── global.json                       # .NET SDK version
└── README.md                         # This file
```

## 🔄 CI/CD

The project uses GitHub Actions for continuous integration. The CI pipeline:

1. Builds all services using .NET 8
2. Runs tests (if available)
3. Builds Docker images for each service
4. Validates Docker images

See `.github/workflows/ci.yml` for details.

## 📖 Documentation

For detailed architecture documentation, see [ARCHITECTURE.md](docs/ARCHITECTURE.md)

## 🤝 Contributing

This is a foundational scaffold for the VisionFlow platform. Business logic and features will be added in subsequent iterations.

## 📝 License

Copyright © 2024 VisionFlow Platform

