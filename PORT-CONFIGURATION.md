# Port Configuration Guide

The backend API port is now fully configurable across all deployment modes.

## Default Port

The default port is **5026** for both development and production.

## Configuration Methods

### 1. Local Development (Backend)

The port is configured in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5026"
      }
    }
  }
}
```

To override, set the `PORT` environment variable:

```bash
cd backend
ASPNETCORE_ENVIRONMENT=Development PORT=8080 dotnet run --project src/PromptTemplateManager.Api
```

**Important:** Always set `ASPNETCORE_ENVIRONMENT=Development` when running locally to use `appsettings.Development.json`.

### 2. Local Development (Frontend)

Create a `.env` file in the `frontend` directory:

```bash
cd frontend
echo "VITE_API_PORT=5026" > .env
npm run dev
```

To use a different port:

```bash
echo "VITE_API_PORT=8080" > .env
```

### 3. Docker / Podman

**Using docker-compose:**

Create a `.env` file in the project root:

```bash
PORT=5026
```

Then run:

```bash
docker-compose up -d
# or
podman compose up -d
```

To use a different port:

```bash
PORT=8080 docker-compose up -d
```

**Using docker run:**

```bash
docker run -d -p 5026:5026 -e PORT=5026 -v $(pwd)/data:/app/data proomptz:latest

# Or on a different port:
docker run -d -p 8080:8080 -e PORT=8080 -v $(pwd)/data:/app/data proomptz:latest
```

**Using helper script:**

```bash
./docker-run.sh -p 8080
```

## Environment Variables

### Backend

- `PORT` - The port the backend listens on (default: 5026)
- Takes precedence over `ASPNETCORE_URLS` if both are set

### Frontend (Development Only)

- `VITE_API_PORT` - The port where the frontend should connect to the backend API (default: 5026)
- Only used in development mode (when running `npm run dev`)
- In production, the frontend is served by the backend and uses the same origin

## Examples

### Example 1: Run everything on port 8080

**Backend:**
```bash
cd backend
ASPNETCORE_ENVIRONMENT=Development PORT=8080 dotnet run --project src/PromptTemplateManager.Api
```

**Frontend:**
```bash
cd frontend
echo "VITE_API_PORT=8080" > .env
npm run dev
```

### Example 2: Run with Docker on port 3000

Create `.env`:
```bash
PORT=3000
```

Run:
```bash
docker-compose up -d
```

Access at http://localhost:3000

### Example 3: Use default port (5026)

Just run without any configuration:

```bash
# Backend (in Development mode)
cd backend && ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/PromptTemplateManager.Api

# Frontend
cd frontend && npm run dev

# Docker
docker-compose up -d
```

## Troubleshooting

### Port Already in Use

If you see an error about the port being in use:

1. Check what's using the port:
   ```bash
   sudo lsof -i :5026
   # or on Linux:
   sudo ss -tulpn | grep 5026
   ```

2. Either stop that process or use a different port:
   ```bash
   PORT=8080 docker-compose up -d
   ```

### Frontend Can't Connect to Backend

Make sure both are using the same port:

- Backend is configured with `PORT` environment variable
- Frontend `.env` has matching `VITE_API_PORT`

For example:
```bash
# Backend
ASPNETCORE_ENVIRONMENT=Development PORT=8080 dotnet run --project src/PromptTemplateManager.Api

# Frontend .env
VITE_API_PORT=8080
```

## Configuration Priority

The configuration is applied in this order (highest to lowest priority):

1. `PORT` environment variable
2. `ASPNETCORE_URLS` environment variable
3. `Kestrel:Endpoints:Http:Url` in appsettings files
4. ASP.NET Core default (5000)

For most use cases, simply set the `PORT` environment variable.
