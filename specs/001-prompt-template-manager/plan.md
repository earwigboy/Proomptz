# Implementation Plan: Prompt Template Manager

**Branch**: `001-prompt-template-manager` | **Date**: 2025-10-18 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-prompt-template-manager/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a web application for creating, organizing, and using markdown-based prompt templates with placeholder substitution for the Devin LLM. Core features include template CRUD operations (P1-MVP), hierarchical folder organization (P2), placeholder-driven template usage (P3), and search/discovery (P4). Application uses C#/.NET backend with SQLite storage, Vite/TypeScript/shadcn/Tailwind frontend, with automated TypeScript client generation from OpenAPI specifications.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 (backend), TypeScript 5.x (frontend)
**Primary Dependencies**:
- Backend: ASP.NET Core 8.0, Entity Framework Core, Swashbuckle (OpenAPI), SQLite
- Frontend: Vite 5.x, React 18, shadcn/ui, Tailwind CSS, openapi-typescript-codegen
**Storage**: SQLite (local file-based database)
**Testing**: xUnit (backend), Vitest (frontend)
**Target Platform**: Cross-platform desktop/web (runs locally, browser-based UI)
**Project Type**: Web application (separate backend/frontend)
**Performance Goals**:
- API response <200ms p95 for CRUD operations
- Search results <1s for 500 templates
- Support 1000+ templates without degradation
- Frontend bundle <200KB gzipped initial load
**Constraints**:
- Single-user local application (no multi-user concurrency)
- All data persists locally in SQLite
- API types auto-generated from OpenAPI spec (build-time)
- WCAG 2.1 AA accessibility compliance
**Scale/Scope**:
- Support 1000+ templates
- Folder hierarchy depth: 5+ levels
- Individual template size: up to 10,000 lines
- ~8-12 API endpoints
- ~6-8 primary UI screens/views

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Standards

✅ **PASS** - Alignment confirmed:
- C# and TypeScript both provide strong type systems (typed parameters, return values, DTOs)
- Linting configured: ESLint/Prettier (frontend), StyleCop/dotnet format (backend)
- Single Responsibility enforced through layered architecture (Controllers → Services → Repositories)
- Error handling: ASP.NET Core middleware + frontend error boundaries

### II. Testing Standards

✅ **PASS** - Alignment confirmed:
- Test pyramid supported: xUnit (backend unit/integration), Vitest (frontend unit/integration)
- Contract testing: OpenAPI spec serves as contract; can validate with tools like Schemathesis
- Coverage gates: 80% minimum enforced in CI
- Fast feedback: xUnit and Vitest both support <5s unit test execution
- Given/When/Then structure supported in both frameworks

### III. User Experience Consistency

✅ **PASS** - Alignment confirmed:
- Design system: shadcn/ui provides consistent component library built on Radix UI primitives
- Accessibility: shadcn/ui components are WCAG 2.1 AA compliant out-of-box
- Responsive design: Tailwind CSS utility-first approach supports 320px-4K
- Loading states: React Suspense + shadcn/ui loading components for async operations
- Error messages: Centralized error handling with user-friendly messages (no stack traces to UI)
- Consistent patterns: shadcn/ui enforces uniform form, modal, notification behaviors

### IV. Performance Requirements

✅ **PASS** - Alignment confirmed:
- API response time: ASP.NET Core + SQLite easily achieves <200ms p95 for CRUD
- Page load: Vite optimizes bundle splitting; target <200KB gzipped achievable
- Database queries: EF Core LINQ prevents N+1 with eager loading; pagination built-in
- Bundle size: Vite tree-shaking + code splitting; lazy route loading supported
- Memory footprint: .NET 8 optimized runtime <512MB for local single-user app
- Monitoring: ASP.NET Core built-in telemetry + browser performance APIs

### Development Workflow

