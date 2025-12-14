# VisionFlow Kubernetes Manifests

This directory contains Kubernetes manifests for deploying the VisionFlow Smart Manufacturing Quality Platform.

## Architecture

The platform consists of the following components:

### Services
- **API Gateway**: Entry point for all API requests (Port 8080)
- **Quality Service**: Handles quality monitoring and anomaly detection (Port 8081)
- **Analytics Service**: Provides analytics and reporting capabilities (Port 8082)
- **Inspection Worker**: Scalable workers for processing inspection tasks

### Configuration
- **Namespace**: `visionflow` - Isolated namespace for all platform components
- **ConfigMap**: `visionflow-config` - Centralized configuration for all services

## Features

### Resource Management
All deployments include:
- **Resource Requests**: Guaranteed minimum resources
- **Resource Limits**: Maximum resource usage caps
- **Readiness Probes**: Ensures containers are ready before receiving traffic
- **Liveness Probes**: Automatic restart of unhealthy containers

### Scalability
- Services are configured with multiple replicas for high availability
- Inspection workers are designed for horizontal scaling (HPA-ready)
- All configuration is externalized via ConfigMaps (no hardcoded values)

## Deployment

### Prerequisites
- Kubernetes cluster (v1.20+)
- kubectl configured to access your cluster
- kustomize (optional, but recommended)

### Quick Start

#### Using kubectl with kustomization
```bash
# Deploy all resources
kubectl apply -k k8s/

# Verify deployment
kubectl get all -n visionflow

# Check pod status
kubectl get pods -n visionflow

# View logs
kubectl logs -n visionflow -l app=visionflow
```

#### Using kubectl directly
```bash
# Deploy resources in order
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/api-gateway.yaml
kubectl apply -f k8s/quality-service.yaml
kubectl apply -f k8s/analytics-service.yaml
kubectl apply -f k8s/inspection-worker.yaml
```

## Configuration

All configuration is managed through the `visionflow-config` ConfigMap. To update configuration:

```bash
# Edit the ConfigMap
kubectl edit configmap visionflow-config -n visionflow

# Or apply changes from file
kubectl apply -f k8s/configmap.yaml
```

After updating the ConfigMap, restart the affected pods:
```bash
kubectl rollout restart deployment/<deployment-name> -n visionflow
```

## Monitoring

### Check Service Health
```bash
# API Gateway
kubectl port-forward -n visionflow svc/api-gateway-service 8080:80
curl http://localhost:8080/health/ready

# Quality Service
kubectl port-forward -n visionflow svc/quality-service 8081:8081
curl http://localhost:8081/health/ready

# Analytics Service
kubectl port-forward -n visionflow svc/analytics-service 8082:8082
curl http://localhost:8082/health/ready
```

### View Logs
```bash
# All components
kubectl logs -n visionflow -l app=visionflow --tail=100 -f

# Specific component
kubectl logs -n visionflow -l component=api-gateway --tail=100 -f
kubectl logs -n visionflow -l component=quality-service --tail=100 -f
kubectl logs -n visionflow -l component=inspection-worker --tail=100 -f
```

## Scaling

### Manual Scaling
```bash
# Scale a specific deployment
kubectl scale deployment api-gateway -n visionflow --replicas=3
kubectl scale deployment inspection-worker -n visionflow --replicas=5
```

### Horizontal Pod Autoscaling (HPA)
The manifests are prepared for HPA. To enable:

```bash
# Example HPA for inspection-worker
kubectl autoscale deployment inspection-worker -n visionflow \
  --cpu-percent=70 \
  --min=3 \
  --max=10
```

## Cleanup

To remove all resources:
```bash
# Using kustomize
kubectl delete -k k8s/

# Or delete namespace (removes all resources)
kubectl delete namespace visionflow
```

## Resource Requirements

### Minimum Cluster Resources
- **CPU**: 3.4 cores (minimum requests)
- **Memory**: 5.5 GB (minimum requests)

### Recommended Cluster Resources
- **CPU**: 10+ cores (for limits + headroom)
- **Memory**: 16+ GB (for limits + headroom)

## Troubleshooting

### Pods not starting
```bash
# Check pod events
kubectl describe pod <pod-name> -n visionflow

# Check if ConfigMap exists
kubectl get configmap visionflow-config -n visionflow
```

### Service connectivity issues
```bash
# Check services
kubectl get svc -n visionflow

# Test internal connectivity
kubectl run -it --rm debug --image=busybox --restart=Never -n visionflow -- sh
# Inside the pod:
wget -O- http://api-gateway-service
```

### Configuration issues
```bash
# Verify ConfigMap data
kubectl get configmap visionflow-config -n visionflow -o yaml
```

## Notes

- All services use `IfNotPresent` image pull policy for local development
- No sensitive data is hardcoded; use Kubernetes Secrets for passwords/tokens
- The platform is designed to work with external dependencies (Postgres, Redis, Kafka)
- Health check endpoints are expected at `/health/ready` and `/health/live` for HTTP services
- **Important**: The inspection-worker uses basic process-based health checks. For production deployments, implement proper health endpoints or status files that the worker application manages to accurately reflect its health and readiness state.
