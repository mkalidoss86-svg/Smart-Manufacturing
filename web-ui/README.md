# VisionFlow Web UI Service

A lightweight, real-time web interface for the VisionFlow Smart Manufacturing Quality Platform.

## Overview

The Web UI service provides a simple, responsive dashboard for monitoring manufacturing quality in real-time. It displays live production line status, defects, anomalies, and system health.

## Features

- **Live Production Line Monitoring**: Real-time quality metrics for each production line
- **Defects & Anomalies Dashboard**: View recent quality issues with severity indicators
- **Real-time Notifications**: WebSocket-based instant updates for quality events
- **System Health Monitoring**: Track the health status of all platform services
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Configuration via Environment Variables**: Easy deployment and configuration

## Architecture

### Design Principles

- **No Business Logic**: UI is purely presentational, all logic resides in backend services
- **API-Driven**: All data fetched from backend REST APIs
- **Real-time Updates**: WebSocket connection for instant notifications
- **Stateless**: No server-side state, can be horizontally scaled
- **Environment Configuration**: All settings via environment variables

### Technology Stack

- **Frontend**: Vanilla JavaScript (ES6+), HTML5, CSS3
- **Server**: Node.js with Express
- **Containerization**: Docker
- **Orchestration**: Docker Compose, Kubernetes-ready

## Getting Started

### Prerequisites

- Node.js 18+ (for local development)
- Docker (for containerized deployment)

### Local Development

1. Install dependencies:
```bash
npm install
```

2. Configure environment variables:
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. Start the server:
```bash
npm start
```

4. Open browser:
```
http://localhost:3000
```

### Docker Deployment

1. Build the image:
```bash
docker build -t visionflow-web-ui .
```

2. Run the container:
```bash
docker run -p 3000:3000 \
  -e API_BASE_URL=http://api-gateway:5000/api \
  -e WEBSOCKET_URL=ws://api-gateway:5000/ws \
  visionflow-web-ui
```

### Docker Compose

```bash
docker-compose up -d
```

## Configuration

All configuration is done via environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `PORT` | HTTP server port | `3000` |
| `API_BASE_URL` | Backend API base URL | `http://localhost:5000/api` |
| `WEBSOCKET_URL` | WebSocket server URL | `ws://localhost:5000/ws` |
| `REFRESH_INTERVAL` | Data refresh interval (ms) | `5000` |
| `MAX_DEFECTS_DISPLAY` | Maximum defects to show | `50` |
| `MAX_NOTIFICATIONS` | Maximum notifications to keep | `20` |

## API Integration

The UI expects the following API endpoints from the backend:

### REST Endpoints

- `GET /api/health` - System health status
- `GET /api/production-lines` - List all production lines
- `GET /api/production-lines/:id` - Get specific line details
- `GET /api/defects?limit=X` - Get recent defects
- `GET /api/defects?lineId=X&limit=Y` - Get defects for a line
- `GET /api/anomalies?limit=X` - Get recent anomalies

### WebSocket Events

The UI subscribes to real-time events via WebSocket:

```javascript
{
  "type": "defect" | "anomaly" | "line-status" | "health-update",
  "severity": "low" | "medium" | "high",
  "message": "Human-readable message",
  "data": { /* event-specific data */ }
}
```

## Project Structure

```
web-ui/
├── public/
│   ├── index.html          # Main HTML page
│   ├── css/
│   │   └── styles.css      # Stylesheet
│   └── js/
│       ├── config.js       # Configuration handling
│       ├── api-client.js   # REST API client
│       ├── websocket-client.js  # WebSocket client
│       └── app.js          # Main application logic
├── server.js               # Express server
├── package.json            # Node.js dependencies
├── Dockerfile              # Container image definition
├── docker-compose.yml      # Multi-container setup
└── README.md              # This file
```

## Development Guidelines

### Adding New Features

1. **Keep it simple**: UI should remain lightweight and focused
2. **No business logic**: All logic belongs in backend services
3. **Use existing patterns**: Follow the established code structure
4. **Environment config**: Use environment variables for all settings

### Code Style

- Use vanilla JavaScript (no frameworks)
- Keep functions small and focused
- Add comments for complex logic
- Follow existing naming conventions

## Health Check

The service exposes a health check endpoint:

```bash
curl http://localhost:3000/health
```

Response:
```json
{
  "status": "healthy",
  "service": "web-ui",
  "timestamp": "2025-12-14T07:52:00.000Z"
}
```

## Troubleshooting

### WebSocket Connection Issues

- Verify `WEBSOCKET_URL` is correct
- Check if WebSocket server is running
- Ensure firewall allows WebSocket connections

### API Connection Issues

- Verify `API_BASE_URL` is correct
- Check if backend services are running
- Review browser console for error messages

### Data Not Displaying

- Check API endpoints return correct data format
- Verify data structure matches expected format
- Review browser console for parsing errors

## Security Considerations

- No sensitive data stored in frontend
- All API calls go through backend gateway
- No direct database connections
- Environment variables for configuration
- Runs as non-root user in container

## Performance

- Lightweight: < 5MB Docker image
- Fast startup: < 2 seconds
- Low resource usage: ~50MB memory
- Efficient: Minimal CPU usage
- Scalable: Stateless, can run multiple instances

## Future Enhancements

Potential improvements (while maintaining simplicity):

- [ ] Dark mode theme
- [ ] Customizable dashboards
- [ ] Export data to CSV
- [ ] Filtering and search
- [ ] Historical data visualization
- [ ] User preferences storage

## License

Copyright © 2025 VisionFlow. All rights reserved.
