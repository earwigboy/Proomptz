# Tasks: Template Usage Screen Enhancements

**Input**: Design documents from `/specs/004-template-usage-enhancements/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Manual testing only (no test framework configured per constitution decision)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- Frontend: `frontend/src/`
- No backend changes required (frontend-only feature)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create TypeScript type definitions and prepare component structure

- [x] T001 [P] Create TypeScript types for placeholders in frontend/src/lib/types/placeholders.ts
- [x] T002 [P] Create TypeScript types for template editor in frontend/src/lib/types/templateEditor.ts
- [x] T003 [P] Create TypeScript types for validation in frontend/src/lib/types/validation.ts
- [x] T004 [P] Create type exports barrel file in frontend/src/lib/types/index.ts
- [x] T005 Create template/ component directory in frontend/src/components/template/

**Checkpoint**: ✅ Type definitions ready - hook and component implementation can begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core hooks and utilities that MUST be complete before ANY user story UI can be implemented

**⚠️ CRITICAL**: No user story work can begin until these hooks are complete

- [x] T006 Modify usePlaceholders hook to add validation state in frontend/src/lib/hooks/usePlaceholders.ts
- [x] T007 [P] Create useTemplateEditor hook in frontend/src/lib/hooks/useTemplateEditor.ts
- [x] T008 [P] Create useFormValidation hook in frontend/src/lib/hooks/useFormValidation.ts

**Checkpoint**: ✅ All hooks ready - user story UI implementation can now begin in parallel

---

## Phase 3: User Story 1 - Enhanced Placeholder Input Experience (Priority: P1) 🎯 MVP

**Goal**: Improve placeholder form fields with appropriate input types (text vs textarea), clear labels, and validation feedback

**Independent Test**: Open template with placeholders, verify improved form fields render with text inputs for short values and textareas for long values, fill placeholders and verify validation feedback appears for empty required fields

### Implementation for User Story 1

- [x] T009 [P] [US1] Modify PlaceholderForm component to render Input vs Textarea based on field characteristics in frontend/src/components/placeholders/PlaceholderForm.tsx
- [x] T010 [P] [US1] Add clear labels with required indicators (*) for each placeholder field in frontend/src/components/placeholders/PlaceholderForm.tsx
- [x] T011 [US1] Add validation error display with role="alert" for each placeholder field in frontend/src/components/placeholders/PlaceholderForm.tsx
- [x] T012 [US1] Add ARIA attributes (aria-invalid, aria-describedby) to placeholder inputs in frontend/src/components/placeholders/PlaceholderForm.tsx
- [x] T013 [US1] Add placeholder text and helper text to guide user input in frontend/src/components/placeholders/PlaceholderForm.tsx
- [x] T014 [US1] Integrate modified PlaceholderForm with usePlaceholders hook in frontend/src/pages/TemplateUsage.tsx
- [x] T015 [US1] Add real-time preview updates when placeholder values change in frontend/src/pages/TemplateUsage.tsx

**Checkpoint**: ✅ User Story 1 complete - enhanced placeholder form should render with improved field types, labels, and validation feedback

---

## Phase 4: User Story 2 - Validation-Based Button Activation (Priority: P1) 🎯 MVP

**Goal**: Disable "Send to Devin" button until all required placeholders are filled, with clear feedback explaining why button is disabled

**Independent Test**: Open template with unfilled placeholders, verify "Send to Devin" button is disabled, fill placeholders one by one and verify button only enables when all are filled, clear a placeholder and verify button disables again

### Implementation for User Story 2

- [x] T016 [US2] Integrate useFormValidation hook in TemplateUsage page to compute canSubmit state in frontend/src/pages/TemplateUsage.tsx
- [x] T017 [US2] Update "Send to Devin" button to use canSubmit for disabled prop in frontend/src/pages/TemplateUsage.tsx
- [x] T018 [US2] Add Tooltip component wrapper around "Send to Devin" button in frontend/src/pages/TemplateUsage.tsx
- [x] T019 [US2] Set tooltip content to show validationMessage when button disabled in frontend/src/pages/TemplateUsage.tsx
- [x] T020 [US2] Add aria-describedby to button linking to validation status message in frontend/src/pages/TemplateUsage.tsx
- [x] T021 [US2] Add sr-only validation status message with aria-live="polite" for screen readers in frontend/src/pages/TemplateUsage.tsx
- [x] T022 [US2] Update button visual state (disabled styling) from shadcn/ui Button component in frontend/src/pages/TemplateUsage.tsx

**Checkpoint**: ✅ User Story 2 complete - "Send to Devin" button should be disabled until validation passes, with clear feedback via tooltip and screen reader announcement

---

## Phase 5: User Story 3 - Template Content Editing Before Sending (Priority: P2)

**Goal**: Allow users to edit the fully-resolved template content after filling placeholders, with ability to preview edits and reset to original

**Independent Test**: Fill placeholder values, switch to Edit tab, modify template content, verify changes are preserved in preview, click Reset button and verify content reverts to original resolved template

### Implementation for User Story 3

- [x] T023 [P] [US3] Create TemplateEditor component with textarea for content editing in frontend/src/components/template/TemplateEditor.tsx
- [x] T024 [P] [US3] Add "Reset to Original" button that only shows when hasEdits is true in frontend/src/components/template/TemplateEditor.tsx
- [x] T025 [P] [US3] Add monospace font styling and responsive min-height to textarea in frontend/src/components/template/TemplateEditor.tsx
- [x] T026 [P] [US3] Add aria-label to textarea for accessibility in frontend/src/components/template/TemplateEditor.tsx
- [x] T027 [US3] Add Tabs component (Preview/Edit) to TemplateUsage page in frontend/src/pages/TemplateUsage.tsx
- [x] T028 [US3] Add TabsList with Preview and Edit triggers in frontend/src/pages/TemplateUsage.tsx
- [x] T029 [US3] Add TabsContent for Preview tab containing PromptPreview component in frontend/src/pages/TemplateUsage.tsx
- [x] T030 [US3] Add TabsContent for Edit tab containing TemplateEditor component in frontend/src/pages/TemplateUsage.tsx
- [x] T031 [US3] Add "Edited" badge to Edit tab trigger when hasEdits is true in frontend/src/pages/TemplateUsage.tsx
- [x] T032 [US3] Integrate useTemplateEditor hook in TemplateUsage page in frontend/src/pages/TemplateUsage.tsx
- [x] T033 [US3] Pass finalContent to both PromptPreview and TemplateEditor components in frontend/src/pages/TemplateUsage.tsx
- [x] T034 [US3] Wire setEditedContent callback to TemplateEditor onChange in frontend/src/pages/TemplateUsage.tsx
- [x] T035 [US3] Wire resetEdits callback to TemplateEditor onReset in frontend/src/pages/TemplateUsage.tsx
- [x] T036 [US3] Update "Send to Devin" action to use getContentToSend() from useTemplateEditor hook in frontend/src/pages/TemplateUsage.tsx

**Checkpoint**: ✅ User Story 3 complete - users should be able to switch between Preview and Edit tabs, modify template content, and reset to original

---

## Phase 6: User Story 4 - Larger Preview for Better Template Visibility (Priority: P3)

**Goal**: Increase preview box size to show significantly more template content at once, with responsive design for different screen sizes

**Independent Test**: Open template with long content (20+ lines), verify preview box displays at least 50% more visible lines than before (target 25-30+ lines on desktop), test on mobile/tablet/desktop viewports to ensure responsive sizing

### Implementation for User Story 4

- [x] T037 [P] [US4] Modify PromptPreview component to use flex-1 for flexible height in frontend/src/components/placeholders/PromptPreview.tsx
- [x] T038 [P] [US4] Add responsive min-height classes (min-h-[300px] md:min-h-[400px] lg:min-h-[500px]) in frontend/src/components/placeholders/PromptPreview.tsx
- [x] T039 [P] [US4] Add overflow-y-auto for scrolling within preview box in frontend/src/components/placeholders/PromptPreview.tsx
- [x] T040 [P] [US4] Add "Edited" badge display when hasEdits prop is true in frontend/src/components/placeholders/PromptPreview.tsx
- [x] T041 [US4] Update TemplateUsage page layout to use flexbox column with h-full in frontend/src/pages/TemplateUsage.tsx
- [x] T042 [US4] Set PlaceholderForm section to flex-shrink-0 (fixed height) in frontend/src/pages/TemplateUsage.tsx
- [x] T043 [US4] Set Tabs section to flex-1 (takes remaining space) with flex flex-col in frontend/src/pages/TemplateUsage.tsx
- [x] T044 [US4] Set Send button section to flex-shrink-0 (fixed height) in frontend/src/pages/TemplateUsage.tsx
- [x] T045 [US4] Pass className="flex-1" to PromptPreview component in frontend/src/pages/TemplateUsage.tsx

**Checkpoint**: ✅ User Story 4 complete - preview box should be significantly larger, showing 50%+ more content with responsive sizing across mobile/tablet/desktop

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements and validation across all user stories

- [ ] T046 [P] Test placeholder form with templates containing 0 placeholders (edge case validation)
- [ ] T047 [P] Test placeholder form with templates containing 10+ placeholders (performance validation)
- [ ] T048 [P] Test editing workflow when user changes placeholder values after editing (edge case validation)
- [ ] T049 [P] Test button state when user clears all content in Edit tab (edge case validation)
- [ ] T050 [P] Verify keyboard navigation through entire form (tab order, Enter to submit)
- [ ] T051 [P] Test with screen reader (NVDA/VoiceOver) to verify ARIA announcements
- [ ] T052 Test responsive layout on 320px (mobile), 768px (tablet), 1920px (desktop) viewports
- [ ] T053 Verify color contrast for error messages meets WCAG 2.1 AA (4.5:1 minimum)
- [ ] T054 Run npm run build and verify bundle size increase is minimal (<10KB)
- [ ] T055 Run npm run lint and fix any ESLint errors
- [ ] T056 Run npx tsc --noEmit and fix any TypeScript errors
- [ ] T057 Validate all acceptance scenarios from spec.md User Stories 1-4
- [ ] T058 Test complete workflow from quickstart.md Scenario 5 (end-to-end validation)

**Checkpoint**: All user stories tested and validated - feature ready for PR

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) completion - Can proceed independently
- **User Story 2 (Phase 4)**: Depends on Foundational (Phase 2) and User Story 1 (Phase 3) completion - Builds on US1 form
- **User Story 3 (Phase 5)**: Depends on Foundational (Phase 2) completion - Can proceed independently (but integrates with US1/US2)
- **User Story 4 (Phase 6)**: Depends on User Story 3 (Phase 5) completion - Modifies same components as US3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after User Story 1 (Phase 3) - Requires placeholder form to exist
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Independently testable but integrates with US1/US2 components
- **User Story 4 (P3)**: Can start after User Story 3 (Phase 5) - Modifies PromptPreview component created/modified in US3

### Within Each User Story

- Type definitions before hooks (Phase 1 → Phase 2)
- Hooks before components (Phase 2 → Phase 3+)
- Components before page integration (component files → TemplateUsage.tsx)
- Core implementation before edge case testing (implementation tasks → polish tasks)

### Parallel Opportunities

**Phase 1 (Setup)**: All 5 tasks can run in parallel (different files)
- T001, T002, T003, T004, T005

**Phase 2 (Foundational)**: T007 and T008 can run in parallel (different files)
- T007 (useTemplateEditor.ts)
- T008 (useFormValidation.ts)
- Note: T006 modifies existing file, should complete first

**User Story 1 (Phase 3)**: T009 and T010 can start together (same file, sequential edits)

**User Story 2 (Phase 4)**: Most tasks sequential (same file: TemplateUsage.tsx)

**User Story 3 (Phase 5)**: Component creation can run in parallel
- T023, T024, T025, T026 (TemplateEditor.tsx - sequential edits to same file)
- T027-T036 (TemplateUsage.tsx - sequential edits to same file)
- TemplateEditor.tsx tasks can run in parallel with TemplateUsage.tsx tasks

**User Story 4 (Phase 6)**: Component modification can run in parallel
- T037, T038, T039, T040 (PromptPreview.tsx)
- T041, T042, T043, T044, T045 (TemplateUsage.tsx)
- Different component files can be modified in parallel

**Phase 7 (Polish)**: Most edge case tests can run in parallel
- T046, T047, T048, T049, T050, T051 (different test scenarios)

---

## Parallel Example: Phase 1 (Setup)

All type definition files can be created simultaneously:

```bash
Task T001: "Create placeholders.ts types"
Task T002: "Create templateEditor.ts types"
Task T003: "Create validation.ts types"
Task T004: "Create index.ts barrel exports"
Task T005: "Create template/ directory"
```

---

## Parallel Example: User Story 3 (Implementation)

Component creation can proceed in parallel with page integration:

```bash
# Team Member A - TemplateEditor component:
Task T023: "Create TemplateEditor component"
Task T024: "Add Reset button"
Task T025: "Add styling"
Task T026: "Add ARIA attributes"

