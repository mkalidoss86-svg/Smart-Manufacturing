# VisionFlow – Inspection Worker Service

## Overview

The Inspection Worker is a .NET 8 background service designed for the VisionFlow Smart Manufacturing Quality Platform. It processes inspection requests from a message queue, simulates inspection logic, and publishes results—enabling autonomous quality control in manufacturing processes.

## Features

- **Clean Architecture**: Organized into Domain, Application, Infrastructure, and Worker layers
- **Event-Driven**: Consumes and publishes messages via RabbitMQ
- **Inspection Strategies**: Uses the Strategy pattern for flexible inspection logic
- **Resilience**: Implements retry and dead-letter queue patterns
- **Idempotent Processing**: Prevents duplicate processing of the same request
- **Graceful Shutdown**: Properly handles shutdown signals
- **Health Checks**: Built-in RabbitMQ health monitoring
- **Structured Logging**: JSON-formatted logs for observability
- **Horizontal Scalability**: Stateless design supports multiple workers

## Architecture

### Layers

```
InspectionWorker/                 # Worker/Console application
├── Program.cs                    # DI configuration & startup
└── Worker.cs                     # Background service

InspectionWorker.Application/     # Business logic layer
├── Interfaces/                   # Service contracts
├── Services/                     # Application services
└── Strategies/                   # Inspection strategies (Strategy pattern)

InspectionWorker.Infrastructure/  # External concerns
├── Messaging/                    # RabbitMQ consumer & publisher
├── Configuration/                # Settings classes
└── HealthChecks/                 # Health check implementations

InspectionWorker.Domain/          # Core domain models
├── Entities/                     # Domain entities
├── Enums/                        # Status enums
└── Interfaces/                   # Domain contracts
```

### Design Patterns

- **Strategy Pattern**: Different inspection algorithms can be swapped without changing the service
- **Repository Pattern**: Abstraction of messaging infrastructure
- **Dependency Injection**: Loose coupling between layers

## Configuration

Configuration is managed via `appsettings.json`:

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "InspectionRequestQueue": "inspection-requests",
    "InspectionResultQueue": "inspection-results",
    "DeadLetterExchange": "inspection-dlx",
    "DeadLetterQueue": "inspection-dlq",
    "PrefetchCount": 10,
    "MaxRetryAttempts": 3
  }
}
```

### Environment Variables

Configuration can be overridden using environment variables:
- `RabbitMq__HostName`
- `RabbitMq__Port`
- `RabbitMq__UserName`
- `RabbitMq__Password`
- etc.

## Running the Worker

### Prerequisites

- .NET 8 SDK
- RabbitMQ instance (local or remote)

### Local Development

1. Start RabbitMQ (e.g., via Docker):
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the worker:
   ```bash
   cd src/InspectionWorker
   dotnet run
   ```

### Production Deployment

The worker can be deployed as:
- **Docker Container**: Use multi-stage Dockerfile for optimized image
- **Kubernetes Pod**: Deploy as Deployment with multiple replicas
- **Windows Service**: Use `dotnet publish` with SC.exe
- **Linux Systemd Service**: Configure as systemd unit

## Message Contracts

### Inspection Request (Input)

```json
{
  "requestId": "req-12345",
  "productId": "PROD-001",
  "batchId": "BATCH-2024-001",
  "timestamp": "2024-12-14T07:52:00Z",
  "measurements": {
    "temperature": 25.5,
    "pressure": 101.3
  }
}
```

### Inspection Result (Output)

```json
{
  "requestId": "req-12345",
  "productId": "PROD-001",
  "batchId": "BATCH-2024-001",
  "status": "Pass",
  "inspectedAt": "2024-12-14T07:52:01Z",
  "defectType": null,
  "confidenceScore": 0.95,
  "metadata": {
    "inspectionDurationMs": 125,
    "inspectorVersion": "1.0.0"
  }
}
```

### Inspection Status Values

- **Pass**: Product meets quality standards
- **Defect**: Product has identifiable defects (includes `defectType`)
- **Anomaly**: Product shows anomalous behavior (includes `anomalyScore`)

## Resilience & Error Handling

### Retry Strategy

Failed messages are retried up to `MaxRetryAttempts` times with incremental backoff:
1. First retry: immediate
2. Subsequent retries: via requeue mechanism
3. After max retries: moved to dead-letter queue

### Dead Letter Queue

Messages that cannot be processed after max retries are sent to the dead-letter queue for:
- Manual inspection
- Debugging
- Replay after fixes

### Idempotency

The service tracks processed request IDs to prevent duplicate processing. Each request is processed exactly once.

## Monitoring & Observability

### Structured Logging

All logs are output in JSON format with contextual information:
- Request IDs
- Timestamps
- Status codes
- Error details

### Health Checks

Built-in health check endpoint verifies:
- RabbitMQ connectivity
- Queue availability

Access health status programmatically for orchestration platforms.

## Scaling

### Horizontal Scaling

- Multiple worker instances can run concurrently
- RabbitMQ distributes messages via round-robin (with prefetch limit)
- Each worker is stateless (except in-memory idempotency cache)

### Concurrency Configuration

Adjust `PrefetchCount` to control message throughput per worker:
- Lower values: Better load distribution, slower processing
- Higher values: Faster processing, less even distribution

## Testing

Run unit tests:
```bash
dotnet test
```

Test coverage includes:
- Inspection strategy logic
- Service idempotency
- Cancellation handling
- Result validation

## Future Enhancements

- [ ] Add telemetry (OpenTelemetry/Application Insights)
- [ ] Implement persistent idempotency store (Redis/Database)
- [ ] Add metrics collection (Prometheus)
- [ ] Support additional inspection strategies
- [ ] Implement ML-based inspection models
- [ ] Add distributed tracing
- [ ] Performance benchmarking

## License

[Your License Here]

## Contributing

[Your Contributing Guidelines Here]
