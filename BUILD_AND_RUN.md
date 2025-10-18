# Build and Run Instructions - Prompt Template Manager MVP

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm

## Backend Setup

```bash
cd backend

# Build the solution
dotnet build

# Run the API (will create SQLite database automatically)
dotnet run --project src/PromptTemplateManager.Api

# API will be available at: http://localhost:5026
# Swagger UI at: http://localhost:5026/swagger
```

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

1. Start backend: `cd backend && dotnet run --project src/PromptTemplateManager.Api`
2. Start frontend: `cd frontend && npm run dev`
3. Open browser to http://localhost:5173
4. Use Swagger at http://localhost:5000/swagger to test API directly

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
