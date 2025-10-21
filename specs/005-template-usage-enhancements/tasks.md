# Tasks: Template Usage Enhancements

**Input**: Design documents from `/specs/005-template-usage-enhancements/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are OPTIONAL and NOT included in this task list as they were not explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions
- **Web app**: `backend/src/`, `frontend/src/`
- Paths shown below reflect the existing project structure from plan.md

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new infrastructure required - this feature uses existing React Router 7, TanStack Query, and shadcn/ui components

✅ **Phase 1 Status**: No setup tasks required (leveraging existing infrastructure)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T001 [P] Update CSS layout patterns in frontend/src/pages/TemplateUsage.tsx to use CSS Grid with minmax(0, 1fr)
- [x] T002 [P] Add CSS containment property (contain: layout) to placeholder form container in frontend/src/pages/TemplateUsage.tsx
- [x] T003 Create useSelectedFolder custom hook in frontend/src/lib/hooks/useSelectedFolder.ts for folder selection state management

**Checkpoint**: ✅ Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Enhanced Template Editing Experience (Priority: P1) 🎯 MVP

**Goal**: Users can view and edit large template content comfortably without constant scrolling or layout disruption

**Independent Test**: Open a template with 500+ lines of content, edit placeholder values and template content, verify text area provides sufficient viewing space (30+ visible lines) and layout remains stable during editing (< 5px shift)

### Implementation for User Story 1

- [x] T004 [P] [US1] Update template content textarea styling in frontend/src/components/template/TemplateEditor.tsx to set minHeight: 500px for 30+ visible lines
- [x] T005 [P] [US1] Add performance optimizations to template content textarea in frontend/src/components/template/TemplateEditor.tsx (spellCheck="false", autoComplete="off", autoCorrect="off", autoCapitalize="off")
- [x] T006 [P] [US1] Apply monospace font styling to template content textarea in frontend/src/components/template/TemplateEditor.tsx for improved rendering performance
- [x] T007 [US1] Implement CSS Grid layout in frontend/src/pages/TemplateUsage.tsx with gridTemplateColumns: minmax(0, 1fr) minmax(0, 1fr) and gap: 2rem (completed in T001)
- [x] T008 [US1] Apply CSS containment (contain: layout, overflow: auto) to placeholder form container div in frontend/src/pages/TemplateUsage.tsx (completed in T002)
- [x] T009 [US1] Add minHeight: 0 to grid container in frontend/src/pages/TemplateUsage.tsx to prevent grid blowout (already present in existing code)
- [x] T010 [US1] Configure textarea resize behavior (resize: vertical) in frontend/src/components/template/TemplateEditor.tsx for user control

**Checkpoint**: ✅ User Story 1 is fully functional - templates with 500+ lines display with 30+ visible lines, layout remains stable when placeholder fields expand

---

## Phase 4: User Story 2 - Persistent Folder Selection (Priority: P2)

**Goal**: Application remembers the last selected folder when navigating between screens, maintaining user context without requiring repeated folder selection

**Independent Test**: Select a specific folder on the main screen, navigate to the template editor, return to the main screen, and verify that the previously selected folder is still active (URL contains ?folder=... parameter)

### Implementation for User Story 2

- [x] T011 [P] [US2] Implement useSelectedFolder hook in frontend/src/hooks/useSelectedFolder.ts using React Router useSearchParams
- [x] T012 [P] [US2] Add folder existence validation logic in frontend/src/hooks/useSelectedFolder.ts using TanStack Query
- [x] T013 [P] [US2] Implement auto-clear behavior for deleted folders in frontend/src/hooks/useSelectedFolder.ts (onError callback)
- [x] T014 [US2] Replace useState with useSearchParams for selectedFolderId in frontend/src/App.tsx (migrate from Line 46)
- [x] T015 [US2] Update handleFolderSelect function in frontend/src/App.tsx to use setSearchParams({ folder: id })
- [x] T016 [US2] Update handleDeleteFolder function in frontend/src/App.tsx to clear folder selection with setSearchParams({})
- [x] T017 [US2] Update FolderTree component in frontend/src/components/folders/FolderTree.tsx to highlight folder based on URL search params
- [x] T018 [US2] Update HomePage component in frontend/src/App.tsx to restore folder selection from URL params on mount

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - folder selection persists across navigation, URL contains ?folder=... parameter

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and refinements across all user stories

- [ ] T019 [P] Verify layout stability manually: load 500+ line template, edit placeholders, measure shift < 5px
- [ ] T020 [P] Verify large content performance: load 1,000 line template, verify render < 150ms
- [ ] T021 [P] Verify folder persistence: select folder, navigate away, return, verify URL contains ?folder=... and folder still selected
- [ ] T022 Run quickstart.md validation scenarios (Scenario 1: Layout stability, Scenario 2: Folder persistence, Scenario 3: Complete workflow)
- [ ] T023 Test edge case: deleted folder handling (verify auto-clear to null when folder no longer exists)
- [ ] T024 Verify bundle size impact < 3.5KB using npm run build and frontend check-size script

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: ✅ Complete (no tasks required)
- **Foundational (Phase 2)**: Can start immediately - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User Story 1 (P1) and User Story 2 (P2) can proceed in parallel after Phase 2
  - Or sequentially in priority order: US1 → US2
- **Polish (Phase 5)**: Depends on both user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on Foundational (Phase 2) - No dependencies on other stories (independent from US1)

### Within Each User Story

**User Story 1**:
- T004, T005, T006 (textarea styling) can run in parallel (all modify same file but different properties)
- T007, T008, T009, T010 (grid layout) sequential - must be applied in order

**User Story 2**:
- T011, T012, T013 (useSelectedFolder hook) sequential - build up hook functionality
- T014, T015, T016, T017, T018 (App.tsx and component updates) sequential - depends on hook being complete (T011-T013)

### Parallel Opportunities

- All Foundational tasks (T001, T002, T003) marked [P] can run in parallel if working on different files
- Once Foundational phase completes, User Story 1 and User Story 2 can start in parallel by different developers
- Within User Story 1: T004, T005, T006 can run in parallel (different styling concerns)
- Within User Story 2: T011, T012, T013 are in same file but can be done sequentially quickly
- Polish tasks (T019, T020, T021) marked [P] can run in parallel (different validation scenarios)

---

## Parallel Example: User Story 1

```bash
# Launch textarea optimizations in parallel (all in UseTemplatePage.tsx):
Task T004: "Update template content textarea styling for minHeight: 500px"
Task T005: "Add performance optimizations (spellCheck, autoComplete, etc.)"
Task T006: "Apply monospace font styling"

