# Research & Technical Decisions: Prompt Template Manager

**Feature**: 001-prompt-template-manager
**Date**: 2025-10-18
**Phase**: Phase 0 - Technology Research

## Overview

This document captures the technology decisions and research findings for implementing the Prompt Template Manager application. All decisions align with the user-specified tech stack (C#/.NET backend, Vite/TypeScript/shadcn frontend, SQLite storage) and project constitution requirements.

## Technology Stack Decisions

### Backend Stack

**Decision**: C# 12 / .NET 8.0 with ASP.NET Core

**Rationale**:
- **Type Safety**: C# strong typing aligns with Constitution I (Code Quality Standards)
- **Performance**: .NET 8 runtime optimizations achieve <200ms API response targets
- **Ecosystem**: Rich ecosystem for web APIs, ORM, testing, OpenAPI generation
- **Cross-platform**: Runs on Windows, macOS, Linux

**Alternatives Considered**:
- Python/FastAPI: Rejected - user specified C#/.NET
- Node.js/Express: Rejected - user specified C#/.NET

### Frontend Stack

**Decision**: Vite 5.x + React 18 + TypeScript 5.x + shadcn/ui + Tailwind CSS

**Rationale**:
- **Build Performance**: Vite provides fastest dev server and build times
- **Type Safety**: TypeScript ensures type-safe props, state, API calls
- **Component Library**: shadcn/ui provides accessible, customizable components (WCAG 2.1 AA)
- **Styling**: Tailwind CSS utility-first approach enables rapid UI development
- **Bundle Size**: Vite tree-shaking + code splitting easily achieves <200KB target

**Alternatives Considered**:
- Angular: Rejected - user specified Vite + React ecosystem
- Vue.js: Rejected - user specified React
- Next.js: Rejected - Vite specified; SSR unnecessary for local single-user app

### Database & Storage

**Decision**: SQLite with Entity Framework Core

**Rationale**:
- **Simplicity**: File-based, zero-config database perfect for single-user local app
- **Performance**: Sufficient for 1000+ templates with proper indexing
- **Portability**: Single `.db` file is easy to backup/restore
- **EF Core Support**: First-class ORM support, migrations, LINQ queries

**Alternatives Considered**:
- PostgreSQL: Rejected - overkill for single-user; requires separate server process
- JSON files: Rejected - no ACID guarantees, poor query performance at scale
- LiteDB (C# native): Rejected - SQLite has better tooling, ecosystem, EF Core integration

### API Type Generation

**Decision**: Swashbuckle (backend) + openapi-typescript-codegen (frontend)

**Rationale**:
- **Single Source of Truth**: OpenAPI spec generated from C# controllers
- **Build-Time Safety**: Frontend gets compile-time type errors if API changes
- **Zero Manual Work**: Types auto-update on backend changes
- **Standards Compliance**: OpenAPI 3.0 is industry standard

**Alternatives Considered**:
- NSwag: Rejected - Swashbuckle more actively maintained, better .NET 8 support
- Manual TypeScript types: Rejected - violates DRY, error-prone, no contract validation
- tRPC: Rejected - requires both sides to be TypeScript

## Architecture Patterns

### Backend Architecture

**Decision**: Clean Architecture (Domain-Driven Design layers)

**Layers**:
1. **PromptTemplateManager.Core**: Domain entities, interfaces (no dependencies)
2. **PromptTemplateManager.Application**: Business logic services, DTOs, validators
3. **PromptTemplateManager.Infrastructure**: EF Core, repositories, external integrations
4. **PromptTemplateManager.Api**: Controllers, middleware, DI configuration

**Rationale**:
- **Testability**: Core business logic independent of infrastructure
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Can swap SQLite for another DB without changing business logic
- **Constitution Alignment**: Enforces Single Responsibility Principle (I)

**Alternatives Considered**:
- Monolithic MVC: Rejected - harder to test, violates separation of concerns
- Microservices: Rejected - overkill for single-user local app
- Vertical Slice: Rejected - less structure for team collaboration

### Frontend Architecture

**Decision**: Feature-based component organization with custom hooks

**Structure**:
- `/components/ui/`: Base shadcn components
- `/components/templates/`, `/components/folders/`, `/components/placeholders/`: Feature-specific
- `/pages/`: Route-level views
- `/lib/hooks/`: Custom React hooks for state management (useTemplates, useFolders)
- `/lib/api/`: Auto-generated API client

**Rationale**:
- **Discoverability**: Features grouped together, easy to locate code
- **Reusability**: Shared hooks prevent duplication
- **Type Safety**: Generated API client ensures type-safe backend calls

**Alternatives Considered**:
- Atomic Design: Rejected - too rigid for small application
- Redux: Rejected - overkill for local state; React Query/SWR better for server state
- Zustand: Rejected - most state is server-driven; custom hooks sufficient

## Key Technical Decisions

### Placeholder Parsing

**Decision**: Regex-based parser with `{{variable_name}}` syntax

**Implementation**:
```
Pattern: /\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}/g
Validation: Placeholder names must start with letter/underscore, contain alphanumerics/underscores
```

**Rationale**:
- Simple, predictable syntax
- No conflict with markdown syntax (markdown uses `{` for other purposes rarely)
- Easy regex parsing in both C# and TypeScript

**Alternatives Considered**:
- Handlebars syntax (`{{name}}`): Same syntax chosen
- Mustache (`{{{name}}}`): Rejected - triple braces more typing, less common
- `$variable`: Rejected - conflicts with bash/shell syntax in code templates

### Markdown Storage

**Decision**: Store markdown as plain text in SQLite TEXT column

**Rationale**:
- Preserves formatting perfectly (no parsing/serialization)
- Full-text search supported via SQLite FTS5 extension
- No size limit concerns (SQLite TEXT supports up to 1GB)

**Alternatives Considered**:
- Parse to AST and store structured: Rejected - lossy, complex, unnecessary
- Store as file + path in DB: Rejected - two sources of truth, harder backups

### Search Implementation

**Decision**: SQLite FTS5 (Full-Text Search) extension

**Rationale**:
- Built into SQLite, no extra dependencies
- Sub-second search on 500+ templates
- Supports relevance ranking, prefix matching
- Can index both template name and content

**Alternatives Considered**:
- LIKE queries: Rejected - too slow for content search at scale
- Client-side search: Rejected - requires loading all templates into memory
- Elasticsearch: Rejected - massive overkill for local app

### Error Handling

**Decision**: Global exception middleware (backend) + React Error Boundaries (frontend)

**Backend**:
- Custom middleware catches all exceptions
- Maps domain exceptions to appropriate HTTP status codes
- Returns consistent JSON error format: `{ "error": "message", "details": [...] }`
- Logs stack traces server-side, never exposes to client

**Frontend**:
- Error boundaries catch React component errors
- Toast notifications for API errors
- User-friendly messages (no technical jargon)

**Rationale**: Aligns with Constitution III (User Experience Consistency - clear error messages)

## Testing Strategy

### Backend Testing

**Decision**: xUnit + FluentAssertions + Moq + Microsoft.AspNetCore.Mvc.Testing

**Test Types**:
1. **Unit Tests**: Services, validators, domain logic (80%+ coverage)
2. **Integration Tests**: API endpoints with in-memory SQLite (WebApplicationFactory)
3. **Contract Tests**: OpenAPI spec validation

**Rationale**:
- xUnit is .NET standard, fast, well-supported
- In-memory SQLite enables realistic integration tests without external DB
- WebApplicationFactory tests full HTTP pipeline

### Frontend Testing

**Decision**: Vitest + Testing Library + MSW (Mock Service Worker)

**Test Types**:
1. **Unit Tests**: Components, hooks, utilities
2. **Integration Tests**: User flows with mocked API (MSW)

**Rationale**:
- Vitest is Vite-native, fast, ESM-first
- Testing Library encourages accessibility-friendly tests
- MSW intercepts network requests for realistic API mocking

**Alternatives Considered**:
- Jest: Rejected - Vitest faster, better Vite integration
- Cypress: Rejected - heavier, slower; Vitest + Testing Library sufficient
- Playwright: Considered for E2E but deferred (optional enhancement)

## Build & Development Workflow

### Backend Build Process

**Steps**:
1. Restore NuGet packages
2. Build solution (all projects)
3. Run tests (xUnit)
4. Generate OpenAPI spec (Swashbuckle) → `shared/openapi/swagger.json`

**Commands**:
```bash
dotnet restore
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet run --project src/PromptTemplateManager.Api  # Starts API, generates swagger.json
```

### Frontend Build Process

**Steps**:
1. Install npm dependencies
2. Generate TypeScript client from `shared/openapi/swagger.json` (openapi-typescript-codegen)
3. Run type checking (tsc)
4. Run tests (Vitest)
5. Build production bundle (Vite)

**Commands**:
```bash
npm install
npm run generate:api  # openapi-typescript-codegen -i ../shared/openapi/swagger.json
npm run type-check    # tsc --noEmit
npm run test
npm run build         # Vite build
```

**Integration**: Backend must build first to generate OpenAPI spec; frontend consumes it.

### Development Experience

**Decision**: Concurrent dev servers with hot reload

**Setup**:
- Backend: `dotnet watch run` (auto-restart on C# changes)
- Frontend: `npm run dev` (Vite HMR on TypeScript/React changes)
- Backend runs on `http://localhost:5000`
- Frontend runs on `http://localhost:5173` with CORS configured

## Devin LLM Integration

**Decision**: Abstraction layer for future implementation

**Approach**:
- Define `IDevinClient` interface in Core layer
- Stub implementation in Infrastructure (returns success immediately)
- UI flow fully functional with stub
- Real implementation added later when Devin API details available

**Rationale**:
- Unblocks development (Devin integration details unknown)
- Testable without external dependency
- Easy to swap implementation when ready

## Security Considerations

### Input Validation

- **Backend**: FluentValidation for DTOs (template name, content, folder names)
- **Frontend**: React Hook Form with Zod schemas
- **SQL Injection**: EF Core parameterized queries prevent injection
- **XSS**: React escapes by default; markdown rendered via sanitized library (react-markdown + rehype-sanitize)

### Local-Only Security Model

- No authentication/authorization needed (single-user local app)
- No network exposure beyond localhost
- HTTPS not required (localhost-only)

## Performance Optimizations

### Database

- Indexes on: `Templates.FolderId`, `Folders.ParentId`, `Templates.Name`
- FTS5 index on: `Templates.Name`, `Templates.Content`
- Pagination for list endpoints (default 50, max 200)

### Frontend

- Code splitting: Lazy-load route components (`React.lazy`)
- Bundle optimization: Vite tree-shaking, minification, gzip
- Virtual scrolling: For large template lists (react-window)
- Debounced search: 300ms delay on search input

### API

- Response caching: In-memory cache for folder tree (rarely changes)
- Compression: ASP.NET Core response compression middleware
- Minimal DTOs: Only send required fields to frontend

## Open Questions & Future Enhancements

### Deferred to Implementation

1. **Devin API Integration**: Exact protocol/endpoint unknown; stub for now
2. **Export/Import**: Template library backup/restore (future enhancement)
3. **Template Versioning**: Track template change history (out of MVP scope)
4. **Collaborative Editing**: Multi-user support (explicitly out of scope)

### Technical Debt Acceptance

None identified - all decisions align with constitution and requirements.

## Summary

All technical decisions support the feature requirements while adhering to the project constitution. The stack (C#/.NET + Vite/React + SQLite) provides strong typing, performance, and developer experience. Clean architecture in backend and feature-based organization in frontend ensure maintainability. OpenAPI-driven type generation eliminates manual sync work and provides compile-time safety.

**Ready to proceed to Phase 1**: Data model design and API contract definition.
