---
description: "Task list for Devin API Integration"
---

# Tasks: Devin API Integration

**Input**: Design documents from `/specs/007-devin-api-integration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/
**Branch**: `007-devin-api-integration`

**Tests**: Tests are OPTIONAL for this feature and are NOT included (not explicitly requested in spec).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions
- **Web app structure**: `backend/src/`, `frontend/src/`
- Backend follows layered architecture: Api → Application → Core ← Infrastructure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Configuration infrastructure and base models for Devin integration

- [X] T001 [P] Create DevinApiOptions configuration model in backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiOptions.cs
- [X] T002 [P] Create DevinApiModels with request/response DTOs in backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiModels.cs
- [X] T003 [P] Create DevinApiException domain exception in backend/src/PromptTemplateManager.Core/Exceptions/DevinApiException.cs
- [X] T004 [P] Create DevinSessionResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/DevinSessionResponse.cs
- [X] T005 [P] Create DevinErrorResponse DTO in backend/src/PromptTemplateManager.Application/DTOs/DevinErrorResponse.cs
- [X] T006 Update SendPromptResponse to add SessionUrl field in backend/src/PromptTemplateManager.Application/DTOs/SendPromptResponse.cs
- [X] T007 Update appsettings.json with DevinApi configuration section in backend/src/PromptTemplateManager.Api/appsettings.json
- [X] T008 Update appsettings.Development.json with dev configuration in backend/src/PromptTemplateManager.Api/appsettings.Development.json

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core DevinClient implementation that all user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T009 Implement DevinClient with HttpClient and API request logic in backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinClient.cs
- [X] T010 Add HTTP status code error mapping to user-friendly messages in DevinClient
- [X] T011 Add timeout and request cancellation support in DevinClient
- [X] T012 Add structured logging with sensitive data masking in DevinClient
- [X] T013 Register HttpClient with IHttpClientFactory in backend/src/PromptTemplateManager.Api/Program.cs
- [X] T014 Configure DevinApiOptions binding in backend/src/PromptTemplateManager.Api/Program.cs

**Checkpoint**: DevinClient infrastructure ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Send Template to Devin Session (Priority: P1) 🎯 MVP

**Goal**: Enable users to click "Send to Devin" button, create a session, and receive a clickable link

**Independent Test**: Click "Send to Devin" with valid API key configured, verify clickable session link is displayed

### Implementation for User Story 1

- [X] T015 [US1] Update TemplatesController SendPrompt endpoint to call DevinClient in backend/src/PromptTemplateManager.Api/Controllers/TemplatesController.cs
- [X] T016 [US1] Add error handling for DevinApiException in TemplatesController SendPrompt endpoint
- [X] T017 [US1] Map DevinSessionResponse to SendPromptResponse with SessionUrl in TemplatesController
- [X] T018 [US1] Update frontend TemplatesPage to display clickable session link in frontend/src/pages/TemplateUsage.tsx
- [X] T019 [US1] Add loading state to "Send to Devin" button in frontend/src/pages/TemplateUsage.tsx
- [X] T020 [US1] Add success toast notification with session link in frontend/src/pages/TemplateUsage.tsx
- [X] T021 [US1] Ensure session link opens in new browser tab (_blank target) in frontend

**Checkpoint**: User Story 1 complete - users can send templates to Devin and receive clickable links

---

## Phase 4: User Story 2 - Configure Devin API Key (Priority: P2)

**Goal**: Provide mechanism to configure Devin API key via environment variable or appsettings

**Independent Test**: Set API key via environment variable, verify it's used for API authentication

### Implementation for User Story 2

- [ ] T022 [P] [US2] Add API key validation on first use in DevinClient
- [ ] T023 [P] [US2] Add configuration validation in DevinApiOptions with helpful error messages
- [ ] T024 [P] [US2] Document environment variable configuration in README or CLAUDE.md
- [ ] T025 [US2] Update Dockerfile to support DevinApi__ApiKey environment variable injection
- [ ] T026 [US2] Test API key configuration via environment variables in Docker container
- [ ] T027 [US2] Verify API key is never logged or exposed in client-side code

**Checkpoint**: User Story 2 complete - API key can be securely configured

---

## Phase 5: User Story 3 - Handle API Errors Gracefully (Priority: P3)

**Goal**: Display clear, user-friendly error messages for all Devin API failure scenarios

**Independent Test**: Simulate various API failures (invalid key, network error, rate limits) and verify appropriate error messages

### Implementation for User Story 3

- [ ] T028 [P] [US3] Implement error mapping for 400 Bad Request in DevinClient
- [ ] T029 [P] [US3] Implement error mapping for 401 Unauthorized in DevinClient
- [ ] T030 [P] [US3] Implement error mapping for 403 Forbidden in DevinClient
- [ ] T031 [P] [US3] Implement error mapping for 404 Not Found in DevinClient
- [ ] T032 [P] [US3] Implement error mapping for 429 Rate Limit in DevinClient
- [ ] T033 [P] [US3] Implement error mapping for 500-504 Server Errors in DevinClient
- [ ] T034 [P] [US3] Implement error mapping for timeout (TaskCanceledException) in DevinClient
- [ ] T035 [P] [US3] Implement error mapping for network errors (HttpRequestException) in DevinClient
- [ ] T036 [US3] Update frontend to display error toasts for all error scenarios in TemplatesPage
- [ ] T037 [US3] Add retry suggestion to error messages where applicable in frontend
- [ ] T038 [US3] Log technical error details while showing user-friendly messages in TemplatesController

**Checkpoint**: All error scenarios handled gracefully with clear user feedback

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, documentation, and quality checks

- [ ] T039 [P] Verify all error scenarios in quickstart.md work as documented
- [ ] T040 [P] Verify Docker deployment with environment variable configuration
- [ ] T041 [P] Test rapid button clicks to ensure no race conditions
- [ ] T042 [P] Test with very large template payloads (edge case validation)
- [ ] T043 [P] Verify session URLs open correctly in new browser tabs
- [ ] T044 [P] Check that API key is properly masked in all log outputs
- [ ] T045 [P] Verify HTTPS enforcement for Devin API requests
- [ ] T046 [P] Update .gitignore to ensure appsettings.Development.json with secrets is not committed
- [ ] T047 Code review and cleanup: remove debug logging, unused imports
- [ ] T048 Final validation: run through all acceptance scenarios in spec.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User Story 1 → User Story 2 → User Story 3 (in priority order)
  - US2 and US3 could run in parallel after US1 if team capacity allows
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (Phase 2) - Core functionality
- **User Story 2 (P2)**: Depends on US1 - Requires DevinClient to be functional for testing
- **User Story 3 (P3)**: Depends on US1 - Requires error flows to be established

### Within Each User Story

#### User Story 1:
- Controller updates (T015-T017) must happen together (same flow)
- Frontend updates (T018-T021) can happen after controller is ready
- T018, T019, T020, T021 are all in same file, so sequential

#### User Story 2:
- T022, T023 are in DevinClient/Options (can be parallel but related)
- T024 documentation can happen anytime
- T025, T026 Docker changes are sequential
- T027 verification happens last

#### User Story 3:
- T028-T035 error mappings in DevinClient (all parallel - different error codes)
- T036, T037 frontend updates must follow backend error handling
- T038 logging updates can happen anytime

### Parallel Opportunities

- **Phase 1 Setup**: All 8 tasks (T001-T008) can run in parallel - different files
- **Phase 2 Foundational**: T009-T012 are in same file (sequential), T013-T014 are in same file (sequential), but the two groups could be parallel
- **User Story 2**: T022-T027 has tasks marked [P] that can run in parallel
- **User Story 3**: T028-T035 all error mappings can run in parallel (different error codes), T036-T038 sequential
- **Phase 6 Polish**: T039-T046 all marked [P] can run in parallel

---

## Parallel Example: Phase 1 Setup

```bash
# All Setup tasks can launch together (different files):
Task T001: "Create DevinApiOptions in ...DevinApiOptions.cs"
Task T002: "Create DevinApiModels in ...DevinApiModels.cs"
Task T003: "Create DevinApiException in ...DevinApiException.cs"
Task T004: "Create DevinSessionResponse in ...DevinSessionResponse.cs"
Task T005: "Create DevinErrorResponse in ...DevinErrorResponse.cs"
Task T006: "Update SendPromptResponse in ...SendPromptResponse.cs"
Task T007: "Update appsettings.json"
Task T008: "Update appsettings.Development.json"
```

---

## Parallel Example: User Story 3 Error Mappings

```bash
# All error mappings can launch together (different error codes):
Task T028: "Error mapping for 400 Bad Request"
Task T029: "Error mapping for 401 Unauthorized"
Task T030: "Error mapping for 403 Forbidden"
Task T031: "Error mapping for 404 Not Found"
Task T032: "Error mapping for 429 Rate Limit"
Task T033: "Error mapping for 500-504 Server Errors"
Task T034: "Error mapping for timeout"
Task T035: "Error mapping for network errors"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (8 tasks, ~30-45 min)
2. Complete Phase 2: Foundational (6 tasks, ~1-2 hours)
3. Complete Phase 3: User Story 1 (7 tasks, ~1-2 hours)
4. **STOP and VALIDATE**: Test end-to-end with real Devin API
5. If working, this is MVP - users can send templates and get links!

