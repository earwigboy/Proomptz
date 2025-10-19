# Implementation Plan: Migrate Frontend to shadcn/ui Components

**Branch**: `002-shadcn-migration` | **Date**: 2025-10-19 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-shadcn-migration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Migrate all existing custom React components in the frontend application to standardized shadcn/ui components to improve visual consistency, accessibility, and development velocity. The migration covers 9 custom components (TemplateForm, TemplateList, FolderDialog, FolderTree, SearchBar, SearchResults, PlaceholderForm, PromptPreview, FolderContextMenu) and replaces custom CSS styling with shadcn's design system while preserving all existing functionality including drag-and-drop, keyboard navigation, and form validation.

## Technical Context

**Language/Version**: TypeScript 5.9.3 with React 19.1.1
**Primary Dependencies**: React 19, TanStack Query 5.90.5, React Router 7.9.4, Vite 7.1.7, shadcn/ui (to be installed)
**Storage**: N/A (frontend only - consumes existing REST API)
**Testing**: NEEDS CLARIFICATION (no test framework currently configured - spec indicates tests are out of scope unless requested)
**Target Platform**: Modern web browsers (ES2022+ compatible)
**Project Type**: Web application (frontend SPA consuming REST API)
**Performance Goals**: Bundle size <200KB for initial load (currently enforced in build script), First Contentful Paint <1.5s, Largest Contentful Paint <2.5s (per constitution)
**Constraints**: Must preserve existing functionality during incremental migration, maintain dark theme, no breaking changes to component props, work within Vite build tooling
**Scale/Scope**: 9 custom components to migrate, ~1500 lines of component code, 8 shadcn components to install (Dialog, AlertDialog, Card, Button, Input, Textarea, Select, Badge, Alert, Toast/Sonner, Skeleton)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Standards

| Principle | Status | Notes |
|-----------|--------|-------|
| Single Responsibility | ✅ PASS | Each shadcn component has clear purpose; migration maintains existing component boundaries |
| DRY Principle | ✅ PASS | Migrating to shadcn eliminates duplicated styling patterns; shared components reused throughout |
| Meaningful Names | ✅ PASS | Preserving existing component names; shadcn components have descriptive names |
| Type Safety | ✅ PASS | TypeScript strict mode enabled; shadcn components are fully typed |
| Error Handling | ✅ PASS | Existing error handling preserved; shadcn components expose error states |
| Code Review | ✅ PASS | Standard PR review process applies |
| Linting & Formatting | ✅ PASS | ESLint already configured; no changes to linting requirements |

### II. Testing Standards

| Principle | Status | Notes |
|-----------|--------|-------|
| Test Pyramid | ⚠️ DEFERRED | No tests currently exist; spec marks tests as out of scope unless requested |
| Independent Tests | ⚠️ DEFERRED | N/A - no test suite |
| Fast Feedback | ⚠️ DEFERRED | N/A - no test suite |
| Readable Tests | ⚠️ DEFERRED | N/A - no test suite |
| Coverage Gates | ⚠️ DEFERRED | No coverage configured; spec explicitly excludes adding tests |
| Contract Testing | ✅ PASS | API contracts unchanged; frontend consumes existing REST API |
| TDD Encouraged | ⚠️ DEFERRED | Not applicable for UI component migration without test suite |

**Testing Decision**: The specification explicitly marks "Adding unit or integration tests (unless specifically requested later)" as OUT OF SCOPE. This is a UI component migration focused on visual consistency and accessibility, not a functional change. Existing functionality is preserved through manual testing of user stories. If tests are desired, they should be added in a separate feature after migration completes.

### III. User Experience Consistency

| Principle | Status | Notes |
|-----------|--------|-------|
| Design System | ✅ PASS | **PRIMARY GOAL** - Migrating TO design system (shadcn) for consistency |
| Accessibility | ✅ PASS | Requirement FR-014 mandates maintaining/enhancing ARIA attributes; shadcn has built-in WCAG 2.1 AA compliance |
| Responsive Design | ✅ PASS | Requirement FR-017 maintains existing responsive behavior; shadcn components are responsive by default |
| Error Messages | ✅ PASS | Requirement FR-019 preserves validation messages; shadcn provides error styling patterns |
| Loading States | ✅ PASS | Requirements FR-008, FR-020 mandate consistent loading indicators via Skeleton/Spinner components |
| Consistent Patterns | ✅ PASS | **PRIMARY GOAL** - Standardizing forms, dialogs, cards, buttons across features |
| Internationalization | ✅ PASS | No i18n currently; shadcn doesn't prevent future i18n implementation |

