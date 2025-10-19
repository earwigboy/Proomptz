# Prompt Template Manager

A web application for creating, organizing, and using markdown-based prompt templates with placeholder substitution for AI workflows.

## Features

- **Template Management**: Create, edit, and delete prompt templates with markdown support
- **Folder Organization**: Hierarchical folder structure with drag-and-drop organization
- **Placeholder Substitution**: Dynamic placeholders (`{{placeholder_name}}`) with live preview
- **Full-Text Search**: Fast SQLite FTS5-powered search across template names and content
- **Devin Integration**: Send completed prompts directly to Devin LLM

## Tech Stack

- **Backend**: .NET 9.0, ASP.NET Core, Entity Framework Core, SQLite
- **Frontend**: React 19, TypeScript, Vite, TanStack Query, React Router
- **API**: OpenAPI/Swagger with auto-generated TypeScript client

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+ and npm
- Git

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd proomptz
```

2. Start the backend:
```bash
cd backend/src/PromptTemplateManager.Api
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5026`
Swagger UI: `http://localhost:5026/swagger`

3. Start the frontend (in a new terminal):
```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`

### First Run

The application will automatically:
- Create the SQLite database (`prompttemplates.db`)
- Apply all database migrations
- Set up FTS5 full-text search indexes

## Project Structure

```
proomptz/
├── backend/
│   └── src/
│       ├── PromptTemplateManager.Api/          # ASP.NET Core API
│       ├── PromptTemplateManager.Application/  # Business logic
│       ├── PromptTemplateManager.Core/         # Domain entities & interfaces
│       └── PromptTemplateManager.Infrastructure/ # Data access & external services
├── frontend/
│   └── src/
│       ├── components/      # React components
│       ├── pages/          # Page components
│       ├── lib/            # Utilities, hooks, API client
│       └── App.tsx         # Main app with routing
├── shared/
│   └── openapi/           # Generated OpenAPI spec
└── specs/                 # Feature specifications
```

## Development

### API Client Regeneration

The TypeScript API client is auto-generated from the OpenAPI spec:

```bash
cd frontend
npm run generate:api
```

This runs automatically before builds (`npm run build`).

### Database Migrations

Create a new migration:
```bash
cd backend/src/PromptTemplateManager.Api
dotnet ef migrations add MigrationName --project ../PromptTemplateManager.Infrastructure
```

Migrations are applied automatically on application startup.

### Building for Production

Backend:
```bash
cd backend/src/PromptTemplateManager.Api
dotnet build -c Release
```

Frontend:
```bash
cd frontend
npm run build
```

## API Endpoints

- `GET /api/Templates` - List templates (with pagination and folder filtering)
- `POST /api/Templates` - Create template
- `GET /api/Templates/{id}` - Get template by ID
- `PUT /api/Templates/{id}` - Update template
- `DELETE /api/Templates/{id}` - Delete template
- `GET /api/Templates/{id}/placeholders` - Get template placeholders
- `POST /api/Templates/{id}/send` - Send prompt to Devin
- `GET /api/Folders/tree` - Get folder tree
- `GET /api/Search?q={query}` - Search templates
- `GET /health` - Health check endpoint

## Environment Configuration

Backend (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=prompttemplates.db"
  }
}
```

Frontend (`src/lib/api-client.ts`):
```typescript
OpenAPI.BASE = 'http://localhost:5026';
```

## License

[Your License Here]

## Contributing

[Contributing guidelines if applicable]
