# Implementation Plan: Filesystem Template Storage

**Branch**: `006-filesystem-template-storage` | **Date**: 2025-10-22 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-filesystem-template-storage/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Replace the current SQLite database storage for templates with filesystem-based storage, where each template is stored as an individual markdown file with YAML frontmatter metadata. The folder hierarchy will be represented as a directory structure on disk. This approach enables version control integration, external editing, human-readable storage, and improved portability. The existing repository pattern architecture allows for seamless swapping of the database implementation with a filesystem-based implementation while maintaining all current functionality including full-text search, folder hierarchy, and template CRUD operations.

## Technical Context

**Language/Version**: C# .NET 9.0 (backend), TypeScript 5.9 (frontend)
**Primary Dependencies**: ASP.NET Core, Entity Framework Core, React 19, TanStack Query 5, Vite 7
**Storage**: Transitioning from SQLite to filesystem (.md files with YAML frontmatter)
**Testing**: NEEDS CLARIFICATION (no test framework currently configured)
**Target Platform**: Linux server (Alpine Docker container) + modern browsers
**Project Type**: Web application (separate backend/frontend with unified production build)
**Performance Goals**: Template operations <1s for files up to 100KB, search <2s for up to 10k templates
**Constraints**: Maintain current API contracts, preserve all existing functionality, support concurrent file access
**Scale/Scope**: Up to 10,000 templates, nested folders up to 5 levels deep, single-user focused with Docker deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Initial Check (Pre-Phase 0)

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Code Quality Standards** | ⚠️ MEDIUM | Repository pattern and DI already established. Risk: EF Core removal may reduce type safety for queries. Mitigation: Implement strong file validation and error handling. |
| **II. Testing Standards** | ❌ CRITICAL | No test framework configured. Constitution requires 80% line coverage for new code, 100% for critical paths (template CRUD = critical). **VIOLATION: Must add testing framework before implementation.** |
| **III. UX Consistency** | ✅ PASS | No UI changes required - storage layer is transparent to frontend. API contracts preserved. |
| **IV. Performance Requirements** | ✅ PASS | Success criteria (SC-001, SC-004) align with constitution's <200ms p95 for cached data, <1s for uncached. File I/O typically faster than SQLite for small files. |
| **Development Workflow** | ✅ PASS | Feature branch already created (006-filesystem-template-storage). Standard PR workflow applies. |
| **Quality Gates** | ❌ CRITICAL | Cannot pass "100% test suite passing" gate without test infrastructure. **VIOLATION: Blocks merge until tests implemented.** |

**Initial Gate Decision**: ❌ BLOCKED - Resolved in Phase 0 research with xUnit selection

---

### Re-evaluation (Post-Phase 1 Design)

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Code Quality Standards** | ✅ PASS | **RESOLVED**: Phase 1 design maintains repository pattern, adds strong type safety via YamlDotNet models (TemplateMetadata), comprehensive error handling documented in quickstart.md scenarios. FileNameSanitizer provides validation for filesystem operations. |
| **II. Testing Standards** | ✅ PASS | **RESOLVED**: xUnit selected in research.md with FluentAssertions and Moq. Test structure defined: unit tests (services, filesystem utilities) and integration tests (repositories with temp directories). Contract tests ensure API compatibility. |
| **III. UX Consistency** | ✅ PASS | **CONFIRMED**: contracts/README.md documents zero API changes. All endpoints, schemas, status codes preserved. Frontend requires no modifications. Design system and accessibility unchanged. |
| **IV. Performance Requirements** | ✅ PASS | **VALIDATED**: Lucene.NET search <100ms for 10k templates (target: <2s). File I/O <20ms for 100KB (target: <1s). Performance analysis in research.md Task 7 confirms compliance with constitution's <200ms p95 / <1s uncached requirements. |
| **Development Workflow** | ✅ PASS | **CONFIRMED**: Clean architecture preserved (layers: Api → Application → Core → Infrastructure). DI container approach unchanged. Phase 1 design aligns with existing patterns. |
| **Quality Gates** | ✅ PASS | **RESOLVED**: Test infrastructure defined, can now achieve "100% test suite passing" gate. Coverage targets: 80% for new code (filesystem utilities, repositories), 100% for critical paths (template CRUD operations). |

**Final Gate Decision**: ✅ PASS - All constitution violations resolved through Phase 0 research and Phase 1 design.