# Team Member B - TemplateUsage page integration (can start in parallel):
Task T027: "Add Tabs component"
Task T028: "Add TabsList"
Task T029: "Add Preview TabsContent"
Task T030: "Add Edit TabsContent"
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 Only)

1. Complete Phase 1: Setup (T001-T005) - ~30min
2. Complete Phase 2: Foundational (T006-T008) - ~1-2 hours (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (T009-T015) - ~2-3 hours
4. Complete Phase 4: User Story 2 (T016-T022) - ~1-2 hours
5. **STOP and VALIDATE**: Test User Stories 1 & 2 independently
6. **MVP DELIVERED**: Enhanced placeholder form + validation-based button

**Total MVP Effort**: ~5-8 hours for core value delivery

### Incremental Delivery

1. **Iteration 1**: Setup + Foundational → Foundation ready (~2 hours)
2. **Iteration 2**: Add User Story 1 → Test independently → **Demo enhanced forms** (~2-3 hours)
3. **Iteration 3**: Add User Story 2 → Test independently → **MVP delivery** (~1-2 hours)
4. **Iteration 4**: Add User Story 3 → Test independently → **Demo template editing** (~3-4 hours)
5. **Iteration 5**: Add User Story 4 → Test independently → **Demo larger preview** (~1-2 hours)
6. **Iteration 6**: Polish & edge cases → **Production ready** (~2-3 hours)

Each iteration adds value without breaking previous stories.

### Sequential (Single Developer) Strategy

**Day 1**: Setup + Foundational + User Story 1 + User Story 2 (MVP)
- Complete T001-T022
- Validate placeholder forms and button state
- ~6-8 hours

**Day 2**: User Story 3 (Template Editing)
- Complete T023-T036
- Validate edit workflow
- ~4-5 hours

**Day 3**: User Story 4 (Larger Preview) + Polish
- Complete T037-T058
- Full feature validation
- ~4-5 hours

**Total Estimated Effort**: 14-18 hours over 3 days

### Parallel Team Strategy (2 developers)

**Both developers**: Complete Setup + Foundational together (T001-T008) - ~2 hours

**Then split**:
- **Developer A**: User Story 1 + User Story 2 (T009-T022) - ~4-5 hours
- **Developer B**: User Story 3 setup (T023-T026 - TemplateEditor component) - ~2 hours

**Rejoin for integration**:
- **Developer A**: Integrate US3 into TemplateUsage (T027-T036) - ~2-3 hours
- **Developer B**: User Story 4 (T037-T045) - ~2-3 hours

**Both developers**: Polish + validation (T046-T058) - ~2-3 hours

**Total Time**: ~10-12 hours (vs 14-18 hours solo) with 2 developers

---

## Notes

- [P] tasks = different files or independently testable, no immediate dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- No test framework tasks (manual testing per constitution decision)
- Commit after each task or logical group of related tasks
- Stop at any checkpoint to validate story independently
- All paths are relative to repository root `/home/earwigboy/src/repos/Proomptz/`
- Frontend-only feature: no backend changes, no API modifications, no database migrations

---

## Task Count Summary

**Total Tasks**: 58

**By Phase**:
- Phase 1 (Setup): 5 tasks
- Phase 2 (Foundational): 3 tasks
- Phase 3 (User Story 1 - P1): 7 tasks
- Phase 4 (User Story 2 - P1): 7 tasks
- Phase 5 (User Story 3 - P2): 14 tasks
- Phase 6 (User Story 4 - P3): 9 tasks
- Phase 7 (Polish): 13 tasks

**By Priority**:
- P1 (MVP): 14 tasks (US1 + US2)
- P2: 14 tasks (US3)
- P3: 9 tasks (US4)
- Setup/Foundational: 8 tasks
- Polish: 13 tasks

**Parallel Opportunities**: 18 tasks marked [P] can run in parallel with other tasks

**MVP Scope** (Recommended first iteration):
- Phase 1: Setup (5 tasks)
- Phase 2: Foundational (3 tasks)
- Phase 3: User Story 1 (7 tasks)
- Phase 4: User Story 2 (7 tasks)
- **Total MVP**: 22 tasks (~6-8 hours)
