# Application Architecture

## Overview

Prompt Template Manager is a full-stack web application with a .NET backend and React frontend. It supports two deployment modes: **development** (separate servers) and **production** (unified container).

## Architecture Modes

### Development Mode (Separate Servers)

In development, the frontend and backend run as separate processes:

```
┌─────────────────┐         ┌─────────────────┐
│  Vite Dev Server│  CORS   │  ASP.NET Core   │
│  Port 5173      │────────>│  API Server     │
│  (Frontend)     │         │  Port 5026      │
└─────────────────┘         └─────────────────┘
     React App                   REST API
```

**Frontend**:
- Runs on `http://localhost:5173` (Vite dev server)
- Hot module replacement (HMR)
- API client configured to call `http://localhost:5026`

**Backend**:
- Runs on `http://localhost:5026`
- CORS enabled for `http://localhost:5173`
- Serves API endpoints at `/api/*`
- Swagger UI at `/swagger`

**How to run**:
```bash
# Terminal 1: Backend
cd backend/src/PromptTemplateManager.Api
dotnet run

# Terminal 2: Frontend
cd frontend
npm run dev
```

### Production Mode (Unified Container)

In production (Docker/Podman), the backend serves both the API and the frontend:

```
┌────────────────────────────────────────┐
│         ASP.NET Core Server            │
│            Port 5026                   │
│                                        │
│  ┌──────────────┐  ┌────────────────┐ │
│  │ Static Files │  │   REST API     │ │
│  │  (wwwroot)   │  │   (/api/*)     │ │
│  │              │  │                │ │
│  │ index.html   │  │ Controllers    │ │
│  │ assets/      │  │ /health        │ │
│  │ (React SPA)  │  │ /swagger       │ │
│  └──────────────┘  └────────────────┘ │
└────────────────────────────────────────┘
          Single Container
```

**How it works**:
1. Dockerfile builds frontend → outputs to `dist/`
2. Frontend `dist/` copied to backend `wwwroot/`
3. ASP.NET Core serves:
   - Static files from `wwwroot/` (React app)
   - API endpoints from controllers
   - Fallback to `index.html` for client-side routing

**Frontend routing**:
- API base URL: `window.location.origin` (same origin)
- React Router handles `/templates`, `/folders`, etc.
- `MapFallbackToFile("index.html")` ensures SPA routing works

**How to run**:
```bash
# Using Podman
podman compose up -d

# Or manually
podman build -t proomptz .
podman run -d -p 5026:5026 -v $(pwd)/data:/app/data:Z proomptz
```

Access at: `http://localhost:5026`

## Technology Stack

### Backend
- **.NET 9.0** - Runtime framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **SQLite** - Database with FTS5 full-text search
- **Swagger/OpenAPI** - API documentation

### Frontend
- **React 19** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TanStack Query** - Server state management
- **React Router** - Client-side routing
- **Tailwind CSS** - Styling
- **shadcn/ui** - Component library
- **openapi-typescript-codegen** - Auto-generated API client

### Infrastructure
- **Docker/Podman** - Containerization
- **Multi-stage builds** - Optimized images
- **Alpine Linux** - Base image (minimal footprint)

## Project Structure

```
proomptz/
├── backend/
│   └── src/
│       ├── PromptTemplateManager.Api/          # ASP.NET Core API
│       │   ├── Controllers/                    # REST endpoints
│       │   ├── Middleware/                     # Request pipeline
│       │   ├── Program.cs                      # App configuration
│       │   └── wwwroot/                        # Static files (production only)
│       ├── PromptTemplateManager.Application/  # Business logic
│       │   ├── Services/                       # Application services
│       │   └── Validators/                     # FluentValidation rules
│       ├── PromptTemplateManager.Core/         # Domain entities
│       │   ├── Entities/                       # Domain models
│       │   └── Interfaces/                     # Service contracts
│       └── PromptTemplateManager.Infrastructure/ # External dependencies
│           ├── Data/                           # EF Core DbContext
│           ├── Repositories/                   # Data access
│           └── DevinIntegration/               # External API client
├── frontend/
│   └── src/
│       ├── components/                         # React components
│       ├── pages/                              # Page components
│       ├── lib/                                # Utilities & API client
│       │   ├── api/                            # Auto-generated client
│       │   └── api-client.ts                   # API wrapper
│       └── App.tsx                             # Main app with routing
├── shared/
│   └── openapi/                                # OpenAPI specification
│       └── swagger.json                        # Generated from backend
├── Dockerfile                                  # Multi-stage container build
├── docker-compose.yml                          # Container orchestration
└── data/                                       # SQLite database (volume mount)
    └── prompttemplates.db
```

## Data Flow

### Creating a Template

**Development**:
```
User Input → React Component → API Client (fetch to :5026)
  → ASP.NET Controller → Service Layer → Repository
  → EF Core → SQLite → Response back through layers
```

**Production (Container)**:
```
User Input → React Component → API Client (fetch to same origin)
  → ASP.NET Controller (same server) → Service Layer → Repository
  → EF Core → SQLite → Response back through layers
```

### Searching Templates