✅ **PASS** - Alignment confirmed:
- Feature branches workflow supported
- CI validation: GitHub Actions / Azure Pipelines for .NET + Vite builds
- OpenAPI/Swagger: Swashbuckle auto-generates spec; updated before implementation
- Pull request model enforced through Git

### Quality Gates

✅ **PASS** - Alignment confirmed:
- Pre-merge gates: All tests pass, linting, type checking (tsc --noEmit), coverage gates
- Bundle size validation: Vite build reports; can fail CI if exceeds 200KB
- Security scan: npm audit (frontend), dotnet list package --vulnerable (backend)

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
backend/
├── src/
│   ├── PromptTemplateManager.Api/          # ASP.NET Core Web API project
│   │   ├── Controllers/                     # REST API endpoints
│   │   ├── Middleware/                      # Error handling, logging
│   │   ├── Program.cs                       # App entry point, DI configuration
│   │   └── appsettings.json                # Configuration
│   ├── PromptTemplateManager.Core/         # Domain models & interfaces
│   │   ├── Entities/                        # Template, Folder, PromptInstance
│   │   ├── Interfaces/                      # ITemplateRepository, IFolderService
│   │   └── Exceptions/                      # DomainException, ValidationException
│   ├── PromptTemplateManager.Application/  # Business logic services
│   │   ├── Services/                        # TemplateService, FolderService, PlaceholderService
│   │   ├── DTOs/                           # Request/Response models
│   │   └── Validators/                      # FluentValidation rules
│   └── PromptTemplateManager.Infrastructure/ # Data access & external integrations
│       ├── Data/                           # DbContext, migrations
│       ├── Repositories/                   # EF Core repository implementations
│       └── DevinIntegration/              # Devin LLM client (future)
└── tests/
    ├── PromptTemplateManager.Api.Tests/    # API integration tests
    ├── PromptTemplateManager.Application.Tests/  # Service unit tests
    └── PromptTemplateManager.Infrastructure.Tests/ # Repository tests

frontend/
├── src/
│   ├── components/                         # Reusable UI components (shadcn/ui)
│   │   ├── ui/                            # Base shadcn components
│   │   ├── templates/                     # TemplateList, TemplateEditor
│   │   ├── folders/                       # FolderTree, FolderDialog
│   │   └── placeholders/                  # PlaceholderForm, PromptPreview
│   ├── pages/                             # Route-level views
│   │   ├── Templates.tsx                  # Main template management view
│   │   ├── TemplateEditor.tsx            # Create/edit template
│   │   ├── TemplateUsage.tsx             # Placeholder filling & send
│   │   └── Search.tsx                     # Search & discovery
│   ├── lib/                               # Utilities & API client
│   │   ├── api/                          # Auto-generated TypeScript client
│   │   ├── hooks/                        # React hooks (useTemplates, useFolders)
│   │   └── utils/                        # Helpers (placeholder parsing)
│   ├── App.tsx                            # Root component, routing
│   ├── main.tsx                           # Vite entry point
│   └── vite.config.ts                     # Build config, OpenAPI codegen
├── tests/
│   ├── unit/                              # Component unit tests
│   └── integration/                       # E2E tests (Playwright/Vitest)
└── package.json                           # Dependencies, build scripts

shared/
└── openapi/                               # Generated OpenAPI spec (build output)
    └── swagger.json                       # From backend Swashbuckle
