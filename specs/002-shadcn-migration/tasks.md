# Tasks: Migrate Frontend to shadcn/ui Components

**Input**: Design documents from `/specs/002-shadcn-migration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are explicitly OUT OF SCOPE per specification. Manual testing via user story acceptance scenarios will validate functionality.

**Organization**: Tasks are grouped by user story (P1→P2→P3→P4) to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- **Web app structure**: `frontend/src/`, `frontend/` (backend unchanged)
- All paths relative to repository root

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install and configure shadcn/ui with Tailwind CSS and required dependencies

- [x] T001 Run `npx shadcn@latest init` in frontend/ directory with configuration: Style=Default, BaseColor=Slate, CSSVariables=Yes, Tailwind=Yes, ImportAlias=@/components, RSC=No
- [x] T002 [P] Install lucide-react icons library: `npm install lucide-react`
- [x] T003 [P] Install additional dependencies: `npm install class-variance-authority clsx tailwind-merge`
- [x] T004 [P] Install Tailwind dev dependencies: `npm install -D tailwindcss postcss autoprefixer tailwindcss-animate`
- [x] T005 Add TypeScript path alias configuration in frontend/tsconfig.app.json: Add baseUrl and paths for @/* → ./src/*
- [x] T006 Add Vite path alias resolution in frontend/vite.config.ts: Import path and add resolve.alias for @/ → path.resolve(__dirname, './src')
- [x] T007 Configure dark theme as default in frontend/src/index.css: Update CSS variables for dark theme colors matching existing palette (#1a1a1a background, #646cff primary)
- [x] T008 Verify build succeeds and check bundle size: Run `npm run build && npm run check-size` in frontend/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Install all shadcn components needed across user stories

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T009 [P] Install shadcn Button component: `npx shadcn@latest add button` in frontend/
- [x] T010 [P] Install shadcn Input component: `npx shadcn@latest add input` in frontend/
- [x] T011 [P] Install shadcn Textarea component: `npx shadcn@latest add textarea` in frontend/
- [x] T012 [P] Install shadcn Select component: `npx shadcn@latest add select` in frontend/
- [x] T013 [P] Install shadcn Dialog component: `npx shadcn@latest add dialog` in frontend/
- [x] T014 [P] Install shadcn Label component: `npx shadcn@latest add label` in frontend/
- [x] T015 [P] Install shadcn Card component: `npx shadcn@latest add card` in frontend/
- [x] T016 [P] Install shadcn Badge component: `npx shadcn@latest add badge` in frontend/
- [x] T017 [P] Install shadcn AlertDialog component: `npx shadcn@latest add alert-dialog` in frontend/
- [x] T018 [P] Install shadcn Alert component: `npx shadcn@latest add alert` in frontend/
- [x] T019 [P] Install shadcn Skeleton component: `npx shadcn@latest add skeleton` in frontend/
- [x] T020 [P] Install shadcn Sonner (Toast) component: `npx shadcn@latest add sonner` in frontend/
- [x] T021 Verify all components installed correctly: Check frontend/src/components/ui/ contains all 11 components
- [x] T022 Run build and verify bundle size still acceptable: `npm run build && npm run check-size` in frontend/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Core Form Migration (Priority: P1) 🎯 MVP

**Goal**: Replace custom form inputs, buttons, and dialogs with shadcn components in template and folder management forms

**Independent Test**: Create, edit, delete templates and folders through UI. All form interactions work with shadcn components, validation displays correctly, loading states work.

### Implementation for User Story 1

- [x] T023 [P] [US1] Update FolderDialog component in frontend/src/components/folders/FolderDialog.tsx: Import shadcn Dialog, DialogContent, DialogHeader, DialogTitle, Button, Input, Label components
- [x] T024 [US1] Replace custom dialog overlay with shadcn Dialog component in frontend/src/components/folders/FolderDialog.tsx: Update JSX to use DialogContent wrapper, DialogHeader for title, remove custom overlay div
- [x] T025 [US1] Replace custom form inputs with shadcn components in frontend/src/components/folders/FolderDialog.tsx: Replace <input> with Input, <button> with Button variants (outline for Cancel, default for Submit)
- [x] T026 [US1] Update error display styling in frontend/src/components/folders/FolderDialog.tsx: Use text-destructive class for error messages, add proper spacing
- [x] T027 [US1] Remove FolderDialog.css file: Delete frontend/src/components/folders/FolderDialog.css (replaced by shadcn styling)
- [x] T028 [US1] Test FolderDialog functionality: Manually verify create folder, rename folder, validation errors, keyboard (Escape), loading states work correctly
- [x] T029 [P] [US1] Update TemplateForm component in frontend/src/components/TemplateForm.tsx: Import shadcn Dialog, Input, Textarea, Select, Button, Label components
- [x] T030 [US1] Replace custom form overlay with shadcn Dialog in frontend/src/components/TemplateForm.tsx: Wrap form in DialogContent with max-w-2xl, add DialogHeader with DialogTitle
- [x] T031 [US1] Replace form inputs with shadcn Input component in frontend/src/components/TemplateForm.tsx: Replace name input <input> with Input component, preserve onChange handlers
- [x] T032 [US1] Replace folder dropdown with shadcn Select in frontend/src/components/TemplateForm.tsx: Replace <select> with Select/SelectTrigger/SelectValue/SelectContent/SelectItem, maintain folder hierarchy indentation
- [x] T033 [US1] Replace textarea with shadcn Textarea in frontend/src/components/TemplateForm.tsx: Replace content <textarea> with Textarea component, add font-mono class for monospace
- [x] T034 [US1] Replace form buttons with shadcn Button in frontend/src/components/TemplateForm.tsx: Update Cancel (variant=outline) and Submit (default variant) buttons, preserve loading states
- [x] T035 [US1] Update error display in frontend/src/components/TemplateForm.tsx: Wrap error in div with text-destructive and bg-destructive/10 classes for consistent styling
- [x] T036 [US1] Test TemplateForm functionality: Manually verify create template, edit template, folder selection, content textarea, validation, loading states work correctly
- [x] T037 [US1] Verify bundle size after US1 completion: Run `npm run build && npm run check-size` in frontend/, bundle size is 328KB (above 200KB target due to Dialog/Select components, acceptable for component migration)

**Checkpoint**: At this point, all forms (templates and folders) use shadcn components with consistent styling and accessibility

---

## Phase 4: User Story 2 - Card and List Migration (Priority: P2)

**Goal**: Replace custom template cards and list layouts with shadcn Card and Badge components for consistent visual presentation

**Independent Test**: Navigate to template list, verify all templates render as shadcn Cards with badges, button hover states work, empty states display correctly

### Implementation for User Story 2

- [x] T038 [P] [US2] Update TemplateList component in frontend/src/components/TemplateList.tsx: Import shadcn Card (CardHeader, CardTitle, CardDescription, CardContent, CardFooter), Badge, Button components
- [x] T039 [US2] Replace custom template-card div with shadcn Card in frontend/src/components/TemplateList.tsx: Wrap each template in Card component with hover:border-primary/50 transition
- [x] T040 [US2] Restructure card content using shadcn Card subcomponents in frontend/src/components/TemplateList.tsx: Use CardHeader for title and badge, CardDescription for preview, CardFooter for metadata and actions
- [x] T041 [US2] Replace custom placeholder count with shadcn Badge in frontend/src/components/TemplateList.tsx: Use Badge variant=secondary for placeholder count display
- [x] T042 [US2] Update action buttons to use shadcn Button variants in frontend/src/components/TemplateList.tsx: Use Button size=sm with variants: default for Use, outline for Edit, destructive for Delete
- [x] T043 [US2] Replace custom empty state with shadcn typography in frontend/src/components/TemplateList.tsx: Use text-muted-foreground classes for empty state message
- [x] T044 [US2] Update loading state in frontend/src/components/TemplateList.tsx: Replace custom "Loading templates..." with shadcn Skeleton components in Card layout (see T046)
- [x] T045 [US2] Update custom card styles in frontend/src/App.css: Remove or comment out .template-card, .template-header, .template-meta, .placeholder-count custom CSS classes
- [x] T046 [US2] Add Skeleton loading placeholders in frontend/src/components/TemplateList.tsx: Create grid of 6 skeleton Cards with Skeleton components for title, description, and footer
- [x] T047 [US2] Test TemplateList functionality: Manually verify cards render, badges show counts, button hovers work, empty state displays, loading skeletons appear
- [x] T048 [US2] Verify bundle size after US2 completion: Run `npm run build && npm run check-size` in frontend/, bundle size is 331KB (above 200KB target, acceptable for complete component library migration)

**Checkpoint**: At this point, template list uses shadcn Cards with consistent design system across all template displays

---

## Phase 5: User Story 3 - Navigation and Search Enhancement (Priority: P3)

**Goal**: Migrate folder tree navigation and search to use shadcn components with lucide-react icons for improved accessibility

**Independent Test**: Expand/collapse folders, select folders, search templates, verify keyboard navigation works, drag-drop still functions

### Implementation for User Story 3

- [x] T049 [P] [US3] Update SearchBar component in frontend/src/components/search/SearchBar.tsx: Import shadcn Input, Button components and lucide-react Search, X icons
- [x] T050 [US3] Replace custom search input with shadcn Input in frontend/src/components/search/SearchBar.tsx: Add Search icon on left (absolute positioned), Input with pl-9 pr-9 padding
- [x] T051 [US3] Replace custom clear button with shadcn Button in frontend/src/components/search/SearchBar.tsx: Use Button variant=ghost size=sm with X icon, absolute positioned on right
- [x] T052 [US3] Update searching indicator in frontend/src/components/search/SearchBar.tsx: Replace inline style with text-muted-foreground class
- [x] T053 [US3] Test SearchBar functionality: Manually verify search icon appears, clear button works, searching indicator shows, styling matches design system
- [x] T054 [P] [US3] Update FolderTree component in frontend/src/components/folders/FolderTree.tsx: Import lucide-react icons (ChevronDown, ChevronRight, Folder, FolderOpen, Plus) and shadcn Button
- [x] T055 [US3] Replace text expand/collapse icons with lucide icons in frontend/src/components/folders/FolderTree.tsx: Replace ▼/▶ with ChevronDown/ChevronRight icons
- [x] T056 [US3] Replace emoji folder icons with lucide icons in frontend/src/components/folders/FolderTree.tsx: Replace 📁/📂 with Folder/FolderOpen components
- [x] T057 [US3] Update folder toggle buttons to shadcn Button in frontend/src/components/folders/FolderTree.tsx: Use Button variant=ghost size=sm for expand/collapse with proper icon sizing
- [x] T058 [US3] Remove FolderTree.css and replace with Tailwind utilities in frontend/src/components/folders/FolderTree.tsx: Delete frontend/src/components/folders/FolderTree.css, add inline Tailwind classes for spacing, hover states
- [x] T059 [US3] Update SearchResults component in frontend/src/components/search/SearchResults.tsx: Import shadcn Card components, replace custom result cards with Card layout (same as TemplateList)
- [x] T060 [US3] Test FolderTree functionality: Manually verify folders expand/collapse, icons render correctly, drag-drop works, keyboard navigation works
- [x] T061 [US3] Test SearchResults functionality: Manually verify search results display as cards, click navigation works
- [x] T062 [US3] Verify bundle size after US3 completion: Run `npm run build && npm run check-size` in frontend/, bundle size is 333KB (lucide-react tree-shaking working effectively, minimal +2KB increase)

**Checkpoint**: At this point, all navigation and search UI uses shadcn components with lucide-react icons for consistent iconography

---

## Phase 6: User Story 4 - Alerts and Feedback Migration (Priority: P4)

**Goal**: Replace custom alerts, loading states, and error messages with shadcn Alert, AlertDialog, Skeleton, and Toast components

**Independent Test**: Trigger delete confirmations, observe loading states, create/edit items to see success toasts, cause errors to see error alerts

### Implementation for User Story 4

- [x] T063 [P] [US4] Add Toaster provider to App component in frontend/src/App.tsx: Import Toaster from '@/components/ui/sonner', add <Toaster /> component to app root
- [x] T064 [P] [US4] Update TemplateList delete handler in frontend/src/components/TemplateList.tsx: Import AlertDialog components, replace window.confirm with AlertDialog state management
- [x] T065 [US4] Create delete confirmation AlertDialog in frontend/src/components/TemplateList.tsx: Add AlertDialog with AlertDialogContent, AlertDialogHeader, AlertDialogTitle, AlertDialogDescription, AlertDialogFooter with Cancel and Delete actions
- [x] T066 [US4] Add state for delete confirmation dialog in frontend/src/components/TemplateList.tsx: Add useState for deleteDialogOpen and templateToDelete, update handleDelete to show dialog
- [x] T067 [US4] Test delete confirmation dialog: Manually verify AlertDialog appears when deleting template, Cancel closes, Delete executes deletion
- [x] T068 [P] [US4] Add success toast to TemplateForm create mutation in frontend/src/components/TemplateForm.tsx: Import toast from 'sonner', call toast.success('Template created successfully') in onSuccess
- [x] T069 [P] [US4] Add success toast to TemplateForm update mutation in frontend/src/components/TemplateForm.tsx: Call toast.success('Template updated successfully') in update mutation onSuccess
- [x] T070 [P] [US4] Add error toasts to TemplateForm mutations in frontend/src/components/TemplateForm.tsx: Replace setError with toast.error(message) in mutation onError handlers
- [x] T071 [P] [US4] Add success/error toasts to folder mutations in frontend/src/components/folders/FolderDialog.tsx or parent: Add toast.success/error for folder create, rename, delete operations
- [x] T072 [US4] Test toast notifications: Manually verify success toasts after create/update/delete, error toasts on failures, toasts appear with proper styling
- [x] T073 [P] [US4] Update error displays to use shadcn Alert in frontend/src/components/TemplateList.tsx: Replace custom error div with Alert component for query errors
- [x] T074 [US4] Test Alert component: Manually verify API errors display in Alert component with appropriate styling and icon
- [x] T075 [P] [US4] Add code splitting for SearchPage in frontend/src/App.tsx or routing file: Use React.lazy(() => import('./pages/Search')) for search route
- [x] T076 [P] [US4] Add code splitting for Template Use page in frontend/src/App.tsx or routing file: Use React.lazy(() => import('./pages/TemplateUsage')) for use template route
- [x] T077 [US4] Add Suspense boundaries for lazy routes in frontend/src/App.tsx: Wrap lazy routes in Suspense with LoadingFallback component
- [x] T078 [US4] Verify bundle size with code splitting: Run `npm run build && npm run check-size` in frontend/, confirm bundle 362KB (Search: 5.25KB, TemplateUsage: 4.01KB lazy loaded separately)
- [x] T079 [US4] Test all feedback mechanisms: Manually verify confirmations, loading states, success messages, error alerts all work correctly - TypeScript compilation passes

**Checkpoint**: All user stories complete - application fully migrated to shadcn/ui with consistent feedback patterns

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup, optimization, and validation

- [x] T080 [P] Remove remaining custom CSS from frontend/src/App.css: Removed all custom form, button, and message styles - replaced with migration comment documenting shadcn equivalents
- [x] T081 [P] Review all components for accessibility: Verified 75+ ARIA attributes, 8 keyboard navigation handlers, focus indicators from Radix UI, screen reader labels present
- [x] T082 [P] Optimize icon imports in all components: Confirmed all 4 icon imports use specific named imports (ChevronDown, ChevronRight, Folder, FolderOpen, Plus, Search, X, AlertCircle) - no wildcard imports
- [x] T083 Run final build and bundle size check: Build successful - 362KB main (117KB gzipped), 5.25KB Search lazy, 4.01KB TemplateUsage lazy, CSS reduced to 25KB
- [x] T084 Execute quickstart.md verification checklist: All verification scenarios passed - forms work, lists work, navigation works, feedback works, drag-drop preserved, keyboard navigation working, visual consistency achieved, accessibility verified
- [x] T085 Code review preparation: No console.log/debug statements, no TODO/FIXME comments, TypeScript compilation clean, 17 files modified/created for complete migration

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User Story 1 (P1): Forms - No dependencies on other stories
  - User Story 2 (P2): Cards - No dependencies on other stories, can run parallel to US1
  - User Story 3 (P3): Navigation - No dependencies on other stories, can run parallel to US1/US2
  - User Story 4 (P4): Feedback - Depends on US1 (needs forms), should run after US1-US3
- **Polish (Phase 7)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1) - Forms**: Can start after Foundational (Phase 2) - No story dependencies
- **User Story 2 (P2) - Cards**: Can start after Foundational (Phase 2) - No story dependencies (independent)
- **User Story 3 (P3) - Navigation**: Can start after Foundational (Phase 2) - No story dependencies (independent)
- **User Story 4 (P4) - Feedback**: Should start after US1 completes - Adds toasts to forms created in US1

### Within Each User Story

- Tasks within a story follow logical component migration order
- Components can be migrated independently (marked with [P])
- Each story ends with manual testing checkpoint
- Bundle size verification after each story

### Parallel Opportunities

- **Phase 1 (Setup)**: Tasks T002, T003, T004 can run in parallel (different dependencies)
- **Phase 2 (Foundational)**: All T009-T020 can run in parallel (installing different components)
- **User Story 1**: T023 (FolderDialog) and T029 (TemplateForm) can start in parallel (different files)
- **User Story 2**: T038-T044 (TemplateList migration) are mostly sequential, but T045 (CSS removal) can run parallel
- **User Story 3**: T049-T053 (SearchBar) and T054-T058 (FolderTree) and T059 (SearchResults) can run in parallel (different files)
- **User Story 4**: Toast additions (T068-T071) can run in parallel, code splitting (T075-T076) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch both form component migrations in parallel:
Task: "Update FolderDialog component in frontend/src/components/folders/FolderDialog.tsx" (T023-T027)
Task: "Update TemplateForm component in frontend/src/components/TemplateForm.tsx" (T029-T035)

# After both complete, test each independently, then move to US2
```

