# Smart-Manufacturing

The Smart Manufacturing Quality Platform is a scalable, event-driven quality intelligence system designed to monitor manufacturing processes in near real time, detect quality anomalies, and support autonomous decision-making through Agentic AI.

## VisionFlow – Notification Service

A real-time notification service built with .NET 8 and SignalR for pushing inspection updates to clients.

### Features

- **Real-time Communication**: SignalR-based WebSocket connections for instant updates
- **Clean Architecture**: Separated into Domain, Application, Infrastructure, and API layers
- **Observer/Pub-Sub Pattern**: Decoupled event-driven architecture
- **Horizontal Scalability**: Redis backplane support for multi-instance deployments
- **Missed-Event Recovery**: Clients can retrieve events missed during disconnection
- **Connection Management**: Configurable connection limits and automatic reconnection handling
- **Health Monitoring**: Built-in health check endpoint
- **Structured Logging**: JSON-formatted logs for easy monitoring

### Architecture

```
├── NotificationService.Domain         # Core business entities and interfaces
│   ├── Entities                       # InspectionResult, NotificationEvent
│   └── Interfaces                     # INotificationPublisher, IEventStore
├── NotificationService.Application    # Business logic and use cases
│   ├── DTOs                          # Data transfer objects
│   ├── Services                      # InspectionNotificationService
│   └── Observers                     # Observer pattern interfaces
├── NotificationService.Infrastructure # External concerns
│   ├── Hubs                          # SignalR InspectionHub
│   ├── Publishers                    # SignalR implementation
│   └── EventStore                    # Redis-based event storage
└── NotificationService.Api           # Web API host
    └── Controllers                   # REST API endpoints
```

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (optional, for containerized deployment)
- Redis (optional, for horizontal scaling)

### Getting Started

#### Local Development (Without Redis)

1. Build the solution:
```bash
dotnet build VisionFlow.sln
```

2. Run the service:
```bash
cd src/NotificationService/NotificationService.Api
dotnet run
```

The service will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- SignalR Hub: /hubs/inspections
- Health Check: /health
- Swagger UI: /swagger

#### Docker Deployment (With Redis)

1. Build and start services:
```bash
docker-compose up --build
```

The service will be available at:
- HTTP: http://localhost:8080
- SignalR Hub: http://localhost:8080/hubs/inspections
- Health Check: http://localhost:8080/health
- Redis: localhost:6379

### API Endpoints

#### POST /api/inspections
Publish a new inspection result to all connected clients.

**Request Body:**
```json
{
  "id": "INS-001",
  "productId": "PROD-123",
  "status": "Failed",
  "severity": "High",
  "message": "Detected surface defect",
  "timestamp": "2025-12-14T10:30:00Z",
  "metadata": {
    "defectType": "Scratch",
    "location": "Top-Right",
    "confidence": 0.95
  }
}
```

#### GET /api/inspections/missed-events
Retrieve events missed during disconnection.

**Query Parameters:**
- `lastSequenceNumber`: Last received sequence number
- `maxCount`: Maximum number of events to retrieve (default: 100)

**Response:**
```json
[
  {
    "eventId": "evt-123",
    "eventType": "InspectionUpdate",
    "payload": { ... },
    "createdAt": "2025-12-14T10:30:00Z",
    "sequenceNumber": 42
  }
]
```

#### GET /api/inspections/latest-sequence
Get the latest sequence number.

**Response:**
```json
{
  "sequenceNumber": 42
}
```

### SignalR Hub Usage

#### Connect to Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:8080/hubs/inspections")
    .withAutomaticReconnect()
    .build();

await connection.start();
```

#### Subscribe to Inspection Updates
```javascript
await connection.invoke("SubscribeToInspections");

connection.on("InspectionUpdate", (inspectionResult) => {
    console.log("Received inspection update:", inspectionResult);
    // Update UI or process the result
});
```

#### Handle Reconnection and Missed Events
```javascript
connection.onreconnected(async () => {
    const lastSeq = localStorage.getItem("lastSequenceNumber") || 0;
    
    // Retrieve missed events
    const response = await fetch(
        `http://localhost:8080/api/inspections/missed-events?lastSequenceNumber=${lastSeq}`
    );
    const missedEvents = await response.json();
    
    // Process missed events
    missedEvents.forEach(event => {
        console.log("Missed event:", event);
    });
});

connection.on("InspectionUpdate", (inspectionResult) => {
    // Store latest sequence number
    localStorage.setItem("lastSequenceNumber", inspectionResult.sequenceNumber);
});
```

### Configuration

#### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"  // Empty for in-memory cache
  },
  "SignalR": {
    "MaxMessageSize": 102400,
    "ClientTimeoutSeconds": 60,
    "KeepAliveSeconds": 30
  },
  "ConnectionLimits": {
    "MaxConnections": 1000,
    "MaxUpgradedConnections": 1000
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200"
    ]
  }
}
```

### Horizontal Scaling

The service is designed for horizontal scaling using Redis as a backplane:

1. Configure Redis connection string in appsettings.json
2. Deploy multiple instances behind a load balancer
3. SignalR will use Redis to synchronize messages across instances
4. Each instance is stateless and can be scaled independently

### Health Monitoring

Check service health:
```bash
curl http://localhost:8080/health
```

### Logging

The service uses structured JSON logging. Logs include:
- Connection/disconnection events
- Inspection result publications
- Errors and exceptions
- Performance metrics

### Testing

Run tests (if available):
```bash
dotnet test
```

### Production Deployment

1. Set environment variables:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__Redis=<redis-connection-string>`

2. Configure reverse proxy (nginx, IIS, etc.) for:
   - WebSocket support
   - SSL/TLS termination
   - Load balancing

3. Monitor health endpoint for service availability

### Security Considerations

- Enable CORS only for trusted origins
- Use HTTPS in production
- Implement authentication/authorization for SignalR connections
- Configure connection limits to prevent DoS attacks
- Use Redis password authentication in production

### License

[Your License Here]

### Support

For issues and questions, please open an issue in the repository.

