# Tasks: Filesystem Template Storage

**Input**: Design documents from `/specs/006-filesystem-template-storage/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are NOT explicitly requested in the feature specification. However, the constitution requires 80% line coverage for new code and 100% for critical paths (template CRUD operations are critical). Tests MUST be included to meet constitution requirements.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions
- **Backend**: `backend/src/`
- **Frontend**: `frontend/src/` (no changes needed per contracts/README.md)
- **Tests**: `backend/tests/`
- **Data**: `/data/` (Docker volume mount)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Test infrastructure and filesystem utilities setup

- [X] T001 Create test project PromptTemplateManager.Tests.Unit in backend/tests/
- [X] T002 [P] Create test project PromptTemplateManager.Tests.Integration in backend/tests/
- [X] T003 [P] Add xUnit, FluentAssertions, and Moq packages to test projects per research.md Task 1
- [X] T004 [P] Add YamlDotNet package to Infrastructure project per research.md Task 2
- [X] T005 [P] Add Lucene.NET packages to Infrastructure project per research.md Task 3
- [X] T006 Add TemplateStoragePath configuration to backend/src/PromptTemplateManager.Api/appsettings.json

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core filesystem utilities that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Create FileNameSanitizer utility in backend/src/PromptTemplateManager.Infrastructure/FileSystem/FileNameSanitizer.cs per research.md Task 5
- [X] T008 [P] Create TemplateMetadata model in backend/src/PromptTemplateManager.Infrastructure/FileSystem/TemplateMetadata.cs per data-model.md
- [X] T009 [P] Create FolderMetadata model in backend/src/PromptTemplateManager.Infrastructure/FileSystem/FolderMetadata.cs per data-model.md
- [X] T010 Create TemplateFileManager for YAML frontmatter parsing/serialization in backend/src/PromptTemplateManager.Infrastructure/FileSystem/TemplateFileManager.cs per research.md Task 2
- [X] T011 [P] Create FileSystemSearchEngine with Lucene.NET in backend/src/PromptTemplateManager.Infrastructure/FileSystem/FileSystemSearchEngine.cs per research.md Task 3
- [X] T012 Remove EF Core attributes from Template entity in backend/src/PromptTemplateManager.Core/Entities/Template.cs
- [X] T013 [P] Remove EF Core attributes from Folder entity in backend/src/PromptTemplateManager.Core/Entities/Folder.cs
- [X] T014 Unit test FileNameSanitizer with illegal characters edge cases in backend/tests/PromptTemplateManager.Tests.Unit/FileSystem/FileNameSanitizerTests.cs
- [X] T015 [P] Unit test TemplateFileManager YAML parsing with valid/invalid frontmatter in backend/tests/PromptTemplateManager.Tests.Unit/FileSystem/TemplateFileManagerTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Store Templates as Files (Priority: P1) 🎯 MVP

**Goal**: Users can create, read, update, and delete templates that are persisted as individual files on the filesystem

**Independent Test**: Create template through UI → verify .md file exists with correct content → modify file externally → refresh UI to see changes → delete template to verify file removal

### Tests for User Story 1

**NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T016 [P] [US1] Contract test for POST /api/templates in backend/tests/PromptTemplateManager.Tests.Integration/Controllers/TemplatesControllerTests.cs
- [X] T017 [P] [US1] Contract test for GET /api/templates/{id} in backend/tests/PromptTemplateManager.Tests.Integration/Controllers/TemplatesControllerTests.cs
- [X] T018 [P] [US1] Contract test for PUT /api/templates/{id} in backend/tests/PromptTemplateManager.Tests.Integration/Controllers/TemplatesControllerTests.cs
- [X] T019 [P] [US1] Contract test for DELETE /api/templates/{id} in backend/tests/PromptTemplateManager.Tests.Integration/Controllers/TemplatesControllerTests.cs
- [X] T020 [P] [US1] Integration test for template CRUD lifecycle with temp directory in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemTemplateRepositoryTests.cs

### Implementation for User Story 1

- [X] T021 [US1] Create FileSystemTemplateRepository implementing ITemplateRepository in backend/src/PromptTemplateManager.Infrastructure/Repositories/FileSystemTemplateRepository.cs per plan.md structure
- [X] T022 [US1] Implement CreateAsync with file locking and YAML frontmatter in FileSystemTemplateRepository per research.md Task 4
- [X] T023 [US1] Implement GetByIdAsync with cache support in FileSystemTemplateRepository per research.md Task 7
- [X] T024 [US1] Implement GetAllAsync with directory scanning in FileSystemTemplateRepository
- [X] T025 [US1] Implement UpdateAsync with file rename/move support in FileSystemTemplateRepository per quickstart.md Scenario 5
- [X] T026 [US1] Implement DeleteAsync with cache invalidation in FileSystemTemplateRepository
- [X] T027 [US1] Integrate FileSystemSearchEngine for full-text search in FileSystemTemplateRepository per data-model.md indexing strategy
- [X] T028 [US1] Add FileSystemWatcher for external file change detection in FileSystemTemplateRepository per research.md Task 7
- [X] T029 [US1] Add retry logic for file lock conflicts in FileSystemTemplateRepository per research.md Task 4
- [X] T030 [US1] Add error handling for filesystem failures in FileSystemTemplateRepository per quickstart.md Scenario 6
- [X] T031 [US1] Configure DI to register FileSystemTemplateRepository in backend/src/PromptTemplateManager.Api/Program.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - templates can be created/read/updated/deleted as files

---

## Phase 4: User Story 2 - Maintain Folder Hierarchy (Priority: P2)

**Goal**: Users can organize templates into folders with folder structure represented as directory hierarchy on filesystem

**Independent Test**: Create folder in UI → verify directory created → move template between folders → verify file relocation → delete folder to verify directory removal

### Tests for User Story 2

- [X] T032 [P] [US2] Contract test for POST /api/folders in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemFolderRepositoryTests.cs
- [X] T033 [P] [US2] Contract test for GET /api/folders/tree in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemFolderRepositoryTests.cs
- [X] T034 [P] [US2] Contract test for PUT /api/folders/{id} (move/rename) in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemFolderRepositoryTests.cs
- [X] T035 [P] [US2] Contract test for DELETE /api/folders/{id} in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemFolderRepositoryTests.cs
- [X] T036 [P] [US2] Integration test for folder hierarchy operations with temp directory in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemFolderRepositoryTests.cs

### Implementation for User Story 2

- [X] T037 [US2] Create FileSystemFolderRepository implementing IFolderRepository in backend/src/PromptTemplateManager.Infrastructure/Repositories/FileSystemFolderRepository.cs per plan.md structure
- [X] T038 [US2] Implement CreateAsync with .folder-meta generation in FileSystemFolderRepository per data-model.md folder metadata
- [X] T039 [US2] Implement GetByIdAsync with .folder-meta parsing in FileSystemFolderRepository
- [X] T040 [US2] Implement GetAllAsync with directory tree scanning in FileSystemFolderRepository
- [X] T041 [US2] Implement GetTreeAsync with recursive tree construction in FileSystemFolderRepository per data-model.md relationships
- [X] T042 [US2] Implement UpdateAsync with directory rename/move in FileSystemFolderRepository per quickstart.md Scenario 5
- [X] T043 [US2] Implement DeleteAsync with empty folder validation in FileSystemFolderRepository per data-model.md cascading behavior
- [X] T044 [US2] Implement HasCircularReferenceAsync for move validation in FileSystemFolderRepository per data-model.md circular reference prevention
- [X] T045 [US2] Implement GetDepthAsync for max depth validation (5 levels) in FileSystemFolderRepository per data-model.md max depth enforcement
- [X] T046 [US2] Configure DI to register FileSystemFolderRepository in backend/src/PromptTemplateManager.Api/Program.cs
- [X] T047 [US2] Update FileSystemTemplateRepository to handle template moves between folders per quickstart.md Scenario 5

**Checkpoint**: All folder operations functional - templates can be organized in nested folder structures

---

## Phase 5: User Story 3 - Metadata Storage (Priority: P3)

**Goal**: System maintains template metadata (ID, created/updated dates) in YAML frontmatter without database storage

**Independent Test**: Create template → verify metadata in frontmatter → perform search using metadata → ensure application reconstructs template list with proper IDs after restart

### Tests for User Story 3

- [X] T048 [P] [US3] Integration test for metadata persistence across restarts in backend/tests/PromptTemplateManager.Tests.Integration/Repositories/FileSystemTemplateRepositoryTests.cs
- [X] T049 [P] [US3] Integration test for search using metadata fields in backend/tests/PromptTemplateManager.Tests.Integration/Search/FileSystemSearchEngineTests.cs
- [X] T050 [P] [US3] Contract test for GET /api/search/templates with metadata filtering in backend/tests/PromptTemplateManager.Tests.Integration/Controllers/SearchControllerTests.cs

### Implementation for User Story 3

- [X] T051 [P] [US3] Add metadata fields (created, updated timestamps) to Lucene index in FileSystemSearchEngine per data-model.md indexing strategy
- [X] T052 [US3] Implement metadata-based search ranking in FileSystemSearchEngine
- [X] T053 [US3] Add application startup scan to rebuild template list from .md files in backend/src/PromptTemplateManager.Api/Program.cs per quickstart.md Scenario 1
- [X] T054 [US3] Implement cache warming on startup for frequently accessed templates in FileSystemTemplateRepository per research.md Task 7
- [X] T055 [US3] Add UpdatedAt timestamp auto-refresh on template save in FileSystemTemplateRepository per data-model.md metadata evolution

**Checkpoint**: All metadata functionality working - templates have persistent IDs and searchable timestamps

---

## Phase 6: Migration & Data Transition

**Purpose**: One-time migration from SQLite to filesystem storage

- [ ] T056 Create DatabaseToFileSystemMigrator in backend/src/PromptTemplateManager.Infrastructure/Migration/DatabaseToFileSystemMigrator.cs per research.md Task 6
- [ ] T057 Implement MigrateAsync with backup creation in DatabaseToFileSystemMigrator per research.md Task 6
- [ ] T058 Implement folder migration (parent folders first) in DatabaseToFileSystemMigrator per data-model.md migration mapping
- [ ] T059 Implement template migration with YAML frontmatter generation in DatabaseToFileSystemMigrator per data-model.md migration mapping
- [ ] T060 Implement Lucene index rebuild from migrated files in DatabaseToFileSystemMigrator
- [ ] T061 Implement migration verification (file count vs database count) in DatabaseToFileSystemMigrator per research.md Task 6
- [ ] T062 Add migration trigger on application startup in backend/src/PromptTemplateManager.Api/Program.cs per quickstart.md Scenario 2
- [ ] T063 [P] Integration test for migration with sample SQLite database in backend/tests/PromptTemplateManager.Tests.Integration/Migration/DatabaseToFileSystemMigratorTests.cs
- [ ] T064 [P] Integration test for migration rollback on failure in backend/tests/PromptTemplateManager.Tests.Integration/Migration/DatabaseToFileSystemMigratorTests.cs

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Performance, observability, and production readiness

- [ ] T065 [P] Add comprehensive logging for all filesystem operations in FileSystemTemplateRepository and FileSystemFolderRepository
- [ ] T066 [P] Add performance metrics for search operations in FileSystemSearchEngine per quickstart.md Scenario 7
- [ ] T067 [P] Implement IMemoryCache size limits and eviction policy in FileSystemTemplateRepository per research.md Task 7
- [ ] T068 Run quickstart.md validation scenarios 1-7 against implementation
- [ ] T069 Verify success criteria SC-001 through SC-006 from spec.md
- [ ] T070 [P] Update appsettings.json with production-ready cache and Lucene settings
- [ ] T071 [P] Add health check for filesystem access and Lucene index in backend/src/PromptTemplateManager.Api/Program.cs
- [ ] T072 Performance test with 10,000 templates per quickstart.md Scenario 7
- [ ] T073 [P] Security review for path traversal prevention in FileNameSanitizer
- [ ] T074 Verify test coverage meets 80% for new code and 100% for critical paths (template CRUD)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Phase 2 - No dependencies on other stories
  - User Story 2 (P2): Can start after Phase 2 - Independent but may integrate with US1
  - User Story 3 (P3): Can start after Phase 2 - Builds on US1 and US2 but independently testable
- **Migration (Phase 6)**: Can start after US1 and US2 complete (needs both template and folder repositories)
- **Polish (Phase 7)**: Depends on all user stories and migration complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Core template storage functionality
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Folder operations independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Metadata enhancements independently verifiable

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Repository implementation before DI registration
- Core operations before advanced features (caching, FileSystemWatcher)
- Error handling after happy path implementation

### Parallel Opportunities

**Phase 1 (Setup):**
- T002-T005 can all run in parallel (different projects/packages)

**Phase 2 (Foundational):**
- T008-T009 can run in parallel (different model files)
- T011 can run parallel to T010 (different utility classes)
- T012-T013 can run in parallel (different entity files)
- T014-T015 can run in parallel (different test files)

**User Story 1 (Tests):**
- T016-T020 can all run in parallel (different test files/methods)

**User Story 2 (Tests):**
- T032-T036 can all run in parallel (different test files/methods)

**User Story 3 (Tests):**
- T048-T050 can all run in parallel (different test files)

**User Story 3 (Implementation):**
- T051-T052 can run in parallel (both in FileSystemSearchEngine)

**Phase 6 (Migration Tests):**
- T063-T064 can run in parallel (different test scenarios)

**Phase 7 (Polish):**
- T065-T067 can run in parallel (different components)
- T070-T071 can run in parallel (configuration files)
- T073 can run parallel to T072 (different concerns)

---

## Parallel Example: User Story 1 Implementation

```bash
# After tests written (T016-T020), launch tests together:
Task: T016 Contract test for POST /api/templates
Task: T017 Contract test for GET /api/templates/{id}
Task: T018 Contract test for PUT /api/templates/{id}
Task: T019 Contract test for DELETE /api/templates/{id}
Task: T020 Integration test for template CRUD lifecycle

