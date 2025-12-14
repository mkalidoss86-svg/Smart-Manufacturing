# Deployment Guide - VisionFlow Notification Service

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Local Development](#local-development)
3. [Docker Deployment](#docker-deployment)
4. [Production Deployment](#production-deployment)
5. [Horizontal Scaling](#horizontal-scaling)
6. [Monitoring](#monitoring)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

- .NET 8 SDK (for local development)
- Docker and Docker Compose (for containerized deployment)
- Redis server (for horizontal scaling)
- Reverse proxy (nginx, IIS, or cloud load balancer for production)

## Local Development

### Step 1: Clone the Repository
```bash
git clone https://github.com/mkalidoss86-svg/Smart-Manufacturing.git
cd Smart-Manufacturing
```

### Step 2: Build the Solution
```bash
dotnet build VisionFlow.sln
```

### Step 3: Configure Application
Edit `src/NotificationService/NotificationService.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Redis": ""  // Leave empty for in-memory cache
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

### Step 4: Run the Service
```bash
cd src/NotificationService/NotificationService.Api
dotnet run
```

The service will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## Docker Deployment

### Single Instance (Without Redis)

Create a `docker-compose.override.yml`:
```yaml
version: '3.8'
services:
  notification-service:
    environment:
      - ConnectionStrings__Redis=
```

Build and run:
```bash
docker-compose up --build
```

### Multi-Instance (With Redis)

Use the default `docker-compose.yml`:
```bash
docker-compose up --build -d
docker-compose scale notification-service=3
```

## Production Deployment

### Step 1: Configure Environment Variables

Create a production configuration file or set environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__Redis="your-redis-connection-string"
export SignalR__MaxMessageSize=102400
export SignalR__ClientTimeoutSeconds=60
export ConnectionLimits__MaxConnections=5000
export Cors__AllowedOrigins__0="https://your-domain.com"
```

### Step 2: Build Production Image

```bash
docker build -t visionflow-notification:latest .
```

### Step 3: Deploy to Container Orchestration

#### Kubernetes Deployment

Create `k8s-deployment.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notification-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: notification-service
  template:
    metadata:
      labels:
        app: notification-service
    spec:
      containers:
      - name: notification-service
        image: visionflow-notification:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__Redis
          valueFrom:
            secretKeyRef:
              name: redis-secret
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: notification-service
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
  selector:
    app: notification-service
```

Deploy:
```bash
kubectl apply -f k8s-deployment.yaml
```

#### Docker Swarm Deployment

```bash
docker stack deploy -c docker-compose.yml visionflow
docker service scale visionflow_notification-service=3
```

### Step 4: Configure Reverse Proxy

#### Nginx Configuration

```nginx
upstream notification_backend {
    least_conn;
    server notification-service-1:8080;
    server notification-service-2:8080;
    server notification-service-3:8080;
}

map $http_upgrade $connection_upgrade {
    default upgrade;
    '' close;
}

server {
    listen 80;
    server_name notifications.yourdomain.com;

    location / {
        proxy_pass http://notification_backend;
        proxy_http_version 1.1;
        
        # WebSocket support
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
        
        # Standard headers
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Timeouts for long-lived connections
        proxy_read_timeout 3600s;
        proxy_send_timeout 3600s;
        proxy_connect_timeout 75s;
    }
}
```

## Horizontal Scaling

### Redis Configuration

#### Managed Redis Services
- **Azure Redis Cache**: Recommended for Azure deployments
- **AWS ElastiCache**: Recommended for AWS deployments
- **Redis Cloud**: Cloud-agnostic solution

#### Self-Hosted Redis

Install Redis:
```bash
docker run -d -p 6379:6379 \
  --name redis \
  -v redis-data:/data \
  redis:7-alpine redis-server --appendonly yes --requirepass your-password
```

Configure connection string:
```
redis-connection-string:6379,password=your-password,ssl=true,abortConnect=false
```

### Scaling Configuration

Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-connection-string"
  },
  "ConnectionLimits": {
    "MaxConnections": 5000,
    "MaxUpgradedConnections": 5000
  }
}
```

### Load Balancer Configuration

Configure your load balancer for:
- **Sticky Sessions**: Not required (stateless design)
- **WebSocket Support**: Must be enabled
- **Health Checks**: Use `/health` endpoint
- **Connection Timeout**: Set to at least 3600 seconds

## Monitoring

### Health Checks

Check service health:
```bash
curl http://your-service-url/health
```

Expected response: `Healthy`

### Logging

The service uses structured JSON logging. Configure log aggregation:

#### Using Serilog (Recommended)

Add to `Program.cs`:
```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq("http://seq-server:5341");
});
```

#### Using Application Insights (Azure)

Add NuGet package:
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Configure in `appsettings.Production.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

### Metrics

Monitor these key metrics:
- **Active Connections**: Number of connected SignalR clients
- **Messages/Second**: Throughput of inspection updates
- **Error Rate**: Failed message deliveries
- **Latency**: Time to deliver messages
- **Memory Usage**: Per instance
- **CPU Usage**: Per instance

### SignalR-Specific Monitoring

Log SignalR events:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Microsoft.AspNetCore.Http.Connections": "Debug"
    }
  }
}
```

## Troubleshooting

### Issue: Clients Cannot Connect

**Symptoms**: Connection timeouts, 404 errors

**Solutions**:
1. Verify SignalR hub route is correct: `/hubs/inspections`
2. Check CORS configuration matches client origin
3. Ensure WebSocket support is enabled on reverse proxy
4. Check firewall rules allow WebSocket traffic (port 8080/443)

### Issue: Messages Not Received

**Symptoms**: Clients connected but not receiving updates

**Solutions**:
1. Verify client subscribed to topic: `SubscribeToInspections()`
2. Check Redis connection if using multi-instance deployment
3. Review logs for publish errors
4. Verify inspection results are being published to API

### Issue: High Memory Usage

**Symptoms**: Memory consumption grows over time

**Solutions**:
1. Review event store retention (default: 24 hours)
2. Implement connection limits
3. Monitor for connection leaks
4. Consider reducing `MaxMessageSize` configuration

### Issue: Redis Connection Failures

**Symptoms**: Unable to connect to Redis, scaling not working

**Solutions**:
1. Verify Redis server is running and accessible
2. Check connection string format and credentials
3. Ensure Redis version is compatible (7.0+ recommended)
4. Review network security groups/firewall rules
5. Test connection: `redis-cli -h host -p port -a password ping`

### Issue: WebSocket Disconnections

**Symptoms**: Frequent client disconnections

**Solutions**:
1. Increase `ClientTimeoutSeconds` and `KeepAliveSeconds`
2. Check network stability and proxy configurations
3. Implement exponential backoff in client reconnection logic
4. Review load balancer idle timeout settings

### Debugging Steps

1. **Enable Detailed Errors** (Development only):
   ```json
   {
     "SignalR": {
       "EnableDetailedErrors": true
     }
   }
   ```

2. **Check Service Logs**:
   ```bash
   docker logs notification-service -f
   ```

3. **Test Health Endpoint**:
   ```bash
   curl -v http://localhost:8080/health
   ```

4. **Test Redis Connection**:
   ```bash
   redis-cli -h redis-host -p 6379 -a password ping
   ```

5. **Monitor Network Traffic**:
   ```bash
   tcpdump -i any port 8080 -nn -A
   ```

## Performance Tuning

### Connection Limits

Adjust based on expected load:
```json
{
  "ConnectionLimits": {
    "MaxConnections": 10000,
    "MaxUpgradedConnections": 10000
  }
}
```

### SignalR Options

Optimize for your use case:
```json
{
  "SignalR": {
    "MaxMessageSize": 102400,
    "ClientTimeoutSeconds": 60,
    "KeepAliveSeconds": 30,
    "HandshakeTimeout": 15,
    "MaximumParallelInvocations": 1
  }
}
```

### Redis Performance

1. Enable connection pooling
2. Use Redis Cluster for high availability
3. Configure appropriate eviction policy
4. Monitor Redis memory usage

## Security Best Practices

1. **Use HTTPS** in production
2. **Implement Authentication**: Add JWT or cookie authentication to SignalR hub
3. **Rate Limiting**: Implement rate limiting on API endpoints
4. **Secure Redis**: Use password authentication and TLS
5. **Principle of Least Privilege**: Run containers as non-root user
6. **Regular Updates**: Keep .NET runtime and dependencies updated
7. **Secrets Management**: Use Azure Key Vault, AWS Secrets Manager, or similar

## Backup and Recovery

### Event Store Backup

If using Redis for event storage:
```bash
redis-cli --rdb /backup/dump.rdb
```

### Configuration Backup

Store configuration in version control or configuration management system.

### Disaster Recovery

1. Deploy to multiple regions
2. Use managed Redis with automatic failover
3. Implement circuit breaker pattern for external dependencies
4. Regular backup of configuration and secrets

## Support

For issues and questions:
- GitHub Issues: [Repository URL]
- Documentation: [Wiki URL]
- Email: support@yourdomain.com
