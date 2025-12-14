# VisionFlow – Smart Manufacturing Quality Platform

## Ingestion API Service

The Ingestion API is a .NET 8 Minimal API service that accepts production quality events via REST, validates them, enriches them with metadata, and publishes them to RabbitMQ for asynchronous processing.

## Architecture

This service follows **Clean Architecture** principles with clear separation of concerns:

```
src/
├── IngestionApi/          # API Layer (Presentation)
│   └── VisionFlow.IngestionApi
├── Application/           # Application Layer (Business Logic)
│   └── VisionFlow.Application
├── Infrastructure/        # Infrastructure Layer (External Dependencies)
│   └── VisionFlow.Infrastructure
└── Domain/               # Domain Layer (Core Entities)
    └── VisionFlow.Domain
```

### Layers

1. **Domain Layer** (`VisionFlow.Domain`)
   - Core business entities
   - No external dependencies
   - Contains `ProductionQualityEvent` entity

2. **Application Layer** (`VisionFlow.Application`)
   - Business logic and use cases
   - DTOs for data transfer
   - Input validation using FluentValidation
   - Service interfaces
   - Depends only on Domain layer

3. **Infrastructure Layer** (`VisionFlow.Infrastructure`)
   - RabbitMQ integration
   - Retry and Circuit Breaker patterns using Polly
   - Configuration management
   - Depends on Application layer

4. **API Layer** (`VisionFlow.IngestionApi`)
   - .NET 8 Minimal API endpoints
   - Dependency injection configuration
   - Structured logging with Serilog
   - Health checks
   - Depends on Application and Infrastructure layers

## Features

### ✅ Implemented Features

- **REST API Endpoint**: `POST /api/events` to accept production quality events
- **Input Validation**: Schema validation using FluentValidation
- **Event Enrichment**: Automatically adds EventId (GUID), Timestamp (UTC), and LineId
- **RabbitMQ Publishing**: Asynchronous event publishing to message queue
- **Retry Pattern**: Exponential backoff retry on failures (3 attempts)
- **Circuit Breaker**: Protects against cascading failures (threshold: 5, duration: 30s)
- **Structured Logging**: Serilog with console and file outputs
- **Health Check**: `/health` endpoint for monitoring
- **Stateless Design**: No state maintained between requests
- **Configuration Management**: Via appsettings.json and environment variables

## API Endpoints

### 1. Ingest Quality Event

**Endpoint:** `POST /api/events?lineId={lineId}`

**Request Body:**
```json
{
  "productId": "PROD-12345",
  "batchId": "BATCH-2024-001",
  "stationId": "STATION-A1",
  "qualityMetrics": {
    "temperature": 45.5,
    "pressure": 120.0,
    "viscosity": 2.3
  },
  "status": "Pass",
  "additionalData": {
    "operator": "John Doe",
    "shift": "Morning"
  }
}
```

**Query Parameters:**
- `lineId` (required): Production line identifier

**Response:**
- `202 Accepted`: Event successfully queued for processing
- `400 Bad Request`: Validation errors
- `500 Internal Server Error`: Processing error

**Validation Rules:**
- `productId`: Required, max 100 characters
- `batchId`: Required, max 100 characters
- `status`: Required, must be one of: Pass, Fail, Warning, Pending
- `qualityMetrics`: Required, at least one metric
- `stationId`: Optional, max 100 characters

### 2. Health Check

**Endpoint:** `GET /health`

**Response:**
- `200 OK`: Service is healthy

## Configuration

### appsettings.json

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "quality-events",
    "QueueName": "production-quality-events",
    "RoutingKey": "quality.event",
    "RetryCount": 3,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30
  }
}
```

### Environment Variables

You can override configuration using environment variables:
- `RabbitMQ__HostName`
- `RabbitMQ__Port`
- `RabbitMQ__UserName`
- `RabbitMQ__Password`
- etc.

## Getting Started

### Prerequisites

- .NET 8 SDK
- RabbitMQ server (local or remote)

### Running the Application

1. **Start RabbitMQ**
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Build the Solution**
   ```bash
   cd /home/runner/work/Smart-Manufacturing/Smart-Manufacturing
   dotnet build
   ```

3. **Run the API**
   ```bash
   cd src/IngestionApi/VisionFlow.IngestionApi
   dotnet run
   ```

4. **Access Swagger UI** (Development mode)
   - Navigate to: `https://localhost:5001/swagger`

