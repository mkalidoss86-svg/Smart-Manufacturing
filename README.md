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