### IV. Performance Requirements

| Principle | Status | Notes |
|-----------|--------|-------|
| Response Time | ✅ PASS | Frontend consumes existing API; no API changes affect response times |
| Page Load | ✅ PASS | FCP <1.5s, LCP <2.5s targets preserved; shadcn uses tree-shaking to minimize bundle impact |
| Bundle Size | ⚠️ MONITOR | Current target: <200KB gzipped. shadcn adds Radix UI primitives (~30-40KB for used components). Need to verify post-migration. |
| Database Queries | N/A | Frontend only; no database access |
| Memory Footprint | N/A | Browser-based; no server processes |
| Scalability | ✅ PASS | No architectural changes affect horizontal scaling |
| Monitoring | ✅ PASS | Existing monitoring unchanged |

**Performance Decision**: Bundle size requires monitoring. Current build: ~200KB warning threshold enforced. shadcn uses tree-shaking (only imports used components), but adding Radix UI primitives will increase bundle. Mitigation: Remove custom CSS (~10-15KB), lazy-load routes, verify bundle stays under target post-migration.

### Gate Summary

**STATUS**: ✅ **CONDITIONAL PASS** - Proceed to Phase 0 with monitoring requirements

**Conditions**:
1. **Bundle Size**: Must verify bundle size remains <200KB after shadcn installation in Phase 0 research
2. **Testing**: Deferred per spec requirements; manual testing of user stories required before release
3. **Accessibility**: Must validate WCAG 2.1 AA compliance maintained in migrated components during implementation

**Violations Requiring Justification**: None - see Complexity Tracking for testing deferral justification.

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
frontend/
├── src/
│   ├── components/
│   │   ├── ui/                    # NEW: shadcn components will be installed here
│   │   │   ├── button.tsx        # NEW: shadcn Button
│   │   │   ├── input.tsx         # NEW: shadcn Input
│   │   │   ├── textarea.tsx      # NEW: shadcn Textarea
│   │   │   ├── select.tsx        # NEW: shadcn Select
│   │   │   ├── dialog.tsx        # NEW: shadcn Dialog
│   │   │   ├── alert-dialog.tsx  # NEW: shadcn AlertDialog
│   │   │   ├── card.tsx          # NEW: shadcn Card
│   │   │   ├── badge.tsx         # NEW: shadcn Badge
│   │   │   ├── alert.tsx         # NEW: shadcn Alert
│   │   │   ├── skeleton.tsx      # NEW: shadcn Skeleton
│   │   │   └── sonner.tsx        # NEW: shadcn Toast/Sonner
│   │   ├── folders/
│   │   │   ├── FolderTree.tsx           # MIGRATE: Use Collapsible/Tree patterns
│   │   │   ├── FolderTree.css           # REMOVE: Replace with shadcn styling
│   │   │   ├── FolderDialog.tsx         # MIGRATE: Use Dialog component
│   │   │   ├── FolderDialog.css         # REMOVE: Replace with shadcn styling
│   │   │   └── FolderContextMenu.tsx    # MIGRATE: Use shadcn patterns
│   │   ├── placeholders/
│   │   │   ├── PlaceholderForm.tsx      # MIGRATE: Use Input, Button
│   │   │   └── PromptPreview.tsx        # MIGRATE: Use Card, typography
│   │   ├── search/
│   │   │   ├── SearchBar.tsx            # MIGRATE: Use Input with icon
│   │   │   └── SearchResults.tsx        # MIGRATE: Use Card components
│   │   ├── TemplateForm.tsx             # MIGRATE: Use Input, Textarea, Select, Button
│   │   └── TemplateList.tsx             # MIGRATE: Use Card, Badge, Button
│   ├── lib/
│   │   ├── utils.ts              # NEW: shadcn utilities (cn helper)
│   │   ├── api/                  # UNCHANGED: Generated API client
│   │   ├── api-client.ts         # UNCHANGED: API client wrapper
│   │   └── hooks/                # UNCHANGED: React Query hooks
│   ├── pages/                    # UNCHANGED: Route components
│   ├── App.tsx                   # UPDATE: Add Toaster provider
│   ├── App.css                   # UPDATE: Remove custom component styles
│   ├── index.css                 # UPDATE: Add shadcn global styles
│   └── main.tsx                  # UNCHANGED: React app entry
├── components.json               # NEW: shadcn configuration file
├── tailwind.config.js            # NEW: Tailwind configuration (if using Tailwind approach)
├── tsconfig.json                 # UPDATE: Add @/ path alias for @/components/ui
├── tsconfig.app.json             # UPDATE: Ensure paths configuration
├── vite.config.ts                # UPDATE: Add path aliases resolution
└── package.json                  # UPDATE: Add shadcn dependencies

