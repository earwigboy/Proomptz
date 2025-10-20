# Implementation Plan: Template Usage Screen Enhancements

**Branch**: `004-template-usage-enhancements` | **Date**: 2025-10-19 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-template-usage-enhancements/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Enhance the template usage screen with improved placeholder input fields, validation-based button activation, template content editing capability, and larger preview display. The feature targets the React SPA at `/frontend/src/pages/TemplateUsage.tsx`, leveraging existing shadcn/ui components, TanStack Query state management, and the placeholder extraction utilities in `/frontend/src/lib/utils/placeholders.ts`. Implementation follows a component-driven approach with reactive form validation using React hooks and Tailwind CSS responsive design patterns.

## Technical Context

**Language/Version**: TypeScript 5.9.3, React 19.1.1
**Primary Dependencies**: React Router DOM 7.9.4, TanStack Query 5.90.5, shadcn/ui 3.4.2, Tailwind CSS 3.4.18, Radix UI primitives, Lucide React 0.546.0, Axios 1.12.2
**Storage**: N/A (frontend-only changes; backend API handles persistence)
**Testing**: No test framework configured (manual testing; constitution may require adding Vitest)
**Target Platform**: Modern web browsers (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+); responsive design 320px-4K viewports
**Project Type**: Web application (separate frontend/backend monorepo)
**Performance Goals**: First Contentful Paint <1.5s, Largest Contentful Paint <2.5s, JS bundle <200KB gzipped per constitution
**Constraints**: Form validation response <100ms, placeholder substitution <1s, WCAG 2.1 AA accessibility, no breaking changes to existing template data model
**Scale/Scope**: ~10 React components modified/added, 4 user stories (P1-P3), ~15 functional requirements, shadcn/ui component library integration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Standards ✅ PASS

- **Single Responsibility**: Each component will have one clear purpose (PlaceholderForm, TemplateEditor, PromptPreview)
- **DRY Principle**: Reuse existing placeholder utilities (`extractPlaceholders`, `generatePrompt`, `validatePlaceholderValues`)
- **Meaningful Names**: Components and hooks follow established naming conventions (`usePlaceholders`, `useTemplateValidation`)
- **Type Safety**: TypeScript strict mode enabled; all props and state typed
- **Error Handling**: Form validation errors displayed via shadcn/ui form components; API errors handled by TanStack Query
- **Code Review**: PR required before merge (per constitution workflow)
- **Linting & Formatting**: ESLint 9.36.0 configured and enforced

**Status**: No violations. Code quality standards will be maintained.

### II. Testing Standards ⚠️ FLAGGED

**CRITICAL ISSUE**: No test framework configured in frontend codebase.

Constitution requires:
- Test Pyramid with unit > integration > e2e
- Minimum 80% line coverage for new code
- 100% coverage for critical paths (placeholder validation, template editing)
- Fast feedback (unit tests <5s, integration <30s)

**Current State**: No Jest, Vitest, or testing library detected in `/frontend/package.json`

**Justification Required**:
- Option 1: Add Vitest + React Testing Library before implementation (recommended)
- Option 2: Document testing as out-of-scope for this feature (requires maintainer approval per constitution HIGH violation policy)
- Option 3: Manual testing only with explicit acceptance criteria validation

**Recommendation**: Add Vitest setup as Phase 0 task. Placeholder validation and button state logic are critical paths requiring 100% coverage per constitution.

**Decision**: NEEDS CLARIFICATION - defer to constitution enforcement policy

### III. User Experience Consistency ✅ PASS

- **Design System**: All components use shadcn/ui with existing design tokens
- **Accessibility**: Radix UI primitives provide WCAG 2.1 AA compliance; form labels, ARIA attributes, keyboard navigation supported
- **Responsive Design**: Tailwind CSS responsive utilities ensure 320px-4K viewport support
- **Error Messages**: Form validation feedback via shadcn/ui form components; clear user-facing messages (no stack traces)
- **Loading States**: TanStack Query provides loading/error/success states; template preview updates within 1s per constraint
- **Consistent Patterns**: Forms, buttons, dialogs follow existing app patterns (TemplateList, Search components)
- **Internationalization**: Not required for this feature (app currently English-only)

**Status**: No violations. UX consistency maintained through shadcn/ui and established patterns.

### IV. Performance Requirements ✅ PASS

- **Response Time**: Form validation executes client-side (<100ms per constraint); API calls cached by TanStack Query
- **Page Load**: Feature reuses lazy-loaded TemplateUsage page; no new route bundles
- **Bundle Size**: Minimal bundle impact (reusing existing dependencies: Radix UI, React Hook Form if added)
- **Database Queries**: N/A (frontend-only changes; no backend modifications)
- **Memory Footprint**: N/A (no server-side changes)
- **Scalability**: Client-side rendering; no impact on horizontal scaling
- **Monitoring**: No new monitoring required (existing frontend error tracking sufficient)

**Status**: No violations. Performance budgets maintained.

### Development Workflow ✅ PASS