### Testing with curl

```bash
curl -X POST "https://localhost:5001/api/events?lineId=LINE-001" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "PROD-12345",
    "batchId": "BATCH-2024-001",
    "stationId": "STATION-A1",
    "qualityMetrics": {
      "temperature": 45.5,
      "pressure": 120.0
    },
    "status": "Pass"
  }'
```

## Dependencies

### NuGet Packages

- **RabbitMQ.Client** - RabbitMQ client library
- **Polly** - Resilience and transient-fault-handling library
- **FluentValidation** - Validation library
- **FluentValidation.DependencyInjectionExtensions** - DI integration for FluentValidation
- **Serilog.AspNetCore** - Structured logging
- **Serilog.Sinks.Console** - Console logging sink
- **Serilog.Sinks.File** - File logging sink

## Design Patterns

### 1. Retry Pattern
- Exponential backoff retry strategy
- Configurable retry count (default: 3)
- Automatic retry on transient failures

### 2. Circuit Breaker Pattern
- Protects RabbitMQ from cascading failures
- Opens circuit after threshold failures (default: 5)
- Half-open state for testing recovery
- Configurable break duration (default: 30s)

### 3. Repository Pattern
- `IEventPublisher` interface abstracts message publishing
- Easy to mock for testing
- Supports dependency injection

### 4. Options Pattern
- Strongly-typed configuration
- `RabbitMqSettings` class for RabbitMQ configuration
- Integration with .NET configuration system

## Logging

The service uses Serilog for structured logging:

- **Console Output**: For development and debugging
- **File Output**: Rotating daily log files in `logs/` directory
- **Structured Data**: Logs include context like EventId, ProductId, LineId

### Log Levels

- `Information`: Normal operations
- `Warning`: Retry attempts
- `Error`: Circuit breaker events, failures
- `Fatal`: Application startup failures

## Event Flow

1. **Receive Request**: Client sends POST to `/api/events`
2. **Validate Input**: FluentValidation validates the DTO
3. **Enrich Event**: Add EventId, Timestamp, LineId
4. **Publish to RabbitMQ**: 
   - With retry pattern (3 attempts)
   - With circuit breaker protection
   - As durable, persistent message
5. **Return Response**: 202 Accepted or error

## RabbitMQ Configuration

The service automatically:
- Creates connection to RabbitMQ
- Declares exchange (type: topic, durable)
- Declares queue (durable)
- Binds queue to exchange with routing key

**Exchange:** `quality-events`  
**Queue:** `production-quality-events`  
**Routing Key:** `quality.event`

## Security Considerations

- Credentials stored in configuration (use secrets in production)
- HTTPS redirection enabled
- No sensitive data logged
- Stateless design (no session state)

## Production Deployment

### Recommendations

1. **Use Secret Management**
   - Azure Key Vault
   - AWS Secrets Manager
   - Kubernetes Secrets

2. **Configure Health Checks**
   - Add RabbitMQ health check
   - Monitor with Kubernetes liveness/readiness probes

3. **Scale Horizontally**
   - Stateless design supports multiple instances
   - Load balancer distributes traffic

4. **Monitor Metrics**
   - Event processing rate
   - Circuit breaker state
   - Queue depth

5. **Configure Resilience**
   - Adjust retry count based on workload
   - Tune circuit breaker thresholds

## Not Implemented

As per requirements, the following are **NOT** implemented:
- Consumer services (event processing)
- UI/Frontend
- Authentication/Authorization
- Database persistence
- Additional business logic

## License

Copyright © 2024 VisionFlow Smart Manufacturing Platform
