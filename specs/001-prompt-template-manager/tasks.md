# Tasks: Prompt Template Manager

**Input**: Design documents from `/specs/001-prompt-template-manager/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL per spec - not explicitly requested

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- **Web app**: `backend/src/`, `frontend/src/`
- Backend projects: PromptTemplateManager.Api, .Core, .Application, .Infrastructure
- Frontend: React components in `frontend/src/components/`, pages in `frontend/src/pages/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create solution file and .NET project structure per plan.md at backend/
- [ ] T002 [P] Initialize backend PromptTemplateManager.Api project with ASP.NET Core 8.0 in backend/src/PromptTemplateManager.Api/
- [ ] T003 [P] Initialize backend PromptTemplateManager.Core class library in backend/src/PromptTemplateManager.Core/
- [ ] T004 [P] Initialize backend PromptTemplateManager.Application class library in backend/src/PromptTemplateManager.Application/
- [ ] T005 [P] Initialize backend PromptTemplateManager.Infrastructure class library in backend/src/PromptTemplateManager.Infrastructure/
- [ ] T006 [P] Initialize Vite + React + TypeScript project in frontend/
- [ ] T007 [P] Install backend NuGet packages (EF Core, Swashbuckle, FluentValidation, SQLite) in respective projects
- [ ] T008 [P] Install frontend npm packages (React 18, Vite, shadcn/ui, Tailwind, openapi-typescript-codegen) in frontend/package.json
- [ ] T009 [P] Configure Tailwind CSS in frontend/tailwind.config.js and frontend/src/index.css
- [ ] T010 [P] Configure ESLint and Prettier in frontend/.eslintrc.js and frontend/.prettierrc
- [ ] T011 [P] Configure StyleCop and dotnet format in backend/.editorconfig
- [ ] T012 Create shared/openapi/ directory for OpenAPI spec output

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T013 Configure dependency injection in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T014 [P] Create ApplicationDbContext in backend/src/PromptTemplateManager.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T015 [P] Configure SQLite connection and EF Core in backend/src/PromptTemplateManager.Api/appsettings.json
- [ ] T016 [P] Create global error handling middleware in backend/src/PromptTemplateManager.Api/Middleware/ErrorHandlingMiddleware.cs
- [ ] T017 [P] Create base DomainException in backend/src/PromptTemplateManager.Core/Exceptions/DomainException.cs
- [ ] T018 [P] Create ValidationException in backend/src/PromptTemplateManager.Core/Exceptions/ValidationException.cs
- [ ] T019 [P] Create NotFoundException in backend/src/PromptTemplateManager.Core/Exceptions/NotFoundException.cs
- [ ] T020 [P] Create ConflictException in backend/src/PromptTemplateManager.Core/Exceptions/ConflictException.cs
- [ ] T021 Configure Swashbuckle OpenAPI generation in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T022 [P] Configure CORS for frontend localhost:5173 in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T023 [P] Setup React Router in frontend/src/App.tsx
- [ ] T024 [P] Create React Error Boundary component in frontend/src/components/ErrorBoundary.tsx
- [ ] T025 [P] Initialize shadcn/ui components (Button, Input, Dialog, Toast) in frontend/src/components/ui/
- [ ] T026 Configure Vite OpenAPI codegen script in frontend/vite.config.ts
- [ ] T027 Add build scripts to frontend/package.json for generate:api, type-check, build

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Template CRUD Operations (Priority: P1) 🎯 MVP

**Goal**: Users can create, view, edit, and delete prompt templates with markdown content and placeholders

**Independent Test**: Create a new template → Edit content → View template → Delete template. All CRUD operations work independently.

### Implementation for User Story 1