backend/                          # UNCHANGED: No backend changes
shared/                           # UNCHANGED: No shared API contract changes
```

**Structure Decision**: This is a **web application** (Option 2) with separate frontend and backend. This migration only affects the `frontend/` directory. The key structural changes are:

1. **New `src/components/ui/` directory**: shadcn components installed here via CLI
2. **New `src/lib/utils.ts`**: shadcn utility functions (cn helper for className merging)
3. **Migrate 9 existing components**: Replace custom implementations with shadcn components
4. **Remove 2 CSS files**: FolderTree.css, FolderDialog.css replaced by shadcn styling
5. **Update configuration**: Add path aliases, Tailwind config (if using Tailwind approach), components.json

The backend and shared API contracts remain completely unchanged - this is purely a frontend UI migration.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Testing Standards deferred | This is a UI component migration preserving existing functionality, not adding new business logic. Manual testing via user story acceptance scenarios is sufficient. | Adding test suite now would delay delivery without preventing regressions (no baseline tests exist). Better to validate migration manually, then add tests in future feature if desired. |
| Bundle size monitoring required | shadcn + Radix UI add ~30-40KB. Current bundle near 200KB threshold. | Cannot eliminate - shadcn is the requirement. Mitigation: remove custom CSS, verify tree-shaking works, lazy-load if needed. Monitor in Phase 0 research. |

---

## Constitution Re-Check (Post-Phase 1 Design)

### Bundle Size Status: ✅ RESOLVED

**Research Findings** (from research.md):
- Projected bundle with shadcn: ~228KB (EXCEEDS target)
- With mitigations:
  - Remove custom CSS: -12KB
  - Code splitting (lazy load search/use pages): -25KB
  - Optimize icon imports: Already accounted for
- **Final projection**: ~191KB ✅ **UNDER 200KB TARGET**

**Mitigation Plan** (documented in quickstart.md):
1. Remove FolderDialog.css, FolderTree.css, custom App.css styles
2. Implement React.lazy for SearchPage and TemplateUsePage
3. Import specific icons only: `import { Search, X } from 'lucide-react'`
4. Monitor with `npm run check-size` after each migration phase

**Gate Status**: ✅ PASS - Bundle size concern resolved with documented mitigation strategy

### Testing Status: ⚠️ DEFERRED (Justified)

**Manual Testing Plan** (documented in quickstart.md):
- Each user story has detailed acceptance scenarios
- Checklist-based validation for all functionality
- Focus on:
  - Functional preservation (forms, navigation, search)
  - Visual consistency (components, styling, theme)
  - Accessibility (keyboard nav, screen readers, ARIA)
  - Performance (bundle size, page load)

**Gate Status**: ⚠️ DEFERRED - Acceptable per spec requirements and Complexity Tracking justification

### Final Assessment

**Overall Status**: ✅ **PASS - Ready for Phase 2 (Tasks Generation)**

All constitutional requirements met or justified:
- ✅ Code Quality: Maintained
- ⚠️ Testing: Deferred with justification
- ✅ UX Consistency: Core objective of migration
- ✅ Performance: Bundle size under target with mitigations

**No blocking issues for task generation**.