# All tests should FAIL initially - this is correct (TDD)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: Foundational (T007-T015) **← CRITICAL BLOCKER**
3. Complete Phase 3: User Story 1 (T016-T031)
4. **STOP and VALIDATE**: Test User Story 1 independently per spec.md acceptance scenarios
5. Skip migration temporarily (use fresh install testing)
6. Deploy/demo basic template storage working

### Incremental Delivery

1. **Foundation**: Setup + Foundational → Core utilities ready
2. **MVP**: + User Story 1 → Template CRUD working → Test independently → Deploy
3. **Enhanced**: + User Story 2 → Folder organization working → Test independently → Deploy
4. **Complete**: + User Story 3 → Metadata/search working → Test independently → Deploy
5. **Production**: + Migration (Phase 6) → Existing users can upgrade → Deploy
6. **Optimized**: + Polish (Phase 7) → Performance validated → Final deploy

### Parallel Team Strategy

With multiple developers after Foundational phase completes:

1. **Team completes Setup + Foundational together** (T001-T015)
2. **Once Foundational done, split:**
   - Developer A: User Story 1 (T016-T031)
   - Developer B: User Story 2 (T032-T047)
   - Developer C: User Story 3 (T048-T055)
3. **Merge and integrate:** Stories merge back independently
4. **Final sprint together:** Migration (T056-T064) + Polish (T065-T074)

