# Container Documentation

This document provides comprehensive container setup, configuration, and deployment instructions for the Prompt Template Manager application. The application supports both **Docker** and **Podman** as container runtimes.

## Container Runtime Support

This application is fully compatible with both:
- **Docker** - Traditional container runtime
- **Podman** - Daemonless, rootless container runtime (recommended for Fedora/RHEL)

All instructions work with both runtimes. Simply replace `docker` with `podman` in commands, or use the provided helper scripts which automatically detect the available runtime.

## Table of Contents

- [Quick Start](#quick-start)
- [Podman on Fedora](#podman-on-fedora)
- [Architecture](#architecture)
- [Building the Image](#building-the-image)
- [Running with Compose](#running-with-compose)
- [Running with CLI](#running-with-cli)
- [Configuration](#configuration)
- [Data Persistence](#data-persistence)
- [Development Mode](#development-mode)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)
- [Advanced Topics](#advanced-topics)

## Quick Start

### Using Helper Scripts (Recommended)

The helper scripts automatically detect whether you're using Docker or Podman:

```bash
# Build the image
./docker-build.sh

# Run the container
./docker-run.sh
```

### Using Compose

**With Docker:**
```bash
docker-compose up -d
```

**With Podman:**
```bash
podman compose up -d
```

### Access the Application

- Frontend & Backend: http://localhost:5026
- Health Check: http://localhost:5026/health
- API Documentation: http://localhost:5026/swagger

### Stop the Application

**Docker:**
```bash
docker-compose down
```

**Podman:**
```bash
podman compose down
```

## Podman on Fedora

Podman is the recommended container runtime for Fedora and RHEL-based systems. It offers several advantages:

- **Daemonless**: No background service required
- **Rootless**: Can run containers without root privileges
- **Docker-compatible**: Drop-in replacement for Docker CLI
- **Systemd integration**: Native support for systemd units

### Installing Podman

On Fedora (already installed by default on most systems):

```bash
sudo dnf install podman
```

Verify installation:

```bash
podman --version
```

### Podman Compose

Podman includes built-in Compose support (v4.0+):

```bash
podman compose version
```

All `docker-compose.yml` files are compatible with Podman Compose.

### Rootless Containers

Podman can run rootless containers by default. No special configuration needed:

```bash
# Runs as your user, no sudo required
podman compose up -d
```

### SELinux Considerations

On Fedora with SELinux enabled, you **must** add `:Z` to volume mounts for proper labeling:

```bash
# Private volume (recommended)
podman run -v $(pwd)/data:/app/data:Z proomptz:latest
```

The `:Z` flag tells SELinux to relabel the volume exclusively for this container. Use `:z` (lowercase) only if multiple containers need shared access.

**Important**: Ensure the data directory has proper permissions:

```bash
# Create directory with appropriate permissions
mkdir -p data
chmod 777 data  # Allows container's non-root user to write

# Or use chown with container UID
mkdir -p data
podman unshare chown 1000:1000 data
```

For docker-compose, add the `:Z` flag to volumes:

```yaml
volumes:
  - ./data:/app/data:Z
```

### Podman Desktop

For a GUI experience, install Podman Desktop:

```bash
flatpak install flathub io.podman_desktop.PodmanDesktop
```

## Architecture

The Dockerfile uses a **multi-stage build** approach with three stages:

1. **frontend-build**: Builds the React/Vite frontend application
2. **backend-build**: Builds the .NET 9.0 backend application
3. **runtime**: Combines both into a single lightweight runtime image

### Image Details

- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` (runtime)
- **Build Images**: Node.js 18 Alpine + .NET 9.0 SDK Alpine
- **Final Size**: ~200-300 MB (optimized)
- **Architecture**: Multi-arch support (amd64, arm64)

## Building the Image

### Using Helper Script (Recommended)

```bash
# Build with defaults
./docker-build.sh

# Build with custom tag
./docker-build.sh -t v1.0.0

# Build without cache
./docker-build.sh --no-cache

# Build for specific platform
./docker-build.sh --platform linux/amd64
```

### Build Manually

**Docker:**
```bash
docker build -t proomptz:latest .
```

**Podman:**
```bash
podman build -t proomptz:latest .
```

### Build with Custom Tag

**Docker:**
```bash
docker build -t myregistry/proomptz:v1.0.0 .
```

**Podman:**
```bash
podman build -t myregistry/proomptz:v1.0.0 .
```

### Multi-Platform Build

**Docker:**
```bash
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t proomptz:latest \
  .
```

**Podman:**
```bash
podman build \
  --platform linux/amd64,linux/arm64 \
  --manifest proomptz:latest \
  .
```

## Running with Compose

Replace `docker-compose` with `podman compose` when using Podman.

### Start Services

**Docker:**
```bash
# Start in detached mode
docker-compose up -d

# Start with logs
docker-compose up

# Rebuild and start
docker-compose up --build
```

**Podman:**
```bash
# Start in detached mode
podman compose up -d

# Start with logs
podman compose up

# Rebuild and start
podman compose up --build
```

### View Logs

**Docker:**
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f app
```

**Podman:**
```bash
# All services
podman compose logs -f

# Specific service
podman compose logs -f app
```

### Stop Services

**Docker:**
```bash
# Stop containers (keep data)
docker-compose stop

# Stop and remove containers
docker-compose down

# Remove containers and volumes (WARNING: deletes database)
docker-compose down -v
```

**Podman:**
```bash
# Stop containers (keep data)
podman compose stop

# Stop and remove containers
podman compose down

# Remove containers and volumes (WARNING: deletes database)
podman compose down -v
```

## Running with CLI

### Using Helper Script (Recommended)

```bash
# Run with defaults
./docker-run.sh

# Run on custom port
./docker-run.sh -p 8080

# Run in foreground
./docker-run.sh --foreground

# Run in development mode
./docker-run.sh -e Development
```

### Basic Run

**Docker:**
```bash
docker run -d \
  --name proomptz \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data \
  proomptz:latest
```

**Podman:**
```bash
podman run -d \
  --name proomptz \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data:z \
  proomptz:latest
```

Note: The `:z` flag on Podman enables SELinux relabeling for the volume.

### Run with Environment Variables

**Docker:**
```bash
docker run -d \
  --name proomptz \
  -p 5026:5026 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/prompttemplates.db" \
  -v $(pwd)/data:/app/data \
  proomptz:latest
```

**Podman:**
```bash
podman run -d \
  --name proomptz \
  -p 5026:5026 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/prompttemplates.db" \
  -v $(pwd)/data:/app/data:z \
  proomptz:latest
```

### Interactive Run (for debugging)

**Docker:**
```bash
docker run -it --rm \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data \
  proomptz:latest
```

**Podman:**
```bash
podman run -it --rm \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data:z \
  proomptz:latest
```

### Run with Custom Network

```bash
# Create network
docker network create proomptz-net

# Run container
docker run -d \
  --name proomptz \
  --network proomptz-net \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data \
  proomptz:latest
```

## Configuration

### Environment Variables

The following environment variables can be configured:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment (Development/Staging/Production) |
| `ASPNETCORE_URLS` | `http://+:5026` | URLs the server listens on |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/prompttemplates.db` | SQLite database connection string |

### Custom Configuration File

Create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/prompttemplates.db"
  }
}
```

Mount it to the container:

```bash
docker run -d \
  -v $(pwd)/appsettings.Production.json:/app/appsettings.Production.json \
  -v $(pwd)/data:/app/data \
  -p 5026:5026 \
  proomptz:latest
```

## Data Persistence

### SQLite Database Location

The SQLite database is stored at `/app/data/prompttemplates.db` inside the container.

### Volume Mounting

**Recommended**: Use a named volume or bind mount to persist data.

#### Bind Mount (Development)

```bash
docker run -d \
  -v $(pwd)/data:/app/data \
  -p 5026:5026 \
  proomptz:latest
```

#### Named Volume (Production)

```bash
# Create volume
docker volume create proomptz-data

# Use volume
docker run -d \
  -v proomptz-data:/app/data \
  -p 5026:5026 \
  proomptz:latest
```

### Backup Database

```bash
# Copy from container
docker cp proomptz:/app/data/prompttemplates.db ./backup/

# Backup with docker-compose
docker-compose exec app cp /app/data/prompttemplates.db /app/data/backup.db

# Backup using volume
docker run --rm \
  -v proomptz-data:/data \
  -v $(pwd)/backup:/backup \
  alpine tar czf /backup/db-backup-$(date +%Y%m%d).tar.gz -C /data .
```

### Restore Database

```bash
# Copy to container
docker cp ./backup/prompttemplates.db proomptz:/app/data/

# Restore using volume
docker run --rm \
  -v proomptz-data:/data \
  -v $(pwd)/backup:/backup \
  alpine tar xzf /backup/db-backup-YYYYMMDD.tar.gz -C /data
```

## Development Mode

### Using Development Containers

Uncomment the development services in `docker-compose.yml` to enable hot reload:

```yaml
services:
  backend-dev:
    # Enables .NET hot reload

  frontend-dev:
    # Enables Vite hot reload
```

Start development services:

```bash
docker-compose up backend-dev frontend-dev
```

### Accessing Development Tools

- Frontend Dev Server: http://localhost:5173
- Backend API: http://localhost:5026
- Swagger UI: http://localhost:5026/swagger

### Debugging in Container

Run with debugging enabled:

```bash
docker run -it --rm \
  -p 5026:5026 \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v $(pwd)/backend:/app \
  mcr.microsoft.com/dotnet/sdk:9.0 \
  dotnet watch run --project /app/src/PromptTemplateManager.Api/PromptTemplateManager.Api.csproj
```

## Production Deployment

### Docker Compose Production Setup

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  app:
    image: proomptz:latest
    container_name: proomptz-prod
    restart: always
    ports:
      - "80:5026"
    volumes:
      - proomptz-data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5026/health"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 10s
    logging:
      driver: json-file
      options:
        max-size: "10m"
        max-file: "3"

volumes:
  proomptz-data:
    driver: local
```

Deploy:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Behind a Reverse Proxy

#### Nginx Configuration

```nginx
server {
    listen 80;
    server_name proomptz.example.com;

    location / {
        proxy_pass http://localhost:5026;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### Traefik Labels

```yaml
services:
  app:
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.proomptz.rule=Host(`proomptz.example.com`)"
      - "traefik.http.services.proomptz.loadbalancer.server.port=5026"
```

### Container Registry

Push to Docker Hub:

```bash
docker tag proomptz:latest username/proomptz:latest
docker push username/proomptz:latest
```

Push to Private Registry:

```bash
docker tag proomptz:latest registry.example.com/proomptz:latest
docker push registry.example.com/proomptz:latest
```

## Troubleshooting

### Container Won't Start

Check logs:

```bash
docker logs proomptz
docker-compose logs app
```

Common issues:
- Port 5026 already in use: Change port mapping `-p 8080:5026`
- Permission issues: Check volume mount permissions
- Database locked: Ensure only one container accesses the database

### Application Errors

View detailed logs:

```bash
docker-compose exec app cat /app/logs/app.log
```

Access container shell:

```bash
docker exec -it proomptz sh
```

### Health Check Failures

Check health status:

```bash
docker inspect proomptz | grep -A 10 Health
```

Manually test health endpoint:

```bash
docker exec proomptz curl -f http://localhost:5026/health
```

### Database Issues

Check database file:

```bash
docker exec proomptz ls -lh /app/data/
docker exec proomptz sqlite3 /app/data/prompttemplates.db ".tables"
```

### Build Failures

Clean build cache:

```bash
docker builder prune
docker-compose build --no-cache
```

### Performance Issues

Check resource usage:

```bash
docker stats proomptz
```

Increase memory limits:

```yaml
services:
  app:
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M
```

### Common Issues

#### Getting 404 or Blank Page After Updates

**Symptom**: Browser shows 404 or nothing loads after updating code

**Cause**: Running old container image that doesn't have the latest changes

**Solution**: Rebuild and restart the container:

**Docker:**
```bash
docker-compose down
docker-compose build
docker-compose up -d
```

**Podman:**
```bash
podman compose down
podman compose build
podman compose up -d
```

Or force a full rebuild without cache:

```bash
podman compose build --no-cache
podman compose up -d
```

### Podman-Specific Issues

#### Permission Denied / Unable to Open Database

**Error**: `SQLite Error 14: 'unable to open database file'`

**Solution**: This occurs when the container's non-root user can't write to the volume. Fix with:

```bash
# Option 1: Set directory permissions
mkdir -p data
chmod 777 data

# Option 2: Use podman unshare (preferred)
mkdir -p data
podman unshare chown 1000:1000 data

# Then run with :Z flag for SELinux
podman run -v $(pwd)/data:/app/data:Z proomptz:latest
```

#### SELinux Denials

**Error**: Container can't access mounted volumes

**Solution**: Use the `:Z` flag for private volumes or `:z` for shared:

```bash
# Private volume (exclusive to this container)
podman run -v $(pwd)/data:/app/data:Z proomptz:latest

# Shared volume (multiple containers)
podman run -v $(pwd)/data:/app/data:z proomptz:latest
```

Or temporarily disable SELinux (not recommended for production):

```bash
sudo setenforce 0
```

#### HEALTHCHECK Not Supported Warning

**Warning**: `HEALTHCHECK is not supported for OCI image format`

This is informational only. Podman uses OCI format by default, which doesn't support HEALTHCHECK. The warning can be safely ignored, or build with Docker format:

```bash
podman build --format docker -t proomptz:latest .
```

#### Port Already in Use

**Error**: Port binding fails even though `podman ps` shows no containers

**Solution**: Check for lingering processes or use a different port:

```bash
# Find what's using the port
sudo ss -tulpn | grep 5026

# Or use a different port
podman run -p 8080:5026 proomptz:latest
```

## Advanced Topics

### Multi-Stage Build Optimization

The Dockerfile is already optimized with:
- Layer caching for dependencies
- Multi-stage builds to minimize image size
- Alpine Linux base for smaller footprint
- Non-root user for security

### Security Best Practices

1. **Run as non-root user**: Already configured in Dockerfile
2. **Use read-only filesystem**:

```bash
docker run -d \
  --read-only \
  --tmpfs /tmp \
  -v $(pwd)/data:/app/data \
  proomptz:latest
```

3. **Scan for vulnerabilities**:

```bash
docker scan proomptz:latest
```

4. **Use secrets for sensitive data**:

```bash
echo "secret-connection-string" | docker secret create db_connection -
```

### Monitoring and Logging

#### Export Logs to File

```bash
docker-compose logs -f > app.log
```

#### Integrate with Logging Stack

```yaml
logging:
  driver: "fluentd"
  options:
    fluentd-address: localhost:24224
    tag: proomptz
```

### CI/CD Integration

#### GitHub Actions Example

```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker image
        run: docker build -t proomptz:${{ github.sha }} .

      - name: Push to registry
        run: |
          echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
          docker push proomptz:${{ github.sha }}
```

### Container Orchestration

#### Docker Swarm

```bash
docker stack deploy -c docker-compose.prod.yml proomptz
```

#### Kubernetes

Example deployment manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: proomptz
spec:
  replicas: 3
  selector:
    matchLabels:
      app: proomptz
  template:
    metadata:
      labels:
        app: proomptz
    spec:
      containers:
      - name: proomptz
        image: proomptz:latest
        ports:
        - containerPort: 5026
        volumeMounts:
        - name: data
          mountPath: /app/data
        livenessProbe:
          httpGet:
            path: /health
            port: 5026
          initialDelaySeconds: 10
          periodSeconds: 30
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: proomptz-data
```

## Podman-Specific Features

### Running as Systemd Service

Podman supports running containers as systemd services, which is great for production:

```bash
# Generate systemd unit file
podman generate systemd --name proomptz --files --new

# Move to systemd directory
mkdir -p ~/.config/systemd/user/
mv container-proomptz.service ~/.config/systemd/user/

# Enable and start service
systemctl --user enable container-proomptz.service
systemctl --user start container-proomptz.service

# Enable linger (keeps service running after logout)
loginctl enable-linger $USER
```

Check status:

```bash
systemctl --user status container-proomptz.service
```

### Podman Pods

You can group containers into a pod (similar to Kubernetes pods):

```bash
# Create a pod
podman pod create --name proomptz-pod -p 5026:5026

# Run container in the pod
podman run -d \
  --pod proomptz-pod \
  --name proomptz \
  -v $(pwd)/data:/app/data:z \
  proomptz:latest

# Manage the entire pod
podman pod stop proomptz-pod
podman pod start proomptz-pod
podman pod rm proomptz-pod
```

### Auto-Update Containers

Podman supports automatic container updates:

```bash
# Run with auto-update label
podman run -d \
  --name proomptz \
  --label io.containers.autoupdate=registry \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data:z \
  proomptz:latest

# Enable auto-update systemd timer
systemctl --user enable --now podman-auto-update.timer
```

### Rootless Port Binding

By default, rootless Podman cannot bind to ports < 1024. To enable:

```bash
# Allow binding to privileged ports
sudo sysctl net.ipv4.ip_unprivileged_port_start=80

# Make persistent
echo 'net.ipv4.ip_unprivileged_port_start=80' | sudo tee /etc/sysctl.d/99-unprivileged-ports.conf
```

### Docker Compatibility Alias

For seamless Docker → Podman migration, create an alias:

```bash
echo "alias docker='podman'" >> ~/.bashrc
source ~/.bashrc
```

Now all `docker` commands will use Podman.

## Support

For issues or questions:
- Check the [main README](README.md)
- Review application logs
- Open an issue on GitHub

## License

[Your License Here]
