# API Documentation - VisionFlow Notification Service

## Overview

The VisionFlow Notification Service provides real-time push notifications for inspection results using SignalR WebSockets. It supports both REST API endpoints for publishing inspection results and SignalR hub connections for receiving real-time updates.

## Base URL

- **Development**: `http://localhost:5019`
- **Docker**: `http://localhost:8080`
- **Production**: `https://your-domain.com`

## Authentication

Currently, the API does not require authentication. For production deployments, implement authentication using:
- JWT Bearer tokens
- Azure AD / OAuth 2.0
- API Keys

## REST API Endpoints

### 1. Publish Inspection Result

Publish a new inspection result to all connected clients.

**Endpoint**: `POST /api/inspections`

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "id": "string",
  "productId": "string",
  "status": "string",
  "severity": "string",
  "message": "string",
  "timestamp": "2025-12-14T10:30:00Z",
  "metadata": {
    "key1": "value1",
    "key2": "value2"
  }
}
```

**Field Descriptions**:
- `id` (required): Unique identifier for the inspection
- `productId` (required): Product being inspected
- `status` (required): Inspection status (e.g., "Passed", "Failed", "Warning")
- `severity` (required): Issue severity (e.g., "Low", "Medium", "High", "Critical")
- `message` (required): Human-readable description
- `timestamp` (required): ISO 8601 timestamp
- `metadata` (optional): Additional key-value data

**Response**: `200 OK`
```json
{
  "message": "Inspection result published successfully"
}
```

**Error Response**: `500 Internal Server Error`
```json
{
  "error": "Failed to publish inspection result"
}
```

**Example using cURL**:
```bash
curl -X POST http://localhost:8080/api/inspections \
  -H "Content-Type: application/json" \
  -d '{
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
  }'
```

### 2. Get Missed Events

Retrieve events that occurred after a specific sequence number. Used for client reconnection recovery.

**Endpoint**: `GET /api/inspections/missed-events`

**Query Parameters**:
- `lastSequenceNumber` (required): Last received sequence number
- `maxCount` (optional): Maximum events to return (default: 100, max: 1000)

**Response**: `200 OK`
```json
[
  {
    "eventId": "uuid",
    "eventType": "InspectionUpdate",
    "payload": {
      "id": "INS-001",
      "productId": "PROD-123",
      "status": "Failed",
      "severity": "High",
      "message": "Detected surface defect",
      "timestamp": "2025-12-14T10:30:00Z",
      "metadata": {
        "defectType": "Scratch"
      }
    },
    "createdAt": "2025-12-14T10:30:01Z",
    "sequenceNumber": 42
  }
]
```

**Example using cURL**:
```bash
curl "http://localhost:8080/api/inspections/missed-events?lastSequenceNumber=10&maxCount=50"
```

### 3. Get Latest Sequence Number

Retrieve the latest sequence number for synchronization.

**Endpoint**: `GET /api/inspections/latest-sequence`

**Response**: `200 OK`
```json
{
  "sequenceNumber": 42
}
```

**Example using cURL**:
```bash
curl http://localhost:8080/api/inspections/latest-sequence
```

### 4. Health Check

Check if the service is running and healthy.

**Endpoint**: `GET /health`

**Response**: `200 OK`
```
Healthy
```

**Example using cURL**:
```bash
curl http://localhost:8080/health
```

## SignalR Hub

### Hub URL

**Endpoint**: `/hubs/inspections`

**Full URL Examples**:
- `http://localhost:5019/hubs/inspections`
- `http://localhost:8080/hubs/inspections`
- `wss://your-domain.com/hubs/inspections` (production with SSL)

### Client Methods (Server to Client)

These methods are invoked by the server and should be handled by the client.

#### 1. Connected

Sent when a client successfully connects to the hub.

**Method**: `Connected`

**Payload**:
```json
{
  "connectionId": "string",
  "timestamp": "2025-12-14T10:30:00Z"
}
```

**JavaScript Example**:
```javascript
connection.on("Connected", (data) => {
    console.log(`Connected with ID: ${data.connectionId}`);
});
```

#### 2. InspectionUpdate

Sent when a new inspection result is published.

**Method**: `InspectionUpdate`

**Payload**: InspectionResult object
```json
{
  "id": "INS-001",
  "productId": "PROD-123",
  "status": "Failed",
  "severity": "High",
  "message": "Detected surface defect",
  "timestamp": "2025-12-14T10:30:00Z",
  "metadata": {
    "defectType": "Scratch"
  }
}
```

**JavaScript Example**:
```javascript
connection.on("InspectionUpdate", (inspectionResult) => {
    console.log("New inspection:", inspectionResult);
    updateUI(inspectionResult);
});
```

#### 3. NotificationEvent

Sent for general notification events.

**Method**: `NotificationEvent`

**Payload**: NotificationEvent object
```json
{
  "eventId": "uuid",
  "eventType": "InspectionUpdate",
  "payload": { /* InspectionResult */ },
  "createdAt": "2025-12-14T10:30:00Z",
  "sequenceNumber": 42
}
```

**JavaScript Example**:
```javascript
connection.on("NotificationEvent", (event) => {
    console.log(`Event ${event.eventType}:`, event.payload);
});
```

#### 4. Subscribed

Confirmation that subscription was successful.

**Method**: `Subscribed`

**Payload**: Topic name (string)

**JavaScript Example**:
```javascript
connection.on("Subscribed", (topic) => {
    console.log(`Subscribed to: ${topic}`);
});
```

#### 5. Unsubscribed

Confirmation that unsubscription was successful.

**Method**: `Unsubscribed`

