# Tasks: UI Enhancement with Shadcn Blocks and Blue Theme

**Feature**: 003-shadcn-blocks-ui
**Input**: Design documents from `/specs/003-shadcn-blocks-ui/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md

**Tests**: Not requested in specification - manual visual testing and accessibility validation required

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions
- Web app structure: `frontend/src/`, `frontend/components/`
- Config files: `frontend/tailwind.config.js`, `frontend/components.json`
- Styles: `frontend/src/index.css`, `frontend/src/App.css`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install shadcn Sidebar component and verify prerequisites

- [x] T001 Verify shadcn/ui is installed by checking frontend/components.json exists
- [x] T002 Install shadcn Sidebar component via `npx shadcn@latest add sidebar` in frontend/
- [x] T003 [P] Install separator component if not present via `npx shadcn@latest add separator` in frontend/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Apply blue theme CSS variables that ALL user stories depend on

**⚠️ CRITICAL**: Blue theme must be applied before testing any UI changes to ensure visual consistency

- [x] T004 Update CSS variables for blue theme in frontend/src/index.css per research.md Decision 2
- [x] T005 Verify Tailwind color configuration in frontend/tailwind.config.js matches CSS variables
- [x] T006 Remove old custom theme CSS from frontend/src/App.css (header, sidebar styles)

**Checkpoint**: Foundation ready - blue theme applied, sidebar component installed

---

## Phase 3: User Story 1 - Professional Sidebar Navigation (Priority: P1) 🎯 MVP

**Goal**: Replace current folder navigation panel with shadcn Sidebar block that supports collapse/expand and responsive behavior

**Independent Test**: Open application, verify sidebar displays folder tree, test collapse/expand toggle, test mobile responsiveness (<768px), verify all folder operations work (create, rename, delete, drag-drop)

### Implementation for User Story 1

- [x] T007 [US1] Create AppSidebar component wrapping Sidebar in frontend/src/App.tsx
- [x] T008 [US1] Wrap app with SidebarProvider in frontend/src/App.tsx
- [x] T009 [US1] Add SidebarInset wrapper for main content area in frontend/src/App.tsx
- [x] T010 [US1] Create sticky header with SidebarTrigger and Separator in frontend/src/App.tsx
- [x] T011 [US1] Move FolderTree component into AppSidebar SidebarContent in frontend/src/App.tsx
- [x] T012 [US1] Update main content area with responsive padding classes (p-4 md:p-6 lg:p-8) in frontend/src/App.tsx
- [x] T013 [US1] Remove old sidebar CSS classes from frontend/src/App.css

**Checkpoint**: Sidebar fully functional with collapse/expand, folder operations working, mobile responsive

---

## Phase 4: User Story 2 - Cohesive Blue Theme Application (Priority: P2)

**Goal**: Ensure all UI components use the blue theme color palette consistently across the application

**Independent Test**: Navigate through all pages (home, search, template usage), verify buttons, cards, inputs, dialogs all use blue theme, run WCAG contrast checks (4.5:1 minimum), test hover/focus/active states

### Implementation for User Story 2

- [x] T014 [P] [US2] Review and verify Button components use blue theme in frontend/src/components/ui/button.tsx
- [x] T015 [P] [US2] Review and verify Card components use blue theme in frontend/src/components/ui/card.tsx
- [x] T016 [P] [US2] Review and verify Dialog components use blue theme in frontend/src/components/ui/dialog.tsx
- [x] T017 [P] [US2] Review and verify Input components use blue theme in frontend/src/components/ui/input.tsx
- [x] T018 [P] [US2] Review and verify Badge components use blue theme in frontend/src/components/ui/badge.tsx
- [x] T019 [P] [US2] Review TemplateList component for blue theme compatibility in frontend/src/components/TemplateList.tsx
- [x] T020 [P] [US2] Review TemplateForm component for blue theme compatibility in frontend/src/components/TemplateForm.tsx
- [x] T021 [P] [US2] Review SearchBar component for blue theme compatibility in frontend/src/components/search/SearchBar.tsx
- [x] T022 [P] [US2] Review FolderTree component for blue theme compatibility in frontend/src/components/folders/FolderTree.tsx
- [x] T023 [US2] Run WCAG contrast validation on all text/background combinations using browser dev tools
- [x] T024 [US2] Adjust CSS variable lightness values if contrast ratios fail (<4.5:1) in frontend/src/index.css

**Checkpoint**: All UI elements use cohesive blue theme, WCAG AA contrast requirements met

---

## Phase 5: User Story 3 - Enhanced Layout with Shadcn Blocks (Priority: P3)

**Goal**: Apply shadcn block patterns to header, main content area, and forms for consistent spacing and visual hierarchy

**Independent Test**: Create/edit templates, search, navigate app to verify all sections use shadcn block patterns with consistent spacing, test responsive behavior at 375px, 768px, 1024px, 1920px

### Implementation for User Story 3

- [x] T025 [US3] Verify header uses block pattern (h-16, sticky, flex items-center, gap-2, border-b) in frontend/src/App.tsx
- [x] T026 [US3] Verify main content uses progressive padding (p-4 md:p-6 lg:p-8) in frontend/src/App.tsx
- [x] T027 [P] [US3] Verify TemplateList grid uses responsive columns (grid-cols-1 md:grid-cols-2 lg:grid-cols-3) in frontend/src/components/TemplateList.tsx
- [x] T028 [P] [US3] Review TemplateForm layout for shadcn block patterns (space-y-4 for fields) in frontend/src/components/TemplateForm.tsx
- [x] T029 [P] [US3] Review SearchBar layout for shadcn block patterns in frontend/src/components/search/SearchBar.tsx
- [x] T030 [US3] Test responsive layout at 375px, 768px, 1024px, 1920px viewports

**Checkpoint**: All layouts use shadcn block patterns, responsive behavior graceful at all breakpoints

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Quality checks, accessibility validation, and performance verification

- [x] T031 [P] Run TypeScript compilation check via `npx tsc -b` in frontend/
- [x] T032 [P] Build for production via `npm run build` in frontend/
- [x] T033 Test sidebar collapse/expand functionality on desktop (≥768px)
- [x] T034 Test sidebar sheet overlay functionality on mobile (<768px)
- [x] T035 Test sidebar state persistence (reload page, check cookie)
- [x] T036 Test all folder operations (create, rename, delete, drag-drop templates)
- [x] T037 [P] Test keyboard navigation (Tab, Enter, Space, Esc)
- [x] T038 [P] Verify accessibility with WAVE or axe DevTools browser extension
- [x] T039 Measure page load time increase (must be <100ms per SC-006)
- [x] T040 Verify bundle size increase is reasonable (<50KB per plan.md)
- [x] T041 Visual QA across all pages (home, search, template usage, folder dialogs)
- [x] T042 Update agent context with sidebar patterns via `.specify/scripts/bash/update-agent-context.sh claude`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - US1 (Sidebar) can proceed first (P1 priority)
  - US2 (Blue Theme) can proceed after US1 (visual validation easier with new layout)
  - US3 (Layout) can proceed after US1 (depends on sidebar structure)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1 - Sidebar)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2 - Blue Theme)**: Can start after Foundational, but testing is easier after US1 complete
- **User Story 3 (P3 - Layout)**: Can start after US1 (sidebar structure must exist)

### Within Each User Story

**User Story 1 (Sidebar)**:
1. T007-T010 can run sequentially (all modify App.tsx)
2. T011-T012 depend on T007-T010 completing
3. T013 can run in parallel with T011-T012 (different file)

**User Story 2 (Blue Theme)**:
- T014-T022 can ALL run in parallel (different files)
- T023-T024 depend on T014-T022 completing

**User Story 3 (Layout)**:
- T025-T029 can ALL run in parallel (different files, review tasks)
- T030 depends on T025-T029 completing

### Parallel Opportunities

**Setup Phase**:
- T002 and T003 can run in parallel (different components)

**Foundational Phase**:
- T004, T005, T006 should run sequentially (T005 verifies T004, T006 cleans up)

**User Story 2 - All component reviews**:
```bash
# Launch all component reviews together:
Task: "Review Button components (frontend/src/components/ui/button.tsx)"
Task: "Review Card components (frontend/src/components/ui/card.tsx)"
Task: "Review Dialog components (frontend/src/components/ui/dialog.tsx)"
Task: "Review Input components (frontend/src/components/ui/input.tsx)"
Task: "Review Badge components (frontend/src/components/ui/badge.tsx)"
Task: "Review TemplateList (frontend/src/components/TemplateList.tsx)"
Task: "Review TemplateForm (frontend/src/components/TemplateForm.tsx)"
Task: "Review SearchBar (frontend/src/components/search/SearchBar.tsx)"
Task: "Review FolderTree (frontend/src/components/folders/FolderTree.tsx)"
```

**User Story 3 - All layout verifications**:
```bash
# Launch all layout reviews together:
Task: "Verify TemplateList grid (frontend/src/components/TemplateList.tsx)"
Task: "Review TemplateForm layout (frontend/src/components/TemplateForm.tsx)"
Task: "Review SearchBar layout (frontend/src/components/search/SearchBar.tsx)"
```

**Polish Phase**:
- T031, T032, T037, T038 can ALL run in parallel (independent checks)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (install sidebar component)
2. Complete Phase 2: Foundational (apply blue theme variables)
3. Complete Phase 3: User Story 1 (sidebar implementation)
4. **STOP and VALIDATE**: Test sidebar independently
   - Collapse/expand works
   - Mobile responsive works
   - Folder operations work
   - Visual appearance acceptable
5. If ready, deploy/demo sidebar functionality

### Incremental Delivery

1. Complete Setup + Foundational → Blue theme foundation ready
2. Add User Story 1 (Sidebar) → Test independently → Deploy/Demo (MVP with professional sidebar!)
3. Add User Story 2 (Blue Theme Review) → Test independently → Deploy/Demo (cohesive theme applied)
4. Add User Story 3 (Layout Enhancements) → Test independently → Deploy/Demo (polished layouts)
5. Complete Polish phase → Final QA → Production ready

### Sequential Implementation (Recommended)

Since this is a UI-only feature affecting the same application:

1. Complete Setup + Foundational (required foundation)
2. Complete User Story 1 (Sidebar) - PRIORITY 1
   - Test thoroughly before moving on
   - Ensure all folder functionality works
3. Complete User Story 2 (Blue Theme) - PRIORITY 2
   - Review all components with new sidebar in place
   - Easier to validate visual consistency
4. Complete User Story 3 (Layout) - PRIORITY 3
   - Final polish on spacing and alignment
5. Complete Polish phase - Final quality checks

### Parallel Team Strategy

If multiple developers are available:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Sidebar - highest priority)
   - Developer B: User Story 2 (Blue Theme - can review in isolation)
   - Note: User Story 3 should wait until US1 completes (depends on sidebar structure)
3. Final Polish phase done together

---

## Testing Checkpoints

### After User Story 1 (Sidebar)
- [ ] Sidebar displays folder tree correctly
- [ ] Collapse/expand toggle works smoothly
- [ ] Mobile responsive (sheet overlay <768px, fixed sidebar ≥768px)
- [ ] All folder operations work (create, rename, delete, drag-drop)
- [ ] Keyboard navigation preserved
- [ ] State persists on page reload (sidebar_state cookie)

### After User Story 2 (Blue Theme)
- [ ] All buttons use blue theme palette
- [ ] All cards use blue backgrounds and borders
- [ ] All inputs use blue border colors
- [ ] All dialogs use blue theme
- [ ] Hover/focus/active states use blue variants
- [ ] WCAG AA contrast ratios met (4.5:1 minimum for text)
- [ ] Visual consistency across all pages

### After User Story 3 (Layout)
- [ ] Header uses block pattern (64px height, sticky, proper spacing)
- [ ] Content area uses progressive padding (16px → 24px → 32px)
- [ ] Template grid responsive (1 col → 2 col → 3 col)
- [ ] Forms use consistent spacing (space-y-4 for fields, gap-2 for buttons)
- [ ] Responsive behavior graceful at all breakpoints

### Final Quality Gates
- [ ] TypeScript compiles with zero errors
- [ ] Production build succeeds
- [ ] Bundle size increase < 50KB
- [ ] Page load time increase < 100ms
- [ ] No functional regressions
- [ ] Accessibility maintained (WCAG AA)

---

## Notes

- **[P]** tasks = different files, no dependencies, can run in parallel
- **[Story]** label maps task to specific user story for traceability
- Each user story should be independently testable
- No automated tests requested - rely on manual visual testing and accessibility validation
- Commit after each user story phase for easy rollback
- Stop at any checkpoint to validate story independently
- Reference quickstart.md for detailed implementation steps
- Reference research.md for design decisions and rationale

## Success Criteria Validation

Per spec.md, verify these outcomes:

- **SC-001**: Folder navigation speed unchanged (no functional regressions)
- **SC-002**: Visual consistency improved (cohesive blue theme across all UI)
- **SC-003**: Accessibility maintained (WCAG AA contrast requirements met)
- **SC-004**: UI feels more professional (qualitative improvement)
- **SC-005**: Sidebar adapts to mobile screens (<768px)
- **SC-006**: Page load time increase <100ms
- **SC-007**: All existing features work without bugs

## Risk Mitigation

Per spec.md and plan.md:

1. **Sidebar accommodation**: Follow research.md patterns, use official shadcn Sidebar
2. **Blue theme contrast**: Run WCAG checks, adjust variables if needed (T023-T024)
3. **User resistance**: Preserve all functionality, no breaking changes
4. **Mobile responsive**: Test early at all breakpoints (T030, T034)
5. **CSS conflicts**: Remove old custom CSS (T006, T013)
