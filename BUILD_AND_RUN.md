# Build and Run Instructions - Prompt Template Manager MVP

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm

## Environment Setup

Before running the application, copy the environment example files:

```bash
# Copy root .env.example (for Docker configuration)
cp .env.example .env

# Copy frontend .env.example (for development API configuration)
cp frontend/.env.example frontend/.env
```

These files configure the API port to **5026** for both local development and Docker deployment.

## Backend Setup

```bash
cd backend

# Build the solution
dotnet build

# Option 1: Run using the helper script (recommended)
./dev-run.sh

# Option 2: Run on a custom port
./dev-run.sh 8080

# Option 3: Run manually with environment variable
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/PromptTemplateManager.Api

# API will be available at: http://localhost:5026
# Swagger UI at: http://localhost:5026/swagger
```

**Note:** The `ASPNETCORE_ENVIRONMENT=Development` environment variable is required to:
- Use the correct configuration from `appsettings.Development.json`
- Enable Swagger UI
- Use development-specific settings (like local data paths)

The `dev-run.sh` script automatically sets this for you.

## Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Install additional packages for MVP
npm install @tanstack/react-query axios react-router-dom

# Install shadcn/ui components (optional - can be done incrementally)
npx shadcn-ui@latest init

# Run development server
npm run dev

# Frontend will be available at: http://localhost:5173
```

## Quick Test

1. Start backend: `cd backend && ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/PromptTemplateManager.Api`
2. Start frontend: `cd frontend && npm run dev`
3. Open browser to http://localhost:5173
4. Use Swagger at http://localhost:5026/swagger to test API directly

## MVP Features Available

✅ **Template CRUD**:
- Create template (POST /api/templates)
- List templates (GET /api/templates)
- Get single template (GET /api/templates/{id})
- Update template (PUT /api/templates/{id})
- Delete template (DELETE /api/templates/{id})

✅ **Database**: SQLite (`prompttemplates.db` created automatically)
✅ **CORS**: Configured for localhost:5173
✅ **Error Handling**: Global middleware with proper HTTP status codes
✅ **Validation**: FluentValidation on all requests

## Database Location

The SQLite database file `prompttemplates.db` will be created in:
`backend/src/PromptTemplateManager.Api/`

## Troubleshooting

**Port conflicts**:
- Backend: Change in `Properties/launchSettings.json`
- Frontend: Change in `vite.config.ts`

**CORS errors**: Verify frontend URL matches the CORS policy in `Program.cs`

**Database issues**: Delete `prompttemplates.db` and restart backend to recreate

## Next Steps (Post-MVP)

- User Story 2: Folder organization
- User Story 3: Placeholder substitution & Devin integration
- User Story 4: Full-text search
- Frontend components and pages
- OpenAPI client generation for frontend