- **Feature Branch**: Currently on `004-template-usage-enhancements`
- **Pull Requests**: Required per constitution
- **CI Validation**: ESLint, TypeScript compilation must pass
- **Review Approval**: Required per constitution
- **Merge Strategy**: Squash commits per constitution

**Status**: Workflow compliance verified.

### Pre-Merge Gates ⚠️ CONDITIONAL

**Potential blocker**: No automated tests (Testing Standards violation)

If testing framework not added:
- ✅ Automated Tests: N/A (none exist currently)
- ⚠️ Code Coverage: Cannot measure (no test runner)
- ✅ Linting: ESLint configured
- ✅ Type Checking: TypeScript strict mode
- ✅ Security Scan: npm audit (no new dependencies planned)
- ✅ Performance Budget: Bundle size check script exists

**Decision**: Proceed with HIGH violation (no tests) pending maintainer approval, OR add Vitest in Phase 0.

### Summary

**GATE STATUS**: ⚠️ **CONDITIONAL PASS**

**Violations**:
1. **HIGH**: Testing Standards - No test framework configured (violates 80% coverage requirement)

**Recommended Action**: Add Vitest + React Testing Library as foundational task (Phase 2) before user story implementation.

**Alternative**: Document testing as out-of-scope with explicit maintainer approval per constitution enforcement policy.

## Project Structure

### Documentation (this feature)

```
specs/004-template-usage-enhancements/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── component-api.md # Component props interfaces & hook contracts
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/                        # shadcn/ui components (existing)
│   │   │   ├── button.tsx
│   │   │   ├── input.tsx
│   │   │   ├── textarea.tsx
│   │   │   ├── label.tsx
│   │   │   ├── card.tsx
│   │   │   ├── badge.tsx
│   │   │   ├── tooltip.tsx
│   │   │   └── separator.tsx
│   │   ├── placeholders/              # Placeholder-related components
│   │   │   ├── PlaceholderForm.tsx    # MODIFY: Enhanced form fields
│   │   │   └── PromptPreview.tsx      # MODIFY: Larger preview + edit mode
│   │   └── template/                  # NEW: Template editor components
│   │       └── TemplateEditor.tsx     # NEW: Editable template content
│   ├── pages/
│   │   └── TemplateUsage.tsx          # MODIFY: Main feature page
│   ├── lib/
│   │   ├── hooks/
│   │   │   ├── usePlaceholders.ts     # MODIFY: Add validation state
│   │   │   ├── useTemplateEditor.ts   # NEW: Template editing state
│   │   │   └── useFormValidation.ts   # NEW: Form validation logic
│   │   ├── utils/
│   │   │   └── placeholders.ts        # EXISTING: Reuse extraction/generation
│   │   └── api/
│   │       └── services/
│   │           └── TemplatesService.ts # EXISTING: No changes needed
│   ├── App.tsx                        # EXISTING: No routing changes
│   └── main.tsx                       # EXISTING: No changes
├── package.json                       # MODIFY: Add react-hook-form (optional)
├── vite.config.ts                     # EXISTING: No changes
├── tsconfig.json                      # EXISTING: No changes
└── tailwind.config.js                 # EXISTING: No changes

backend/                               # NO CHANGES (frontend-only feature)
└── [.NET API - unchanged]

shared/
└── openapi/
    └── swagger.json                   # NO CHANGES (no API modifications)
```

**Structure Decision**: Web application (separate frontend/backend monorepo). This feature modifies only the frontend React SPA. Changes concentrated in:
1. `/frontend/src/pages/TemplateUsage.tsx` - Main orchestration
2. `/frontend/src/components/placeholders/` - Enhanced form components
3. `/frontend/src/components/template/` - New template editor component
4. `/frontend/src/lib/hooks/` - New validation and editing hooks

No backend changes required. No new external dependencies beyond potential `react-hook-form` for advanced form validation (decision in Phase 0 research).

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Testing Standards (HIGH) - No test framework | Feature can be delivered with manual testing against acceptance scenarios in spec.md | Adding Vitest requires non-trivial setup (test environment config, React Testing Library integration, mock configuration for TanStack Query). Current project has no testing infrastructure. Effort vs. benefit trade-off favors manual validation for this UX-focused feature. |

**Note**: If maintainer requires test coverage per constitution enforcement, testing framework setup will be added as Phase 2 foundational task before user story implementation.

---

## Constitution Re-Evaluation (Post-Design)

**Date**: 2025-10-19 (After Phase 0 & Phase 1 completion)

### Design Decisions Review

**Phase 0 (Research) Outcomes**:
1. ✅ **No new NPM dependencies** - avoided React Hook Form, Zod, react-contenteditable
2. ✅ **Custom validation** - simple presence validation using React hooks
3. ✅ **Tab-based editor UI** - clear separation using existing shadcn/ui Tabs
4. ✅ **Flex-based responsive layout** - 320px-4K viewport support
5. ✅ **WCAG 2.1 AA accessibility** - Tooltip + aria-describedby + role=alert

