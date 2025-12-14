# Smart-Manufacturing

The Smart Manufacturing Quality Platform is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## Services

### Results API

The Results API service provides RESTful endpoints for storing and querying inspection results.

**Key Features:**
- .NET 8 Minimal API
- Store inspection results with in-memory persistence
- Query by lineId, status, and time range
- Pagination support
- Stateless service design
- Clean Architecture with repository abstraction
- CQRS-style separation (read-focused)
- Structured logging and /health endpoint

**Documentation:** [src/ResultsApi/README.md](src/ResultsApi/README.md)

### Other Platform Services

- **DataIngestion API**: Handles data ingestion from manufacturing lines
- **QualityAnalytics API**: Performs quality analytics and anomaly detection
- **AlertNotification API**: Manages alert notifications and escalations
- **Dashboard API**: Provides dashboard and reporting capabilities
- **Web UI**: Interactive web interface for monitoring and management

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

- .NET 8 SDK or later
- Docker and Docker Compose (for containerized deployment)
- Node.js 18+ (for web-ui development)

### Quick Start with Docker Compose

The easiest way to run all services together:

```bash
# Clone the repository
git clone https://github.com/mkalidoss86-svg/Smart-Manufacturing.git
cd Smart-Manufacturing

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
- Results API: https://localhost:7081 or http://localhost:5106
- Web UI: http://localhost:8080

### Running Results API

To run the Results API service specifically:

```bash
# Build the solution
dotnet build SmartManufacturing.sln

# Run the Results API service
cd src/ResultsApi
dotnet run
```

The Results API will be available at:
- HTTPS: https://localhost:7081
- HTTP: http://localhost:5106
- Swagger UI: https://localhost:7081/swagger

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
curl http://localhost:5001/health  # DataIngestion API
curl http://localhost:5002/health  # QualityAnalytics API
curl http://localhost:5003/health  # AlertNotification API
curl http://localhost:5004/health  # Dashboard API
curl http://localhost:5106/health  # Results API
```

## 📊 API Documentation

When running in Development mode, each service exposes Swagger UI:

- DataIngestion API: http://localhost:5001/swagger
- QualityAnalytics API: http://localhost:5002/swagger
- AlertNotification API: http://localhost:5003/swagger
- Dashboard API: http://localhost:5004/swagger
- Results API: https://localhost:7081/swagger

## 🐳 Docker

Each service has its own Dockerfile located in its respective directory:

```bash
# Build individual service image
docker build -t dataingestion-api:latest -f src/DataIngestion.API/Dockerfile src/DataIngestion.API

# Run individual service
docker run -p 5001:5001 -e ASPNETCORE_URLS=http://+:5001 dataingestion-api:latest

# Build Results API
docker build -t resultsapi:latest -f src/ResultsApi/Dockerfile src/ResultsApi
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
dotnet build SmartManufacturing.sln
```

Or build individual services:

```bash
dotnet build src/DataIngestion.API/DataIngestion.API.csproj
dotnet build src/QualityAnalytics.API/QualityAnalytics.API.csproj
dotnet build src/AlertNotification.API/AlertNotification.API.csproj
dotnet build src/Dashboard.API/Dashboard.API.csproj
dotnet build src/ResultsApi/ResultsApi.csproj
```

### Project Structure

```
Smart-Manufacturing/
├── .github/
│   ├── scripts/                      # CI/CD automation scripts
│   └── workflows/
│       ├── ci.yml                    # GitHub Actions CI workflow
│       └── ci-pipeline.yml           # Full CI/CD pipeline
├── src/
│   ├── DataIngestion.API/            # Data ingestion service
│   ├── QualityAnalytics.API/         # Quality analytics service
│   ├── AlertNotification.API/        # Alert notification service
│   ├── Dashboard.API/                # Dashboard API service
│   └── ResultsApi/                   # Results API service (inspection results)
├── infrastructure/
│   ├── k8s/                          # Kubernetes manifests
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── deployments.yaml
│   │   └── services.yaml
│   └── docker/                       # Docker configurations
├── web-ui/                           # Web UI service
├── docs/
│   ├── ARCHITECTURE.md               # Detailed architecture documentation
│   ├── CI-CD.md                      # CI/CD documentation
│   └── DOCKER_TESTING.md             # Docker testing guide
├── docker-compose.yml                # Local development compose file
├── global.json                       # .NET SDK version
├── SmartManufacturing.sln            # Solution file
└── README.md                         # This file
```

## Architecture

The platform follows Clean Architecture principles with:
- **Domain Layer**: Core business entities
- **Application Layer**: DTOs, interfaces, and CQRS-style operations
- **Infrastructure Layer**: Repository implementations (currently in-memory, designed for future database integration)
- **API Layer**: RESTful endpoints using .NET Minimal APIs

## Technology Stack

- .NET 8.0
- ASP.NET Core Minimal APIs
- Clean Architecture
- Dependency Injection
- Structured Logging
- Docker & Docker Compose
- Kubernetes
- Node.js (Web UI)

## 🔄 CI/CD

The project uses GitHub Actions for continuous integration. The CI pipeline:

1. Builds all services using .NET 8
2. Runs tests (if available)
3. Builds Docker images for each service
4. Validates Docker images
5. Automated failure analysis with issue creation

See `.github/workflows/ci.yml` for details.

## 📖 Documentation

- [Architecture Documentation](docs/ARCHITECTURE.md)
- [CI/CD Documentation](docs/CI-CD.md)
- [Docker Testing Guide](docs/DOCKER_TESTING.md)
- [Results API Implementation](src/ResultsApi/IMPLEMENTATION_SUMMARY.md)
- [CI/CD Failure Analysis](IMPLEMENTATION_SUMMARY.md)

## 🤝 Contributing

This is a foundational scaffold for the VisionFlow platform. Business logic and features will be added in subsequent iterations.

## 📝 License

Copyright © 2024 VisionFlow Platform
