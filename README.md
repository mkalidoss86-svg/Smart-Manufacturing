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

## Getting Started

### Prerequisites

- .NET 8 SDK
- RabbitMQ (local or remote)

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Run

```bash
cd src/InspectionWorker
dotnet run
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

## License

[Your License Here]

