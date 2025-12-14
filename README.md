# Smart-Manufacturing

The Smart Manufacturing Quality Platform is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## Services

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

See [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) for complete documentation.