**Estimated MVP Time**: 3-5 hours total

### Incremental Delivery

1. **Setup + Foundational → Foundation ready** (14 tasks)
2. **Add User Story 1 → Test → Deploy** (MVP! Users can send to Devin)
3. **Add User Story 2 → Test → Deploy** (Better configuration experience)
4. **Add User Story 3 → Test → Deploy** (Better error handling)
5. **Polish → Final validation → Production ready**

### Single Developer Sequential Strategy

**Day 1**: Setup + Foundational + US1 (MVP)
**Day 2**: US2 + US3 (Error handling and configuration)
**Day 3**: Polish + Testing + Documentation

---

## Task Count Summary

- **Phase 1 (Setup)**: 8 tasks (all parallel)
- **Phase 2 (Foundational)**: 6 tasks
- **Phase 3 (User Story 1 - MVP)**: 7 tasks
- **Phase 4 (User Story 2)**: 6 tasks
- **Phase 5 (User Story 3)**: 11 tasks
- **Phase 6 (Polish)**: 10 tasks

**Total**: 48 tasks

**Parallel opportunities**: 24 tasks marked [P] (50% parallelizable)

---

## Notes

- All [P] tasks can run in parallel within their phase
- Each user story is independently testable after completion
- Stop at any checkpoint to validate story independently
- API key never appears in logs or client-side code (security requirement)
- Error messages are user-friendly, not technical (UX requirement)
- External API timeout set to 30s (performance requirement)
- No database changes required (pure addition feature)
- No breaking changes to existing API (backward compatible)

---

## Critical Success Factors

1. ✅ **DevinClient implementation** (Phase 2) must be solid - all stories depend on it
2. ✅ **Error handling** (US3) makes or breaks user experience
3. ✅ **Security** (US2) - API key must never leak to frontend or logs
4. ✅ **Testing** - Validate with real Devin API, not just mocks
5. ✅ **User Story 1** is the MVP - prioritize completing it first

---

## Risk Mitigation

- **Unknown Devin API behavior**: Documented in research.md Open Questions - will adjust during implementation
- **API rate limits**: US3 handles 429 errors gracefully
- **Network timeouts**: 30s timeout prevents hanging requests
- **Missing API key**: Clear error messages guide user to configuration
- **Security exposure**: API key server-side only, masked in logs, environment variable injection

---

**Ready for Implementation**: Run `/speckit.implement` to execute tasks