- [ ] T028 [P] [US1] Create Template entity in backend/src/PromptTemplateManager.Core/Entities/Template.cs
- [ ] T029 [P] [US1] Configure Template entity mapping in backend/src/PromptTemplateManager.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T030 [P] [US1] Create ITemplateRepository interface in backend/src/PromptTemplateManager.Core/Interfaces/ITemplateRepository.cs
- [ ] T031 [P] [US1] Create CreateTemplateRequest DTO in backend/src/PromptTemplateManager.Application/DTOs/CreateTemplateRequest.cs
- [ ] T032 [P] [US1] Create UpdateTemplateRequest DTO in backend/src/PromptTemplateManager.Application/DTOs/UpdateTemplateRequest.cs
- [ ] T033 [P] [US1] Create TemplateResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/TemplateResponse.cs
- [ ] T034 [P] [US1] Create TemplateSummary DTO in backend/src/PromptTemplateManager.Application/DTOs/TemplateSummary.cs
- [ ] T035 [P] [US1] Create TemplateListResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/TemplateListResponse.cs
- [ ] T036 [P] [US1] Create FluentValidation validator for CreateTemplateRequest in backend/src/PromptTemplateManager.Application/Validators/CreateTemplateRequestValidator.cs
- [ ] T037 [P] [US1] Create FluentValidation validator for UpdateTemplateRequest in backend/src/PromptTemplateManager.Application/Validators/UpdateTemplateRequestValidator.cs
- [ ] T038 [US1] Implement TemplateRepository in backend/src/PromptTemplateManager.Infrastructure/Repositories/TemplateRepository.cs
- [ ] T039 [US1] Create ITemplateService interface in backend/src/PromptTemplateManager.Core/Interfaces/ITemplateService.cs
- [ ] T040 [US1] Implement TemplateService with CRUD logic in backend/src/PromptTemplateManager.Application/Services/TemplateService.cs
- [ ] T041 [US1] Create TemplatesController with CRUD endpoints in backend/src/PromptTemplateManager.Api/Controllers/TemplatesController.cs
- [ ] T042 [US1] Create and apply initial EF Core migration for Template entity in backend/src/PromptTemplateManager.Infrastructure/Data/Migrations/
- [ ] T043 [US1] Build backend and generate OpenAPI spec to shared/openapi/swagger.json
- [ ] T044 [US1] Run openapi-typescript-codegen to generate TypeScript client in frontend/src/lib/api/
- [ ] T045 [P] [US1] Create useTemplates custom hook in frontend/src/lib/hooks/useTemplates.ts
- [ ] T046 [P] [US1] Create TemplateList component in frontend/src/components/templates/TemplateList.tsx
- [ ] T047 [P] [US1] Create TemplateEditor component in frontend/src/components/templates/TemplateEditor.tsx
- [ ] T048 [P] [US1] Create TemplateDialog component for create/edit in frontend/src/components/templates/TemplateDialog.tsx
- [ ] T049 [US1] Create Templates page with list + CRUD operations in frontend/src/pages/Templates.tsx
- [ ] T050 [US1] Add Templates route to React Router in frontend/src/App.tsx
- [ ] T051 [US1] Add error handling and toast notifications for template operations in frontend/src/pages/Templates.tsx

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently (MVP COMPLETE)

---

## Phase 4: User Story 2 - Folder Organization (Priority: P2)

**Goal**: Users can organize templates into hierarchical folder structures

**Independent Test**: Create folders → Create subfolders → Move templates between folders → Rename folders → Delete empty folders. Folder hierarchy works independently.

### Implementation for User Story 2

- [X] T052 [P] [US2] Create Folder entity in backend/src/PromptTemplateManager.Core/Entities/Folder.cs
- [X] T053 [P] [US2] Configure Folder entity mapping with self-referential relationship in backend/src/PromptTemplateManager.Infrastructure/Data/ApplicationDbContext.cs
- [X] T054 [P] [US2] Update Template entity mapping to include Folder navigation property in backend/src/PromptTemplateManager.Infrastructure/Data/ApplicationDbContext.cs
- [X] T055 [P] [US2] Create IFolderRepository interface in backend/src/PromptTemplateManager.Core/Interfaces/IFolderRepository.cs
- [X] T056 [P] [US2] Create CreateFolderRequest DTO in backend/src/PromptTemplateManager.Application/DTOs/CreateFolderRequest.cs
- [X] T057 [P] [US2] Create UpdateFolderRequest DTO in backend/src/PromptTemplateManager.Application/DTOs/UpdateFolderRequest.cs
- [X] T058 [P] [US2] Create FolderResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/FolderResponse.cs
- [X] T059 [P] [US2] Create FolderTreeNode DTO in backend/src/PromptTemplateManager.Application/DTOs/FolderTreeNode.cs
- [X] T060 [P] [US2] Create FolderTreeResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/FolderTreeResponse.cs
- [X] T061 [P] [US2] Create FolderDetailsResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/FolderDetailsResponse.cs
- [X] T062 [P] [US2] Create FluentValidation validator for CreateFolderRequest in backend/src/PromptTemplateManager.Application/Validators/CreateFolderRequestValidator.cs
- [X] T063 [P] [US2] Create FluentValidation validator for UpdateFolderRequest in backend/src/PromptTemplateManager.Application/Validators/UpdateFolderRequestValidator.cs
- [X] T064 [US2] Implement FolderRepository with recursive tree query in backend/src/PromptTemplateManager.Infrastructure/Repositories/FolderRepository.cs
- [X] T065 [US2] Create IFolderService interface in backend/src/PromptTemplateManager.Application/Services/IFolderService.cs
- [X] T066 [US2] Implement FolderService with circular reference validation in backend/src/PromptTemplateManager.Application/Services/FolderService.cs
- [X] T067 [US2] Create FoldersController with tree and CRUD endpoints in backend/src/PromptTemplateManager.Api/Controllers/FoldersController.cs
- [X] T068 [US2] Create and apply EF Core migration for Folder entity in backend/src/PromptTemplateManager.Infrastructure/Data/Migrations/
- [X] T069 [US2] Rebuild backend and regenerate OpenAPI spec to shared/openapi/swagger.json
- [X] T070 [US2] Regenerate TypeScript client in frontend/src/lib/api/
- [X] T071 [P] [US2] Create useFolders custom hook in frontend/src/lib/hooks/useFolders.ts
- [X] T072 [P] [US2] Create FolderTree component in frontend/src/components/folders/FolderTree.tsx
- [X] T073 [P] [US2] Create FolderDialog component for create/rename in frontend/src/components/folders/FolderDialog.tsx
- [X] T074 [P] [US2] Create FolderContextMenu component for folder actions in frontend/src/components/folders/FolderContextMenu.tsx
- [X] T075 [US2] Update Templates page to include FolderTree navigation in frontend/src/App.tsx
- [X] T076 [US2] Update TemplateDialog to support folder selection in frontend/src/components/TemplateForm.tsx
- [X] T077 [US2] Implement drag-and-drop for moving templates between folders in frontend/src/App.tsx

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Template Usage with Placeholder Substitution (Priority: P3)

