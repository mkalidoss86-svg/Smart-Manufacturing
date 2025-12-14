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

### Build and Run

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

See [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) for complete documentation.