```

**Structure Decision**: Web application (Option 2) with clean architecture in backend. Backend follows Domain-Driven Design layers (Core/Application/Infrastructure/Api) for maintainability and testability. Frontend uses feature-based organization (components/pages) with auto-generated API client from OpenAPI spec. Shared `openapi/` directory holds generated spec for frontend build process consumption.

## Complexity Tracking

*No constitution violations identified. All gates passed.*

---

## Planning Summary

### Phase 0: Research Complete ✅

**Artifact**: [research.md](./research.md)

**Key Decisions**:
- Backend: C# 12 / .NET 8 with Clean Architecture (Core/Application/Infrastructure/Api)
- Frontend: Vite + React + TypeScript + shadcn/ui + Tailwind CSS
- Database: SQLite with Entity Framework Core
- Type Generation: Swashbuckle → OpenAPI → openapi-typescript-codegen
- Placeholder Syntax: `{{variable_name}}` with regex parsing
- Search: SQLite FTS5 extension for sub-second full-text search
- Testing: xUnit (backend) + Vitest (frontend)

### Phase 1: Design Complete ✅

**Artifacts**:
- [data-model.md](./data-model.md) - Entity definitions, relationships, validation rules
- [contracts/openapi.yaml](./contracts/openapi.yaml) - REST API specification (12 endpoints)
- [contracts/README.md](./contracts/README.md) - API documentation and usage guide
- [quickstart.md](./quickstart.md) - End-to-end integration scenarios

**Data Model**:
- **Template** entity: Guid Id, Name, Content (markdown), FolderId (nullable), timestamps
- **Folder** entity: Guid Id, Name, ParentFolderId (self-referential), timestamps
- **Derived**: PlaceholderInfo, PromptInstance (not persisted in MVP)

**API Endpoints**:
- Templates: CRUD operations (5 endpoints)
- Placeholders: Extract, generate, send to Devin (3 endpoints)
- Folders: Tree, CRUD operations (5 endpoints)
- Search: Full-text search (1 endpoint)

**Integration Scenarios**: 4 comprehensive scenarios covering all user stories

### Constitution Re-Check (Post-Design) ✅

All principles still aligned:
- ✅ Code Quality: Clean Architecture, strong typing, linting
- ✅ Testing: Test pyramid with xUnit/Vitest, contract validation, 80% coverage
- ✅ UX Consistency: shadcn/ui design system, WCAG 2.1 AA, responsive design
- ✅ Performance: <200ms API, <1s search, <200KB bundle, FTS5 indexing

### Next Steps

**Ready for**: `/speckit.tasks` command to generate task breakdown

**Recommended MVP Scope**: User Story 1 (P1) - Template CRUD operations
- Create template
- Edit template
- View template
- Delete template
- Placeholder support in content

**Incremental Delivery Path**:
1. **MVP (P1)**: Template CRUD → Deliverable standalone value
2. **Enhanced (P2)**: Add folder organization → Scale to larger libraries
3. **Complete (P3)**: Add template usage with Devin integration → End-to-end workflow
4. **Polish (P4)**: Add search/discovery → Power user optimization

### Estimated Implementation Effort

Based on clean architecture and test-first approach:

- **Setup & Infrastructure**: 6-8 tasks (projects, DI, middleware, migrations)
- **Foundational**: 8-10 tasks (base entities, repositories, error handling)
- **User Story 1 (P1)**: 10-12 tasks (Template CRUD + basic UI)
- **User Story 2 (P2)**: 8-10 tasks (Folder hierarchy + tree UI)
- **User Story 3 (P3)**: 8-10 tasks (Placeholder parsing + Devin stub)
- **User Story 4 (P4)**: 4-6 tasks (FTS5 search + search UI)
- **Polish**: 6-8 tasks (documentation, error handling, performance tuning)

**Total**: ~50-64 tasks for full feature set, ~25-30 tasks for MVP

### Design Artifacts Location

All design documents in:
```
specs/001-prompt-template-manager/
├── spec.md              ✅ Requirements & user stories
├── plan.md              ✅ This file - technical plan
├── research.md          ✅ Technology decisions
├── data-model.md        ✅ Entity design
├── quickstart.md        ✅ Integration scenarios
└── contracts/           ✅ API contracts
    ├── openapi.yaml
    └── README.md
```

**Agent Context Updated**: CLAUDE.md now includes C#/.NET and TypeScript guidance

---

**Planning Phase Complete** - Ready to proceed to task generation (`/speckit.tasks`)

