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

### Ingestion API

A .NET 8 Minimal API service for ingesting production quality events.

**Features:**
- REST API for production quality events
- Input validation with FluentValidation
- Event enrichment (IDs, timestamps, lineId)
- RabbitMQ publishing with retry and circuit breaker patterns
- Structured logging with Serilog
- Health check endpoint
- Clean Architecture design

**Documentation:** [docs/INGESTION_API.md](docs/INGESTION_API.md)


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

**Documentation:** [docs/INSPECTION-WORKER.md](docs/INSPECTION-WORKER.md)

### Other Platform Services

- **DataIngestion API** (Port 5001): Ingests manufacturing data
- **QualityAnalytics API** (Port 5002): Analyzes quality metrics
- **AlertNotification API** (Port 5003): Manages alerts and notifications
- **Dashboard API** (Port 5004): Provides dashboard data

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
- RabbitMQ (for message-based services)
- Redis (for notification service backplane)
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
- Results API: http://localhost:5106 (HTTP) or https://localhost:7081 (HTTPS)
- Notification Service: http://localhost:8080 (HTTP) or http://localhost:8081 (SignalR)
- Web UI: http://localhost:8080

### Running Individual Services

#### Results API

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

#### Ingestion API

```bash
# Start RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Build and run
dotnet build VisionFlow.sln
cd src/IngestionApi/VisionFlow.IngestionApi
dotnet run
```

#### Notification Service

```bash
# Start Redis
docker run -d --name redis -p 6379:6379 redis:7-alpine

# Build and run
dotnet build VisionFlow.sln
dotnet run --project src/NotificationService/NotificationService.Api
```

## 🏥 Health Checks

Each service exposes a health endpoint at `/health`:

```bash
curl http://localhost:5001/health  # DataIngestion API
curl http://localhost:5002/health  # QualityAnalytics API
curl http://localhost:5003/health  # AlertNotification API
curl http://localhost:5004/health  # Dashboard API
curl http://localhost:5106/health  # Results API
curl http://localhost:8080/health  # Notification Service
```

## 📊 API Documentation

When running in Development mode, most services expose Swagger UI:

- DataIngestion API: http://localhost:5001/swagger
- QualityAnalytics API: http://localhost:5002/swagger
- AlertNotification API: http://localhost:5003/swagger
- Dashboard API: http://localhost:5004/swagger
- Results API: https://localhost:7081/swagger
- Ingestion API: Check service documentation

## 🐳 Docker

Each service has its own Dockerfile located in its respective directory:

```bash
# Build individual service image
docker build -t resultsapi:latest -f src/ResultsApi/Dockerfile src/ResultsApi

# Run individual service
docker run -p 5106:8080 resultsapi:latest
```

## ☸️ Kubernetes Deployment

Kubernetes manifests are located in `k8s/`:

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Apply ConfigMap
kubectl apply -f k8s/configmap.yaml

# Deploy all services using kustomize
kubectl apply -k k8s/

# Check deployment status
kubectl get pods -n visionflow
kubectl get services -n visionflow
```

## 🔧 Development

### Building All Services

```bash
# Build VisionFlow services
dotnet build VisionFlow.sln

# Build Results API (separate solution)
dotnet build SmartManufacturing.sln
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
│   ├── NotificationService/          # Notification service with SignalR
│   ├── IngestionApi/                 # Ingestion API service
│   │   └── VisionFlow.IngestionApi/
│   ├── Domain/                       # Shared domain layer
│   │   └── VisionFlow.Domain/
│   ├── Application/                  # Shared application layer
│   │   └── VisionFlow.Application/
│   ├── Infrastructure/               # Shared infrastructure layer
│   │   └── VisionFlow.Infrastructure/
│   ├── DataIngestion.API/            # Legacy data ingestion service
│   ├── QualityAnalytics.API/         # Quality analytics service
│   ├── AlertNotification.API/        # Alert notification service
│   ├── Dashboard.API/                # Dashboard API service
│   └── ResultsApi/                   # Results API service (inspection results)
├── infrastructure/
│   ├── k8s/                          # Kubernetes manifests
│   └── docker/                       # Docker configurations
├── web-ui/                           # Web UI service
├── docs/
│   ├── ARCHITECTURE.md               # Architecture documentation
│   ├── CI-CD.md                      # CI/CD documentation
│   ├── DOCKER_TESTING.md             # Docker testing guide
│   └── INGESTION_API.md              # Ingestion API documentation
├── docker-compose.yml                # Local development compose file
├── VisionFlow.sln                    # Main solution file
├── SmartManufacturing.sln            # Legacy solution (Results API)
└── README.md                         # This file
```

## Architecture

The platform follows Clean Architecture principles with:
- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: DTOs, interfaces, business logic, and use cases
- **Infrastructure Layer**: Repository implementations, external dependencies (RabbitMQ, Redis, databases)
- **API Layer**: RESTful endpoints and presentation using .NET Minimal APIs

## Technology Stack

- .NET 8.0
- ASP.NET Core Minimal APIs
- SignalR (for real-time communication)
- RabbitMQ (for message queuing)
- Redis (for distributed caching and SignalR backplane)
- FluentValidation (for input validation)
- Serilog (for structured logging)
- Clean Architecture
- Dependency Injection
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
- [Ingestion API Documentation](docs/INGESTION_API.md)
- [Results API Implementation](src/ResultsApi/IMPLEMENTATION_SUMMARY.md)
- [Notification Service API](API.md)
- [Deployment Guide](DEPLOYMENT.md)
- [CI/CD Failure Analysis](IMPLEMENTATION_SUMMARY.md)

## 🤝 Contributing

This is a foundational scaffold for the VisionFlow platform. Business logic and features will be added in subsequent iterations.

## 📝 License

Copyright © 2024 VisionFlow Smart Manufacturing Platform
