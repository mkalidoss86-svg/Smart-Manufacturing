# VisionFlow Results API

The Results API service is part of the VisionFlow Smart Manufacturing Quality Platform. It provides RESTful endpoints for storing and querying inspection results.

## Architecture

This service follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer**: Core business entities (InspectionResult)
- **Application Layer**: DTOs, interfaces, and CQRS-style queries
- **Infrastructure Layer**: In-memory repository implementation
- **API Layer**: .NET 8 Minimal API endpoints

## Features

- ✅ Store inspection results with in-memory persistence
- ✅ Query by lineId, status, and time range
- ✅ Pagination support
- ✅ Stateless service design
- ✅ Structured JSON logging
- ✅ Health check endpoint
- ✅ OpenAPI/Swagger documentation

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022, VS Code, or any text editor

### Running the Service

```bash
# Build the solution
dotnet build SmartManufacturing.sln

# Run the service
cd src/ResultsApi
dotnet run

# The service will start on https://localhost:5001 (HTTPS) and http://localhost:5000 (HTTP)
```

### Accessing Swagger UI

Navigate to `https://localhost:5001/swagger` in your browser to access the interactive API documentation.

## API Endpoints

### Health Check

```
GET /health
```

Returns the health status of the service.

**Response:**
- `200 OK`: Service is healthy

---

### Create Inspection Result

```
POST /api/results
```

Creates a new inspection result.

**Request Body:**
```json
{
  "lineId": "line-001",
  "status": "Pass",
  "timestamp": "2025-12-14T07:00:00Z",
  "productId": "PROD-12345",
  "defectType": null,
  "confidenceScore": 0.98,
  "metadata": {
    "operator": "John Doe",
    "shift": "Day"
  }
}
```

**Fields:**
- `lineId` (required): Manufacturing line identifier
- `status` (required): Inspection status (e.g., "Pass", "Fail")
- `timestamp` (required): When the inspection occurred
- `productId` (optional): Product identifier
- `defectType` (optional): Type of defect if status is "Fail"
- `confidenceScore` (optional): AI confidence score (0.0 - 1.0)
- `metadata` (optional): Additional key-value pairs

**Response:**
```json
{
  "id": "a47b817c-4433-4105-9d70-baf4a78a5479",
  "lineId": "line-001",
  "status": "Pass",
  "timestamp": "2025-12-14T07:00:00Z",
  "productId": "PROD-12345",
  "defectType": null,
  "confidenceScore": 0.98,
  "metadata": {
    "operator": "John Doe",
    "shift": "Day"
  }
}
```

**Status Codes:**
- `201 Created`: Result created successfully
- `500 Internal Server Error`: Failed to create result

---

### Get Inspection Result by ID

```
GET /api/results/{id}
```

Retrieves a specific inspection result by its ID.

**Parameters:**
- `id` (path): GUID of the inspection result

**Response:**
```json
{
  "id": "a47b817c-4433-4105-9d70-baf4a78a5479",
  "lineId": "line-001",
  "status": "Pass",
  "timestamp": "2025-12-14T07:00:00Z",
  "productId": "PROD-12345",
  "defectType": null,
  "confidenceScore": 0.98,
  "metadata": null
}
```

**Status Codes:**
- `200 OK`: Result found and returned
- `404 Not Found`: Result with specified ID does not exist
- `500 Internal Server Error`: Failed to retrieve result

---

### Query Inspection Results

```
GET /api/results?lineId={lineId}&status={status}&startTime={startTime}&endTime={endTime}&page={page}&pageSize={pageSize}
```

Query inspection results with optional filters and pagination.

**Query Parameters:**
- `lineId` (optional): Filter by manufacturing line ID
- `status` (optional): Filter by inspection status
- `startTime` (optional): Filter results after this timestamp (ISO 8601 format)
- `endTime` (optional): Filter results before this timestamp (ISO 8601 format)
- `page` (optional, default: 1): Page number (minimum: 1)
- `pageSize` (optional, default: 50): Results per page (minimum: 1, maximum: 100)