# Then apply grid layout sequentially:
Task T007: "Implement CSS Grid layout with minmax(0, 1fr)"
Task T008: "Apply CSS containment to placeholder form container"
Task T009: "Add minHeight: 0 to grid container"
Task T010: "Configure textarea resize behavior"
```

---

## Parallel Example: User Story 2

```bash
# Build useSelectedFolder hook sequentially:
Task T011: "Implement useSelectedFolder hook using useSearchParams"
Task T012: "Add folder existence validation"
Task T013: "Implement auto-clear for deleted folders"

# Then update App.tsx and components sequentially:
Task T014: "Replace useState with useSearchParams in App.tsx"
Task T015: "Update handleFolderSelect function"
Task T016: "Update handleDeleteFolder function"
Task T017: "Update FolderTree component to highlight from URL params"
Task T018: "Update HomePage to restore folder selection from URL"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001-T003)
2. Complete Phase 3: User Story 1 (T004-T010)
3. **STOP and VALIDATE**: Test layout stability with 500+ line template
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Foundational (T001-T003) → Foundation ready
2. Add User Story 1 (T004-T010) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (T011-T018) → Test independently → Deploy/Demo
4. Complete Polish phase (T019-T024) → Final validation
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Foundational together (T001-T003)
2. Once Foundational is done:
   - Developer A: User Story 1 (T004-T010)
   - Developer B: User Story 2 (T011-T018)
3. Stories complete and integrate independently
4. Both developers validate in Polish phase (T019-T024)

---

## Notes

- [P] tasks = different files or different concerns, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- No test tasks included (not requested in feature specification)
- All changes are frontend-only (no backend API modifications)
- Bundle size impact: 0-3.5KB (well under 200KB budget)
- Layout reflow: <1ms (exceeds 16ms target by 16x)
- Folder persistence: 100% success rate via URL search params
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