**Goal**: Users can fill placeholder values and send prompts to Devin LLM

**Independent Test**: Select template with placeholders → Fill values → Preview prompt → Send to Devin. Placeholder workflow works independently.

### Implementation for User Story 3

- [ ] T078 [P] [US3] Create PlaceholderInfo DTO in backend/src/PromptTemplateManager.Application/DTOs/PlaceholderInfo.cs
- [ ] T079 [P] [US3] Create PlaceholderListResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/PlaceholderListResponse.cs
- [ ] T080 [P] [US3] Create GeneratePromptRequest DTO in backend/src/PromptTemplateManager.Application/DTOs/GeneratePromptRequest.cs
- [ ] T081 [P] [US3] Create PromptInstanceResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/PromptInstanceResponse.cs
- [ ] T082 [P] [US3] Create SendPromptResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/SendPromptResponse.cs
- [ ] T083 [P] [US3] Create IPlaceholderService interface in backend/src/PromptTemplateManager.Core/Interfaces/IPlaceholderService.cs
- [ ] T084 [P] [US3] Create IDevinClient interface in backend/src/PromptTemplateManager.Core/Interfaces/IDevinClient.cs
- [ ] T085 [US3] Implement PlaceholderService with regex parsing in backend/src/PromptTemplateManager.Application/Services/PlaceholderService.cs
- [ ] T086 [US3] Implement DevinClient stub (returns success immediately) in backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinClient.cs
- [ ] T087 [US3] Add placeholder endpoints to TemplatesController in backend/src/PromptTemplateManager.Api/Controllers/TemplatesController.cs
- [ ] T088 [US3] Rebuild backend and regenerate OpenAPI spec to shared/openapi/swagger.json
- [ ] T089 [US3] Regenerate TypeScript client in frontend/src/lib/api/
- [ ] T090 [P] [US3] Create usePlaceholders custom hook in frontend/src/lib/hooks/usePlaceholders.ts
- [ ] T091 [P] [US3] Create placeholder parsing utility in frontend/src/lib/utils/placeholders.ts
- [ ] T092 [P] [US3] Create PlaceholderForm component in frontend/src/components/placeholders/PlaceholderForm.tsx
- [ ] T093 [P] [US3] Create PromptPreview component in frontend/src/components/placeholders/PromptPreview.tsx
- [ ] T094 [US3] Create TemplateUsage page with placeholder workflow in frontend/src/pages/TemplateUsage.tsx
- [ ] T095 [US3] Add TemplateUsage route to React Router in frontend/src/App.tsx
- [ ] T096 [US3] Add "Use Template" action button to TemplateList in frontend/src/components/templates/TemplateList.tsx

**Checkpoint**: All user stories 1, 2, AND 3 should now be independently functional

---

## Phase 6: User Story 4 - Template Search and Discovery (Priority: P4)

**Goal**: Users can search templates by name and content using full-text search

**Independent Test**: Create multiple templates → Search by keyword → Verify results → Clear search. Search works independently.

### Implementation for User Story 4