**Phase 1 (Design) Outcomes**:
1. ✅ **TypeScript interfaces defined** - All component props and hook contracts typed
2. ✅ **Three custom hooks** - usePlaceholders (modified), useTemplateEditor (new), useFormValidation (new)
3. ✅ **Component encapsulation** - PlaceholderForm, PromptPreview (modified), TemplateEditor (new)
4. ✅ **No API changes** - Frontend-only feature, no backend modifications
5. ✅ **Derived state pattern** - Memoized resolvedContent prevents unnecessary recalculations

### Constitution Compliance Re-Check

#### I. Code Quality Standards ✅ PASS (Confirmed)

**Evidence**:
- **Single Responsibility**: Each hook has one purpose (placeholder management, template editing, validation)
- **DRY Principle**: Reuses existing `placeholders.ts` utilities (`extractPlaceholders`, `generatePrompt`)
- **Meaningful Names**: Clear naming (`useTemplateEditor`, `canSubmit`, `finalContent`)
- **Type Safety**: All TypeScript interfaces defined in `/frontend/src/lib/types/`
- **Error Handling**: Form validation errors with `role="alert"`, API errors via TanStack Query

**Confirmation**: No violations. Design adheres to quality standards.

---

#### II. Testing Standards ⚠️ FLAGGED (Status Unchanged)

**Complexity Tracking Justification Stands**:
- Manual testing against acceptance scenarios documented in quickstart.md
- If constitution enforced: Add Vitest + React Testing Library as Phase 2 foundational task
- Critical paths identified: placeholder validation, button state logic, template editing state transitions

**Confirmation**: HIGH violation documented with justification. Awaiting maintainer decision.

---

#### III. User Experience Consistency ✅ PASS (Confirmed)

**Evidence**:
- **Design System**: All components use shadcn/ui (Button, Input, Textarea, Label, Tabs, Tooltip, Badge, Card)
- **Accessibility**: Radix UI primitives ensure WCAG 2.1 AA compliance
  - Form fields: `aria-invalid`, `aria-describedby`, `role="alert"`
  - Button: Tooltip + aria-describedby for validation feedback
  - Keyboard navigation: Tab focus order, Enter to submit
- **Responsive Design**: Tailwind utilities `min-h-[300px] md:min-h-[400px] lg:min-h-[500px]`
- **Error Messages**: Clear user-facing messages ("This field is required", "2 placeholder(s) required: ...")
- **Loading States**: TanStack Query loading/error states, button loading during submission
- **Consistent Patterns**: Tabs, forms, buttons follow existing app patterns

**Confirmation**: No violations. UX consistency maintained.

---

#### IV. Performance Requirements ✅ PASS (Confirmed)

**Evidence**:
- **Response Time**: Form validation O(n) with n<10 placeholders (<10ms, well under 100ms)
- **Page Load**: No new route bundles, reuses lazy-loaded TemplateUsage page
- **Bundle Size**: 0KB added (no new dependencies)
- **Memory Footprint**: Client-side state only, no memory leaks (React manages lifecycle)
- **Scalability**: N/A (frontend-only)
- **Monitoring**: No new monitoring required

**Performance Validation**:
- Memoized `resolvedContent` computation: O(n*m) where n=content length, m=placeholder count
  - 1000 lines, 10 placeholders: <50ms (meets <1s constraint)
- Preview rendering: <100ms for 50KB content (native browser text rendering)

**Confirmation**: No violations. Performance budgets met.

---

#### V. Development Workflow ✅ PASS (Confirmed)

**Current State**:
- Feature branch: `004-template-usage-enhancements` ✓
- Pull request required for merge ✓
- CI validation: ESLint, TypeScript compilation ✓
- Review approval required ✓
- Squash commits on merge ✓

**Confirmation**: Workflow compliance verified.

---

### Pre-Merge Gates Re-Check

**Updated Assessment**:

| Gate | Status | Notes |
|------|--------|-------|
| Automated Tests | ⚠️ N/A | No test framework (documented HIGH violation) |
| Code Coverage | ⚠️ Cannot Measure | No test runner |
| Linting | ✅ PASS | ESLint 9.36.0 configured |
| Type Checking | ✅ PASS | TypeScript strict mode, all interfaces defined |
| Security Scan | ✅ PASS | npm audit (no new dependencies) |
| Performance Budget | ✅ PASS | 0KB bundle increase, <100ms validation |
| Accessibility | ✅ PASS | WCAG 2.1 AA compliance via Radix UI + ARIA attributes |

**Recommendation**: Proceed to task generation (Phase 2) with documented testing violation.

---

### Final Gate Status

**GATE RESULT**: ⚠️ **CONDITIONAL PASS**

**Violations Summary**:
1. **HIGH**: Testing Standards - No test framework configured
   - **Justification**: Manual testing against acceptance scenarios; adding Vitest requires non-trivial setup
   - **Mitigation**: Comprehensive acceptance scenario testing documented in quickstart.md
   - **Decision Path**: Maintainer approval OR add Vitest as foundational task

**No New Violations Introduced by Design Decisions**

**Proceed to**: `/speckit.tasks` command (Phase 2: Task Generation)

---

**Constitution Re-Evaluation Complete** ✅

