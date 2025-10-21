# Podman Quick Start Guide

Quick reference for running Prompt Template Manager with Podman on Fedora.

## Prerequisites

Podman is pre-installed on Fedora. Verify:

```bash
podman --version
podman compose version
```

## Quick Start (3 Steps)

### 1. Prepare Data Directory

```bash
mkdir -p data
chmod 777 data  # Allow container's non-root user to write
```

### 2. Build the Image

```bash
podman build -t proomptz:latest .
```

Or use the helper script:

```bash
./docker-build.sh
```

### 3. Run the Container

**Option A: Using Compose (Recommended)**

```bash
# Create .env file for SELinux flag
echo "SELINUX_FLAG=:Z" > .env

# Start the application
podman compose up -d
```

**Option B: Using Helper Script**

```bash
./docker-run.sh
```

**Option C: Direct Podman Run**

```bash
podman run -d \
  --name proomptz \
  -p 5026:5026 \
  -v $(pwd)/data:/app/data:Z \
  proomptz:latest
```

## Access the Application

- **Frontend**: http://localhost:5026
- **Health Check**: http://localhost:5026/health
- **API Docs**: http://localhost:5026/swagger

## Common Commands

```bash
# View logs
podman logs -f proomptz

# Stop container
podman stop proomptz

# Start container
podman start proomptz

# Remove container
podman rm proomptz

# Remove image
podman rmi proomptz:latest

# View running containers
podman ps

# View all containers
podman ps -a

# Execute command in container
podman exec -it proomptz sh
```

## Compose Commands

```bash
# Start services
podman compose up -d

# View logs
podman compose logs -f

# Stop services
podman compose down

# Rebuild and start
podman compose up --build -d
```

## Troubleshooting

### Database Permission Error

If you see `SQLite Error 14: 'unable to open database file'`:

```bash
# Fix permissions
chmod 777 data

# Or use podman unshare (preferred)
podman unshare chown 1000:1000 data
```

### SELinux Issues

Always use `:Z` flag for volume mounts on Fedora:

```bash
podman run -v $(pwd)/data:/app/data:Z proomptz:latest
```

For compose, set in `.env`:

```bash
echo "SELINUX_FLAG=:Z" > .env
```

### Port Already in Use

Change the port mapping:

```bash
podman run -p 8080:5026 proomptz:latest
```

Access at http://localhost:8080

## Running as Systemd Service

Generate and enable systemd service:

```bash
# Run container first
podman run -d --name proomptz -p 5026:5026 -v $(pwd)/data:/app/data:Z proomptz:latest

# Generate systemd unit
podman generate systemd --name proomptz --files --new

# Install service
mkdir -p ~/.config/systemd/user/
mv container-proomptz.service ~/.config/systemd/user/

# Enable and start
systemctl --user enable container-proomptz.service
systemctl --user start container-proomptz.service

# Enable linger (survives logout)
loginctl enable-linger $USER

# Check status
systemctl --user status container-proomptz.service
```

## Backup and Restore

### Backup Database

```bash
# Copy from running container
podman cp proomptz:/app/data/prompttemplates.db ./backup/

# Or backup entire data directory
tar czf backup-$(date +%Y%m%d).tar.gz data/
```

### Restore Database

```bash
# Extract backup
tar xzf backup-YYYYMMDD.tar.gz

# Or copy to running container
podman cp ./backup/prompttemplates.db proomptz:/app/data/
podman restart proomptz
```

## Advanced: Rootless with Systemd

For production rootless setup:

```bash
# 1. Create dedicated user (optional)
sudo useradd -m proomptz-user

# 2. Enable lingering
sudo loginctl enable-linger proomptz-user

# 3. As proomptz-user, set up service
su - proomptz-user
cd /home/proomptz-user
git clone <repo>
cd Proomptz
mkdir -p data
chmod 777 data
podman build -t proomptz:latest .
podman run -d --name proomptz -p 5026:5026 -v $(pwd)/data:/app/data:Z proomptz:latest
podman generate systemd --name proomptz --files --new
mkdir -p ~/.config/systemd/user/
mv container-proomptz.service ~/.config/systemd/user/
systemctl --user enable container-proomptz.service
systemctl --user start container-proomptz.service
```

## Firewall Configuration

If you need external access:

```bash
# Allow port 5026
sudo firewall-cmd --add-port=5026/tcp --permanent
sudo firewall-cmd --reload

# Or allow http (port 80) and redirect
sudo firewall-cmd --add-service=http --permanent
sudo firewall-cmd --reload
```

## Upgrading

```bash
# Pull latest code
git pull

# Rebuild image
podman build -t proomptz:latest .

# Stop and remove old container (keeps data)
podman stop proomptz
podman rm proomptz

# Start new container
podman run -d --name proomptz -p 5026:5026 -v $(pwd)/data:/app/data:Z proomptz:latest
```

Or with compose:

```bash
git pull
podman compose down
podman compose up --build -d
```

## Additional Resources

- [Full Container Documentation](DOCKER.md)
- [Main README](README.md)
- [Podman Documentation](https://docs.podman.io/)
- [Podman Desktop](https://podman-desktop.io/)