**Key Risk Mitigations**:
1. **Type Safety**: YamlDotNet provides compile-time type checking for metadata serialization
2. **Testing**: Comprehensive test strategy with xUnit covers unit, integration, and contract testing
3. **Performance**: Async I/O + in-memory caching + Lucene.NET ensures targets met
4. **Error Handling**: 7 integration scenarios in quickstart.md cover error cases (SC-006: 95% graceful handling)
5. **Code Quality**: Filename sanitization, file locking retry logic, migration verification all documented with implementation patterns

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
│   ├── PromptTemplateManager.Api/              # Presentation layer
│   │   ├── Controllers/
│   │   │   ├── TemplatesController.cs          # Template CRUD endpoints (unchanged)
│   │   │   └── FoldersController.cs            # Folder CRUD endpoints (unchanged)
│   │   ├── Program.cs                          # DI registration (change: register FileSystemTemplateRepository)
│   │   └── appsettings.json                    # Config (add: TemplateStoragePath)
│   ├── PromptTemplateManager.Application/      # Business logic layer
│   │   ├── Services/
│   │   │   ├── TemplateService.cs              # No changes required (uses ITemplateRepository)
│   │   │   ├── FolderService.cs                # No changes required (uses IFolderRepository)
│   │   │   └── PlaceholderService.cs           # No changes required
│   │   └── DTOs/                               # No changes required
│   ├── PromptTemplateManager.Core/             # Domain layer
│   │   ├── Entities/
│   │   │   ├── Template.cs                     # Remove EF Core attributes, keep properties
│   │   │   └── Folder.cs                       # Remove EF Core attributes, keep properties
│   │   └── Interfaces/
│   │       ├── ITemplateRepository.cs          # No changes required
│   │       └── IFolderRepository.cs            # No changes required
│   └── PromptTemplateManager.Infrastructure/   # Data access layer
│       ├── Repositories/
│       │   ├── TemplateRepository.cs           # [DEPRECATED] SQLite implementation
│       │   ├── FolderRepository.cs             # [DEPRECATED] SQLite implementation
│       │   ├── FileSystemTemplateRepository.cs # [NEW] Filesystem implementation
│       │   └── FileSystemFolderRepository.cs   # [NEW] Filesystem implementation
│       ├── FileSystem/                         # [NEW] Filesystem utilities
│       │   ├── TemplateFileManager.cs          # [NEW] File I/O, YAML parsing, locking
│       │   ├── FileNameSanitizer.cs            # [NEW] Special character handling
│       │   └── FileSystemSearchEngine.cs       # [NEW] Full-text search without SQLite
│       ├── Migration/                          # [NEW] Database-to-filesystem migration
│       │   └── DatabaseToFileSystemMigrator.cs # [NEW] One-time migration utility
│       └── Data/
│           ├── ApplicationDbContext.cs         # [DEPRECATED] Keep for migration only
│           └── Migrations/                     # [DEPRECATED] Keep for migration only
└── tests/                                      # [NEW] Test infrastructure
    ├── PromptTemplateManager.Tests.Unit/
    │   ├── Services/
    │   ├── Repositories/
    │   └── FileSystem/
    └── PromptTemplateManager.Tests.Integration/
        └── Repositories/

frontend/
├── src/
│   ├── components/                             # No changes required (API contracts preserved)
│   ├── lib/
│   │   ├── hooks/                              # No changes required
│   │   └── api/                                # No changes required
│   └── App.tsx                                 # No changes required

data/                                           # Storage location
├── templates/                                  # [NEW] Filesystem storage root
│   ├── {folder-name}/                          # User-created folders
│   │   └── {template-name}.md                  # Template markdown files
│   └── {template-name}.md                      # Root-level templates
└── prompttemplates.db                          # [DEPRECATED] Keep for migration, remove post-migration
```

**Structure Decision**: Web application (Option 2) with clean architecture layers. The repository pattern enables swapping SQLite repositories with filesystem implementations without touching business logic or API layers. New filesystem-specific utilities are isolated in `Infrastructure/FileSystem/` namespace. Tests are organized by layer (unit tests for services/utilities, integration tests for repositories).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Testing Standards (CRITICAL) | Constitution requires tests; none currently exist | Cannot meet 80% coverage requirement without test infrastructure. Must be addressed in Phase 0. |
| Quality Gates (CRITICAL) | Pre-merge gate requires "100% test suite passing" | No test suite exists to pass. Blocks merge until tests implemented for filesystem repositories. |

