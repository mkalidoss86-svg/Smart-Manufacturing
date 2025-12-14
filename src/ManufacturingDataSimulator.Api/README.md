# Manufacturing Data Simulator - Quick Start

Generate realistic manufacturing quality data for the VisionFlow platform in minutes.

## 🚀 Quick Start (Docker)

```bash
# Start all services including the simulator
docker compose up -d

# View simulator logs
docker logs -f simulator-worker

# Test the API
curl http://localhost:5200/api/events/recent?count=10
```

**Services Available:**
- Simulator API: http://localhost:5200
- Swagger UI: http://localhost:5200/swagger
- Web UI: http://localhost:8080

## 📊 What It Does

The simulator generates realistic manufacturing events with:
- ✅ Multiple production lines (Line-A, Line-B, Line-C)
- ✅ Configurable defect rates (default 10%)
- ✅ Various defect types (Scratch, Dent, Crack, etc.)
- ✅ Severity levels (Low, Medium, High, Critical)
- ✅ Environmental metadata (temperature, humidity, pressure)
- ✅ Real-time notifications via SignalR
- ✅ Persistent storage in SQLite/PostgreSQL

## 🔧 Configuration

Edit `docker-compose.yml` or `appsettings.json`:

```yaml
Simulator__EventRatePerSecond: 2          # Events per second
Simulator__DefectPercentage: 10.0         # 10% defect rate
Simulator__EnableBurstMode: false         # Periodic high-volume bursts
Simulator__ContinuousMode: true           # Run continuously
```

## 📖 API Examples

### Generate an event
```bash
curl -X POST http://localhost:5200/api/events
```

### Generate 10 events
```bash
curl -X POST http://localhost:5200/api/events/batch?count=10
```

### Query events
```bash
# Recent events
curl http://localhost:5200/api/events/recent?count=50

# Filter by production line
curl "http://localhost:5200/api/events?productionLine=Line-A&page=1&pageSize=20"

# Filter by status
curl "http://localhost:5200/api/events?status=Fail"

# Get production line statistics
curl http://localhost:5200/api/stats/production-lines
```

## 🧪 Local Development

### Prerequisites
- .NET 8.0 SDK
- SQLite (included) or PostgreSQL

### Run the API
```bash
cd src/ManufacturingDataSimulator.Api
dotnet run
```

### Run the Worker
```bash
cd src/ManufacturingDataSimulator.Worker
dotnet run
```

### Run Tests
```bash
cd tests/ManufacturingDataSimulator.Tests
dotnet test
```

## 🏗️ Architecture

```
Worker (Continuous Generation)
    ↓
SimulatorService
    ↓
EventFactory → DefectDistributionStrategy
    ↓
Repository → SQLite/PostgreSQL
    ↓
SignalREventPublisher → NotificationService
```

## 📚 Documentation

- **Full Documentation**: [docs/MANUFACTURING_DATA_SIMULATOR.md](../docs/MANUFACTURING_DATA_SIMULATOR.md)
- **API Reference**: http://localhost:5200/swagger (when running)
- **Architecture Details**: See main documentation

## ⚙️ Design Patterns

- **Factory Pattern**: Event generation with realistic data
- **Strategy Pattern**: Pluggable defect distribution algorithms
- **Repository Pattern**: Clean data access abstraction
- **Observer Pattern**: Real-time event publishing

## 🎯 Key Features

1. **Configurable**: Event rate, defect %, production lines
2. **Realistic**: Weighted defect distribution, environmental data
3. **Scalable**: Horizontal scaling, high-throughput
4. **Observable**: Real-time SignalR notifications
5. **Testable**: 100% test coverage, deterministic seeding
6. **Production-Ready**: Docker, K8s, health checks, logging

## 🔍 Monitoring

### Health Check
```bash
curl http://localhost:5200/health
```

### View Logs
```bash
# Docker
docker logs simulator-worker
docker logs simulator-api

# Local
# Logs appear in console with structured JSON format
```

## 🤝 Integration

### With Web UI
The UI automatically displays:
- Production line status and metrics
- Recent defects table
- Real-time notifications

### With NotificationService
Events are automatically published to SignalR clients.

### With Other Services
Use the REST API to integrate with:
- Analytics dashboards
- Alert systems
- Quality tracking systems
- Reporting tools

## 🛠️ Troubleshooting

**Problem**: No events generating
- Check `ContinuousMode: true` in configuration
- Verify Worker service is running

**Problem**: UI not updating
- Check NotificationService is running
- Verify SignalR HubUrl in configuration

**Problem**: Database errors
- For SQLite: Check disk space
- For PostgreSQL: Verify connection string

## 🚦 Production Deployment

1. **Use PostgreSQL** for better concurrent write performance
2. **Enable health checks** for Kubernetes probes
3. **Set appropriate event rate** based on load testing
4. **Monitor database size** and implement retention policy
5. **Use burst mode** for realistic production patterns

## 📄 License

Copyright © 2025 VisionFlow Smart Manufacturing Platform