**Payload**: Topic name (string)

**JavaScript Example**:
```javascript
connection.on("Unsubscribed", (topic) => {
    console.log(`Unsubscribed from: ${topic}`);
});
```

### Server Methods (Client to Server)

These methods can be invoked by the client on the server.

#### 1. SubscribeToInspections

Subscribe to receive inspection updates.

**Method**: `SubscribeToInspections`

**Parameters**: None

**Returns**: Task

**JavaScript Example**:
```javascript
await connection.invoke("SubscribeToInspections");
```

#### 2. UnsubscribeFromInspections

Unsubscribe from inspection updates.

**Method**: `UnsubscribeFromInspections`

**Parameters**: None

**Returns**: Task

**JavaScript Example**:
```javascript
await connection.invoke("UnsubscribeFromInspections");
```

#### 3. GetLastSequenceNumber

Get the last sequence number tracked for this connection.

**Method**: `GetLastSequenceNumber`

**Parameters**: None

**Returns**: long (sequence number)

**JavaScript Example**:
```javascript
const lastSeq = await connection.invoke("GetLastSequenceNumber");
console.log(`Last sequence: ${lastSeq}`);
```

#### 4. UpdateSequenceNumber

Update the sequence number for this connection.

**Method**: `UpdateSequenceNumber`

**Parameters**:
- `sequenceNumber` (long): New sequence number

**Returns**: Task

**JavaScript Example**:
```javascript
await connection.invoke("UpdateSequenceNumber", 42);
```

## Client Implementation Examples

### JavaScript/TypeScript

#### Basic Connection

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:8080/hubs/inspections")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Handle connection events
connection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
});

connection.onreconnected((connectionId) => {
    console.log("Reconnected:", connectionId);
});

connection.onclose((error) => {
    console.log("Connection closed:", error);
});

// Start connection
await connection.start();
console.log("Connected to SignalR");
```

#### Subscribe and Handle Updates

```javascript
// Subscribe to inspection updates
await connection.invoke("SubscribeToInspections");

// Handle inspection updates
connection.on("InspectionUpdate", (inspectionResult) => {
    console.log("Received inspection:", inspectionResult);
    
    // Update UI
    displayInspection(inspectionResult);
    
    // Store sequence number for recovery
    if (inspectionResult.sequenceNumber) {
        localStorage.setItem("lastSequence", inspectionResult.sequenceNumber);
    }
});
```

#### Reconnection with Missed Events

```javascript
connection.onreconnected(async (connectionId) => {
    console.log("Reconnected with ID:", connectionId);
    
    // Get last known sequence number
    const lastSeq = localStorage.getItem("lastSequence") || 0;
    
    // Fetch missed events via REST API
    const response = await fetch(
        `http://localhost:8080/api/inspections/missed-events?lastSequenceNumber=${lastSeq}`
    );
    const missedEvents = await response.json();
    
    // Process missed events
    missedEvents.forEach(event => {
        console.log("Missed event:", event);
        displayInspection(event.payload);
        
        // Update sequence
        if (event.sequenceNumber) {
            localStorage.setItem("lastSequence", event.sequenceNumber);
        }
    });
});
```

### C# Client

```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:8080/hubs/inspections")
    .WithAutomaticReconnect()
    .Build();

// Handle inspection updates
connection.On<InspectionResult>("InspectionUpdate", (inspectionResult) =>
{
    Console.WriteLine($"Received inspection: {inspectionResult.Id}");
});

// Start connection
await connection.StartAsync();

// Subscribe
await connection.InvokeAsync("SubscribeToInspections");

// Keep connection alive
await Task.Delay(Timeout.Infinite);
```

### Python Client

```python
from signalrcore.hub_connection_builder import HubConnectionBuilder
import time

def on_inspection_update(data):
    print(f"Received inspection: {data}")

connection = HubConnectionBuilder()\
    .with_url("http://localhost:8080/hubs/inspections")\
    .with_automatic_reconnect({
        "type": "interval",
        "intervals": [0, 2, 5, 10, 30]
    })\
    .build()

connection.on("InspectionUpdate", on_inspection_update)

connection.start()

# Subscribe to inspections
connection.send("SubscribeToInspections", [])

# Keep running
while True:
    time.sleep(1)
```

## Error Handling

### Common Error Codes

- **400 Bad Request**: Invalid request payload
- **500 Internal Server Error**: Server-side error
- **503 Service Unavailable**: Service is starting or shutting down

### SignalR Connection Errors

- **Connection Failed**: Cannot reach server, check URL and network
- **Disconnected**: Connection lost, automatic reconnection will attempt
- **Timeout**: No response from server within timeout period

### Best Practices

1. **Always implement automatic reconnection**
2. **Store sequence numbers for recovery**
3. **Handle missed events on reconnection**
4. **Implement exponential backoff for failed connections**
5. **Log all errors for debugging**
6. **Implement circuit breaker pattern for resilience**

## Rate Limits

Currently, no rate limits are enforced. For production:

- Consider implementing rate limiting on `/api/inspections` endpoint
- Configure SignalR connection limits in configuration
- Monitor and alert on unusual traffic patterns

## Versioning

API Version: 1.0

Future versions will be accessed via URL versioning:
- `/api/v2/inspections`
- `/hubs/v2/inspections`

## Support

For API questions or issues:
- GitHub Issues: [Repository URL]
- Email: api-support@yourdomain.com
- Documentation: [Wiki URL]

## Changelog

### Version 1.0 (2025-12-14)
- Initial release
- SignalR hub for real-time notifications
- REST API for inspection publishing
- Missed events recovery mechanism
- Health check endpoint