---

## Notes

- **[P] tasks**: Different files, no dependencies - safe to parallelize
- **[Story] label**: Maps task to user story for traceability and independent delivery
- **Tests are REQUIRED**: Constitution mandates 80% coverage (new code) and 100% (critical paths)
- **TDD approach**: Write tests first (should fail), then implement until tests pass
- **No frontend changes**: All API contracts preserved per contracts/README.md
- **Constitution compliance**: Resolved testing violations from plan.md through xUnit setup
- **Migration is optional for new users**: Fresh installs skip to filesystem storage directly
- **Each user story delivers value independently**: Can stop after any story and have working system
- **Commit strategy**: Commit after each task or logical group of [P] tasks
- **Stop at checkpoints**: Validate each user story independently before proceeding

---

## Total Task Count

- **Phase 1 (Setup)**: 6 tasks
- **Phase 2 (Foundational)**: 9 tasks
- **Phase 3 (User Story 1)**: 16 tasks (5 tests + 11 implementation)
- **Phase 4 (User Story 2)**: 16 tasks (5 tests + 11 implementation)
- **Phase 5 (User Story 3)**: 8 tasks (3 tests + 5 implementation)
- **Phase 6 (Migration)**: 9 tasks (7 implementation + 2 tests)
- **Phase 7 (Polish)**: 10 tasks

**Total**: 74 tasks

**Parallel Opportunities**: 28 tasks marked [P] (38% of tasks can run in parallel within their phase)

**MVP Scope**: Phases 1-3 = 31 tasks (42% of total) delivers working template storage

**Test Coverage**: 15 test tasks + validation tasks = meets constitution 80%/100% requirements