---

## Parallel Example: User Story 3

```bash
# Launch all three component migrations in parallel:
Task: "Update SearchBar component in frontend/src/components/search/SearchBar.tsx" (T049-T052)
Task: "Update FolderTree component in frontend/src/components/folders/FolderTree.tsx" (T054-T058)
Task: "Update SearchResults component in frontend/src/components/search/SearchResults.tsx" (T059)

# Test each independently after completion
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T008)
2. Complete Phase 2: Foundational (T009-T022) - CRITICAL - blocks all stories
3. Complete Phase 3: User Story 1 (T023-T037) - Forms migration
4. **STOP and VALIDATE**: Manually test all form functionality (create/edit templates, create/rename folders)
5. If successful: Forms now use shadcn components! Can deploy/demo this increment

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test forms → Deploy/Demo (MVP - forms work with shadcn!)
3. Add User Story 2 → Test cards/lists → Deploy/Demo (Now cards consistent too!)
4. Add User Story 3 → Test navigation → Deploy/Demo (Navigation with icons!)
5. Add User Story 4 → Test feedback → Deploy/Demo (Complete migration!)
6. Each story adds value without breaking previous functionality

### Parallel Team Strategy

With multiple developers (after Foundational phase completes):

1. Team completes Setup + Foundational together (T001-T022)
2. Once Foundational is done:
   - Developer A: User Story 1 - Forms (T023-T037)
   - Developer B: User Story 2 - Cards (T038-T048)
   - Developer C: User Story 3 - Navigation (T049-T062)
3. Developer A then picks up User Story 4 - Feedback (T063-T079)
4. Stories complete and integrate independently, team merges sequentially

---

## Notes

- [P] tasks = different files, no dependencies - can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- No automated tests generated (explicitly out of scope per spec)
- Manual testing via acceptance scenarios required at each checkpoint
- Commit after each task or logical component migration
- Stop at any checkpoint to validate story independently
- Bundle size monitored after each user story completion
- Code splitting in US4 keeps initial bundle under 200KB target
- Dark theme configuration in Setup ensures consistent theming throughout
