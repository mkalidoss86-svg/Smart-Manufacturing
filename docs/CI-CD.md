# CI/CD Pipeline Documentation

## Overview

The VisionFlow platform uses GitHub Actions for continuous integration and continuous delivery. The pipeline is designed to be deterministic, fail-fast, and provide clear feedback on build status.

## Pipeline Architecture

### Workflow File
Location: `.github/workflows/ci.yml`

### Trigger Events
The pipeline runs on:
- **Push** to `main` or `develop` branches
- **Pull Requests** targeting `main` or `develop` branches

## Pipeline Stages

### 1. Build and Test Services

**Purpose**: Compile all microservices and run unit tests

**Strategy**: 
- Matrix build for parallel execution
- Fail-fast enabled (stops all jobs on first failure)

**Services Built**:
- DataIngestion.API
- QualityAnalytics.API
- AlertNotification.API
- Dashboard.API

**Steps**:
1. Checkout code
2. Setup .NET 8.0 SDK
3. Restore NuGet dependencies
4. Build in Release configuration
5. Run unit tests (if available)

**Exit Conditions**:
- Build fails → Pipeline stops
- Test fails → Pipeline stops
- All builds succeed → Continue to Docker build

### 2. Build Docker Images

**Purpose**: Create containerized versions of all services

**Dependencies**: Requires successful completion of Build and Test stage

**Strategy**:
- Matrix build for parallel image creation
- Fail-fast enabled

**Process**:
1. Checkout code
2. Setup Docker Buildx for efficient builds
3. Build Docker image for each service
4. Verify image was created successfully
5. Save image as tar file
6. Upload as artifact for validation stage

**Image Naming Convention**:
```
visionflow/[service-name]:[git-sha]
```

**Exit Conditions**:
- Image build fails → Pipeline stops
- All images built → Continue to validation

### 3. Docker Compose Validation

**Purpose**: Verify all services start correctly and pass health checks

**Dependencies**: Requires successful Docker image builds

**Process**:
1. Download all Docker image artifacts
2. Load images into Docker daemon
3. Tag images for docker-compose compatibility
4. Create CI-specific docker-compose override
5. Start all services with docker-compose
6. Wait for services to initialize (30 seconds)
7. Validate health endpoints for each service
8. Stop and cleanup services

**Health Check Endpoints**:
- DataIngestion API: http://localhost:5001/health
- QualityAnalytics API: http://localhost:5002/health
- AlertNotification API: http://localhost:5003/health
- Dashboard API: http://localhost:5004/health

**Exit Conditions**:
- Any service fails to start → Show logs and fail
- Any health check fails → Show logs and fail
- All services healthy → Continue to success gate

### 4. CI Success Gate

**Purpose**: Provide clear signal that all checks passed

**Dependencies**: All previous stages must succeed

This job only runs if all previous stages complete successfully, providing a single check status for branch protection rules.

## Fail-Fast Strategy

The pipeline is configured to fail immediately on errors:

1. **Matrix Strategy**: `fail-fast: true` stops all parallel jobs on first failure
2. **Exit Codes**: Commands use `|| exit 1` to propagate failures
3. **Job Dependencies**: Later stages don't run if earlier stages fail
4. **Health Checks**: Curl commands use `-f` flag to fail on HTTP errors

## Branch Protection

To block PR merges on failure:

1. Go to Repository Settings → Branches
2. Add branch protection rule for `main` and `develop`
3. Enable "Require status checks to pass before merging"
4. Select required checks:
   - Build and Test Services
   - Build Docker Images
   - Validate Docker Compose Startup
   - CI Pipeline Success

## Deterministic Builds

The pipeline ensures reproducible builds:

1. **Pinned Versions**:
   - actions/checkout@v4
   - actions/setup-dotnet@v4
   - docker/setup-buildx-action@v3
   - actions/upload-artifact@v4
   - actions/download-artifact@v4

2. **Explicit Dependencies**:
   - .NET SDK: 8.0.x (from global.json)
   - Docker base images: mcr.microsoft.com/dotnet/aspnet:8.0

3. **Configuration**:
   - Release build configuration
   - No-restore flag prevents unexpected package updates during build

## Environment Requirements

The pipeline requires:
- Ubuntu Linux runner (ubuntu-latest)
- Docker support
- Internet access for NuGet packages
- No special secrets or credentials

## Local Development Testing

To test the pipeline locally:

### Build Services
```bash
# Build all services
for service in DataIngestion.API QualityAnalytics.API AlertNotification.API Dashboard.API; do
  dotnet build src/$service/$service.csproj --configuration Release
done
```

### Build Docker Images
```bash
# Build individual service
docker build -t visionflow/dataingestion.api:local \
  -f src/DataIngestion.API/Dockerfile \
  src/DataIngestion.API
```

### Test with Docker Compose
```bash
# Build and start all services
docker compose up --build -d

# Check health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health

# View logs
docker compose logs

# Stop services
docker compose down
```

## Troubleshooting

### Build Failures

**Symptom**: Dotnet build fails
**Solutions**:
1. Check .csproj file for invalid package references
2. Verify .NET SDK version compatibility
3. Review build logs for specific errors

### Docker Build Failures

**Symptom**: Docker image build fails
**Solutions**:
1. Verify Dockerfile syntax
2. Check network connectivity for base image pull
3. Ensure source files are copied correctly

### Docker Compose Validation Failures

**Symptom**: Health check fails
**Solutions**:
1. Check service logs: `docker compose logs [service-name]`
2. Verify health endpoint returns 200 status
3. Increase wait time if services need longer to start
4. Check for port conflicts

### Artifact Issues

**Symptom**: Download artifact step fails
**Solutions**:
1. Verify upload step succeeded
2. Check artifact retention policy (1 day default)
3. Ensure artifact names match between upload/download

## Performance Optimization

Current optimizations:
1. **Parallel Builds**: Matrix strategy runs services in parallel
2. **Docker Buildx**: Multi-platform build support and caching
3. **Artifacts**: Images shared between jobs to avoid rebuilding
4. **No-Restore**: Build uses cached restore output

## Security Considerations

1. **Minimal Permissions**: Only read access to contents and write to PRs
2. **No Secrets**: Pipeline doesn't require secrets or credentials
3. **Local Only**: No deployment to external environments
4. **Base Images**: Uses official Microsoft .NET images

## Monitoring and Logs

Each job provides:
- Build output with timestamps
- Test results and coverage
- Docker build progress
- Service startup logs
- Health check responses

Failed jobs include:
- Error messages
- Stack traces
- Service logs (for docker-compose failures)

## Future Enhancements

Potential improvements:
1. Add code coverage reporting
2. Implement security scanning (CodeQL, container scanning)
3. Add performance benchmarks
4. Cache Docker layers between builds
5. Parallel health checks for faster validation
6. Integration test suite
7. API contract testing