- [ ] T097 [US4] Create SQLite FTS5 virtual table in EF Core migration in backend/src/PromptTemplateManager.Infrastructure/Data/Migrations/
- [ ] T098 [US4] Add FTS5 triggers for insert/update/delete in migration in backend/src/PromptTemplateManager.Infrastructure/Data/Migrations/
- [ ] T099 [US4] Add SearchTemplates method to ITemplateRepository in backend/src/PromptTemplateManager.Core/Interfaces/ITemplateRepository.cs
- [ ] T100 [US4] Implement FTS5 search query in TemplateRepository in backend/src/PromptTemplateManager.Infrastructure/Repositories/TemplateRepository.cs
- [ ] T101 [US4] Add SearchTemplates method to ITemplateService in backend/src/PromptTemplateManager.Core/Interfaces/ITemplateService.cs
- [ ] T102 [US4] Implement search logic in TemplateService in backend/src/PromptTemplateManager.Application/Services/TemplateService.cs
- [ ] T103 [US4] Create SearchController with search endpoint in backend/src/PromptTemplateManager.Api/Controllers/SearchController.cs
- [ ] T104 [US4] Rebuild backend and regenerate OpenAPI spec to shared/openapi/swagger.json
- [ ] T105 [US4] Regenerate TypeScript client in frontend/src/lib/api/
- [ ] T106 [P] [US4] Create useSearch custom hook in frontend/src/lib/hooks/useSearch.ts
- [ ] T107 [P] [US4] Create SearchBar component with debounced input in frontend/src/components/search/SearchBar.tsx
- [ ] T108 [P] [US4] Create SearchResults component in frontend/src/components/search/SearchResults.tsx
- [ ] T109 [US4] Create Search page in frontend/src/pages/Search.tsx
- [ ] T110 [US4] Add Search route to React Router in frontend/src/App.tsx
- [ ] T111 [US4] Add search input to Templates page header in frontend/src/pages/Templates.tsx

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T112 [P] Add loading states to all async operations in frontend/src/components/
- [ ] T113 [P] Add accessibility attributes (aria-labels, keyboard navigation) to interactive components in frontend/src/components/
- [ ] T114 [P] Implement response compression middleware in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T115 [P] Add request logging middleware in backend/src/PromptTemplateManager.Api/Middleware/RequestLoggingMiddleware.cs
- [ ] T116 [P] Configure performance monitoring in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T117 [P] Add bundle size check to frontend build script in frontend/package.json
- [ ] T118 [P] Optimize Vite build with code splitting in frontend/vite.config.ts
- [ ] T119 Create .gitignore files for backend/ and frontend/
- [ ] T120 Create .dockerignore file at repository root
- [ ] T121 Validate quickstart.md scenarios manually (all 4 scenarios)
- [ ] T122 [P] Update README.md with setup and run instructions at repository root
- [ ] T123 [P] Document API endpoints in backend/README.md
- [ ] T124 [P] Document frontend architecture in frontend/README.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3 → P4)
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Integrates with US1 but independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Uses templates from US1 but independently testable
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Searches templates from US1 but independently testable

### Within Each User Story

- Backend entities before services
- Services before controllers
- Migrations before API endpoints
- OpenAPI generation before TypeScript client generation
- TypeScript client before frontend components
- Components before pages
- Pages before route configuration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T002-T011)
- All Foundational tasks marked [P] can run in parallel (T014-T027)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Within each user story:
  - All DTO creation tasks [P] can run in parallel
  - All validator creation tasks [P] can run in parallel
  - All frontend component tasks [P] can run in parallel (after TypeScript client generated)
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# After T027 completes, launch all DTOs and validators together:
Task: T031 - CreateTemplateRequest DTO
Task: T032 - UpdateTemplateRequest DTO
Task: T033 - TemplateResponse DTO
Task: T034 - TemplateSummary DTO
Task: T035 - TemplateListResponse DTO
Task: T036 - CreateTemplateRequest validator
Task: T037 - UpdateTemplateRequest validator

# After T044 (TypeScript client generation), launch all frontend components:
Task: T045 - useTemplates hook
Task: T046 - TemplateList component
Task: T047 - TemplateEditor component
Task: T048 - TemplateDialog component
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T012)
2. Complete Phase 2: Foundational (T013-T027) - CRITICAL - blocks all stories
3. Complete Phase 3: User Story 1 (T028-T051)
4. **STOP and VALIDATE**: Test User Story 1 independently (create/edit/view/delete templates)
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Polish phase → Final deployment
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (T001-T027)
2. Once Foundational is done (after T027):
   - Developer A: User Story 1 (T028-T051)
   - Developer B: User Story 2 (T052-T077)
   - Developer C: User Story 3 (T078-T096)
   - Developer D: User Story 4 (T097-T111)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- OpenAPI → TypeScript codegen happens after backend builds (T043, T069, T088, T104)
- Frontend builds depend on TypeScript client being generated first