```
Search Input → API Client → /api/Search endpoint
  → SearchService → FTS5 Query (SQLite)
  → Results ranked by relevance → React UI
```

## API Configuration

### Backend CORS (Program.cs:32-40)

Development mode requires CORS to allow frontend on `:5173` to call API on `:5026`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

In production, CORS is not needed (same origin).

### Frontend API Client (api-client.ts:19-21)

```typescript
OpenAPI.BASE = import.meta.env.DEV
  ? 'http://localhost:5026'  // Dev: Separate backend
  : window.location.origin;   // Prod: Same origin
```

## Static File Serving (Production)

### Program.cs Configuration

```csharp
// Serve static files from wwwroot
app.UseStaticFiles();

// ... API routes ...

// SPA fallback - serve index.html for non-API routes
app.MapFallbackToFile("index.html");
```

**How fallback works**:
1. Request comes in (e.g., `/templates/123`)
2. ASP.NET checks for matching controller route → not found
3. Checks for static file at `/templates/123` → not found
4. Falls back to `wwwroot/index.html`
5. React app loads, React Router handles `/templates/123`

## Database

### Schema

- **Templates** - Markdown templates with placeholders
- **Folders** - Hierarchical organization
- **TemplatesFts** - Full-text search virtual table (FTS5)

### Migrations

Migrations are applied automatically on startup (Program.cs:82-84):

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

### Location

- **Development**: `prompttemplates.db` in API project directory
- **Production**: `/app/data/prompttemplates.db` (volume mounted)

## Build Process

### Development

Frontend:
```bash
npm run dev          # Starts Vite dev server
npm run build        # Builds for production → dist/
npm run generate:api # Regenerates API client from swagger.json
```

Backend:
```bash
dotnet run           # Runs API server
dotnet build         # Compiles backend
```

### Production (Dockerfile)

**Stage 1: Build Frontend**
```dockerfile
FROM node:20-alpine
COPY frontend/ .
COPY shared/ .
RUN npm ci
RUN npm run build    # Outputs to dist/
```

**Stage 2: Build Backend**
```dockerfile
FROM dotnet/sdk:9.0-alpine
COPY backend/ .
RUN dotnet restore
RUN dotnet publish   # Outputs to /app/publish
```

**Stage 3: Runtime**
```dockerfile
FROM dotnet/aspnet:9.0-alpine
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /frontend/dist ./wwwroot
CMD ["dotnet", "PromptTemplateManager.Api.dll"]
```

## Security

- **Non-root user**: Container runs as UID 1000 (appuser)
- **Rootless containers**: Podman supports running without privileged access
- **SELinux**: Volume labeling with `:Z` flag for Fedora/RHEL
- **Input validation**: FluentValidation on all API inputs
- **SQL injection**: Protected by EF Core parameterization
- **CORS**: Restricted to development origin

## Performance

- **Response compression**: Gzip/Brotli enabled
- **Static file caching**: Browser caching headers
- **Connection pooling**: EF Core DbContext pooling
- **FTS5 indexing**: Optimized full-text search
- **Image size**: ~197 MB (Alpine-based)
- **Cold start**: ~2-3 seconds (container startup)

## Monitoring

### Health Checks

- **Endpoint**: `GET /health`
- **Container health**: `HEALTHCHECK` in Dockerfile (Docker format)
- **Response**: `Healthy` (200 OK)

### Logging

- **Development**: Console output
- **Production**: JSON structured logs
- **Request logging**: Custom middleware (RequestLoggingMiddleware)
- **Error handling**: Custom middleware (ErrorHandlingMiddleware)

## Development Workflow

1. Make backend changes → Update controllers/services
2. Run backend → OpenAPI spec regenerates at `/swagger/v1/swagger.json`
3. Copy spec to `shared/openapi/swagger.json`
4. Run `npm run generate:api` → TypeScript client updates
5. Use typed API in React components

## Deployment Options

### Option 1: Container (Recommended)
- Single image with frontend + backend
- Easy scaling and orchestration
- Consistent across environments

### Option 2: Separate Deployment
- Frontend: Deploy to CDN/static hosting
- Backend: Deploy as API server
- Requires CORS configuration
- Environment variable for API URL

### Option 3: Systemd Service (Podman)
- Run as user service
- Automatic restarts
- Integration with system logging
- No Docker daemon required

## Common Issues

### "API not found" in production
- **Cause**: Frontend not built or not copied to wwwroot
- **Fix**: Rebuild Docker image

### "CORS error" in development
- **Cause**: Backend CORS policy doesn't include frontend origin
- **Fix**: Check Program.cs CORS configuration (line 36)

### "404 on page refresh" in production
- **Cause**: Missing SPA fallback
- **Fix**: Ensure `app.MapFallbackToFile("index.html")` is present

### "Permission denied" database error
- **Cause**: Container user can't write to volume
- **Fix**: `chmod 777 data` or `podman unshare chown 1000:1000 data`

## Further Reading

- [Development README](README.md)
- [Container Documentation](DOCKER.md)
- [Podman Quick Start](PODMAN-QUICKSTART.md)