**Example Requests:**

```bash
# Get all results
GET /api/results

# Filter by line ID
GET /api/results?lineId=line-001

# Filter by status
GET /api/results?status=Fail

# Filter by time range
GET /api/results?startTime=2025-12-14T00:00:00Z&endTime=2025-12-14T23:59:59Z

# Combined filters with pagination
GET /api/results?lineId=line-001&status=Pass&page=1&pageSize=20
```

**Response:**
```json
{
  "items": [
    {
      "id": "a47b817c-4433-4105-9d70-baf4a78a5479",
      "lineId": "line-001",
      "status": "Pass",
      "timestamp": "2025-12-14T07:00:00Z",
      "productId": "PROD-12345",
      "defectType": null,
      "confidenceScore": 0.98,
      "metadata": null
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50
}
```

**Status Codes:**
- `200 OK`: Query successful (may return empty results)
- `500 Internal Server Error`: Failed to query results

---

## Configuration

The service is configured via `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ResultsApi": "Information"
    }
  },
  "ResultsApi": {
    "DefaultPageSize": 50,
    "MaxPageSize": 100,
    "RepositoryType": "InMemory"
  }
}
```

### Configuration Options

- `DefaultPageSize`: Default number of results per page
- `MaxPageSize`: Maximum allowed page size
- `RepositoryType`: Repository implementation ("InMemory" for now, designed for future database implementations)

## Logging

The service uses structured JSON logging for production-ready observability:

```json
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "Program",
  "Message": "Creating inspection result for line line-001 with status Pass",
  "State": {
    "LineId": "line-001",
    "Status": "Pass"
  }
}
```

## Design Considerations

### Stateless Design

The service is stateless and horizontally scalable:
- No session state
- All data is stored in the repository
- Multiple instances can run behind a load balancer

### Repository Abstraction

The `IInspectionResultRepository` interface allows for easy replacement:
- Current: In-memory implementation
- Future: SQL Server, PostgreSQL, MongoDB, etc.

Simply implement the interface and register in dependency injection.

### CQRS-Style Separation

While this is a read-focused service, the architecture separates:
- Commands: Create operations (POST)
- Queries: Read operations (GET)

This makes it easy to optimize each path independently in the future.

## Testing

### Manual Testing with curl

```bash
# Health check
curl -X GET http://localhost:5000/health

# Create a result
curl -X POST http://localhost:5000/api/results \
  -H "Content-Type: application/json" \
  -d '{
    "lineId": "line-001",
    "status": "Pass",
    "timestamp": "2025-12-14T07:00:00Z",
    "productId": "PROD-12345",
    "confidenceScore": 0.98
  }'

# Query results
curl -X GET "http://localhost:5000/api/results?lineId=line-001"

# Get by ID
curl -X GET "http://localhost:5000/api/results/{id}"
```

## Future Enhancements

The architecture is designed to support:

1. **Database Integration**: Replace in-memory repository with:
   - Entity Framework Core for SQL databases
   - MongoDB driver for document storage
   - Dapper for high-performance queries

2. **Caching Layer**: Add Redis or in-memory cache for frequently accessed queries

3. **Event Publishing**: Integrate with message brokers (RabbitMQ, Azure Service Bus) for event-driven architecture

4. **Authentication & Authorization**: Add JWT token validation and role-based access control

5. **Rate Limiting**: Protect endpoints with rate limiting middleware

6. **Metrics & Monitoring**: Add Prometheus metrics, Application Insights, or similar

## Technology Stack

- .NET 8.0
- ASP.NET Core Minimal APIs
- Swagger/OpenAPI (Swashbuckle)
- Built-in Dependency Injection
- Microsoft.Extensions.Logging

## License

This is part of the VisionFlow Smart Manufacturing Quality Platform.
