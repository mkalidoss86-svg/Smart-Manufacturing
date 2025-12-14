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

