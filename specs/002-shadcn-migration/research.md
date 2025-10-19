# Technical Research: shadcn/ui Migration

**Feature**: 002-shadcn-migration
**Date**: 2025-10-19
**Status**: Complete

## Research Questions

### 1. shadcn/ui Installation & Configuration

**Question**: What is the optimal shadcn/ui configuration for a React 19 + Vite + TypeScript project?

**Research Findings**:

shadcn/ui supports two styling approaches:
1. **Tailwind CSS** (default, recommended)
2. **CSS Variables** (alternative for projects without Tailwind)

Given the project currently has no Tailwind configuration, we need to choose an approach.

**Decision**: **Use Tailwind CSS approach**

**Rationale**:
- Tailwind is shadcn's default and most battle-tested configuration
- Provides better tree-shaking and bundle optimization than CSS variables
- Offers utility-first approach that aligns with modern React development
- shadcn components are designed and optimized for Tailwind
- Easier to customize and extend components
- Better TypeScript autocomplete support with Tailwind IntelliSense

**Alternatives Considered**:
- **CSS Variables**: Simpler setup, but less flexible for customization and doesn't leverage shadcn's full potential. Larger runtime overhead for style calculations.
- **No styling framework**: Would require completely custom CSS, defeating the purpose of using shadcn.

**Implementation Details**:
```bash
# Installation command
npx shadcn@latest init

# Configuration selections:
# - Style: Default
# - Base color: Slate (matches existing dark theme)
# - CSS variables: Yes (for theme switching support)
# - Tailwind config: Yes
# - TypeScript: Yes
# - Import alias: @/components
# - React Server Components: No (this is a client-side SPA)
```

**Dependencies to Install**:
- `tailwindcss` (~40KB)
- `postcss` (build tool)
- `autoprefixer` (browser compatibility)
- `@radix-ui/*` (installed per-component by shadcn CLI)
- `class-variance-authority` (~2KB, for component variants)
- `clsx` (~1KB, for conditional classes)
- `tailwind-merge` (~5KB, for class merging)
- `lucide-react` (~50KB for tree-shaken icons)

---

### 2. Dark Theme Configuration

**Question**: How do we configure shadcn to match the existing dark theme color scheme?

**Research Findings**:

The current application uses a dark theme with these key colors (from App.css):
- Background: `#1a1a1a`, `#242424`
- Text: `rgba(255, 255, 255, 0.87)`
- Accent: `#646cff`
- Borders: `#444`

shadcn uses CSS variables for theming, defined in `index.css`:
```css
:root {
  --background: ...;
  --foreground: ...;
  /* etc. */
}

.dark {
  --background: ...;
  --foreground: ...;
  /* etc. */
}
```

**Decision**: **Configure shadcn with Slate base color and customize CSS variables to match existing palette**

**Rationale**:
- Slate base color is closest to existing dark gray theme
- CSS variables allow fine-tuning colors without modifying component code
- Supports dynamic theme switching (future enhancement potential)
- Maintains visual continuity during migration

**Implementation Details**:

Update `index.css` after shadcn init:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    /* Light theme variables */
  }

  .dark {
    --background: 0 0% 10%;           /* ~#1a1a1a */
    --foreground: 0 0% 98%;           /* ~rgba(255,255,255,0.87) */
    --card: 0 0% 14%;                 /* ~#242424 */
    --card-foreground: 0 0% 98%;
    --primary: 232 79% 65%;           /* ~#646cff */
    --primary-foreground: 0 0% 98%;
    --border: 0 0% 27%;               /* ~#444 */
    /* Additional variables */
  }
}

/* Set dark theme as default */
html {
  @apply dark;
}
```

**Alternatives Considered**:
- **Zinc base color**: Too blue-tinted compared to current neutral gray
- **Custom theme from scratch**: More work, no benefit over customizing Slate
- **Light theme**: Current app is dark only; light theme not needed

---

### 3. Icon Library Selection

**Question**: Which icon library should be used with shadcn components (for search icons, expand/collapse, etc.)?

**Research Findings**:

shadcn documentation recommends **lucide-react** as the default icon library.

Alternatives:
- **Heroicons**: React-specific, good DX, ~40KB
- **React Icons**: Wraps multiple icon sets, ~500KB+ (too large)
- **Custom SVG**: No dependency, but requires manual icon management

**Decision**: **Use lucide-react**

**Rationale**:
- Official recommendation from shadcn documentation
- Tree-shakeable (only import icons you use)
- ~50KB for ~20-30 icons typical usage (acceptable)
- Consistent design language across icons
- TypeScript support out of the box
- Simple API: `<Search className="h-4 w-4" />`

**Implementation Details**:
```bash
npm install lucide-react
```

Icons needed for this migration:
- `Search` (SearchBar)
- `X` (close buttons, clear search)
- `ChevronDown`, `ChevronRight` (folder tree expand/collapse)
- `Folder`, `FolderOpen` (folder tree)
- `Loader2` (loading spinner)
- `AlertCircle` (error states)
- `Check` (success states)
- `Plus` (create buttons)

Estimated bundle impact: ~15KB gzipped for these ~10 icons.

**Alternatives Considered**:
- **Heroicons**: Good alternative, but not the shadcn standard
- **Font icons**: Larger bundle, FOUC issues, accessibility concerns
- **Custom SVG components**: Maintenance burden, inconsistent design

---

### 4. Component Migration Strategy

**Question**: What is the safest approach to migrate existing components without breaking the application?

**Research Findings**:

Three approaches considered:
1. **Big Bang**: Replace all components at once
2. **Feature Flag**: Use runtime flags to switch between old/new components
3. **Incremental**: Migrate one component at a time, commit per component

**Decision**: **Incremental migration with feature branch deployment**

**Rationale**:
- Lowest risk - each component can be tested independently
- Easier code review - smaller, focused changesets
- Rollback capability - can revert individual component migrations
- Follows spec's user story priorities (P1 → P2 → P3 → P4)
- Matches project's phased deployment constraint

**Implementation Details**:

Migration order (aligns with user story priorities):
1. **Phase 1 - Core Forms (P1)**:
   - Install shadcn: `npx shadcn@latest init`
   - Add components: `button input textarea select dialog label`
   - Migrate FolderDialog.tsx (smallest, simplest dialog)
   - Migrate TemplateForm.tsx (complex form with all controls)
   - Remove FolderDialog.css
   - Test: Create/edit/delete templates and folders

2. **Phase 2 - Cards & Lists (P2)**:
   - Add components: `card badge`
   - Migrate TemplateList.tsx (cards, badges, buttons)
   - Update App.css (remove custom card styles)
   - Test: View templates, empty states, card interactions

3. **Phase 3 - Navigation (P3)**:
   - Add components: `collapsible` (if needed for tree)
   - Migrate FolderTree.tsx (tree structure, drag-drop)
   - Migrate SearchBar.tsx (input with icon)
   - Migrate SearchResults.tsx (card-based results)
   - Remove FolderTree.css
   - Test: Folder navigation, search, keyboard controls

4. **Phase 4 - Feedback (P4)**:
   - Add components: `alert-dialog alert skeleton sonner`
   - Replace window.confirm with AlertDialog
   - Add Skeleton loading states
   - Add Sonner toast notifications
   - Add Alert for error displays
   - Update App.tsx (add Toaster provider)
   - Test: Confirmations, loading, errors, success messages

**Alternatives Considered**:
- **Big Bang**: Too risky, hard to test, difficult to review
- **Feature Flag**: Added complexity, runtime overhead, not needed for single team
- **Parallel Implementation**: Duplicate code, confusion about which to use

---

### 5. Bundle Size Impact Analysis

**Question**: Will adding shadcn + dependencies exceed the 200KB bundle size target?

**Research Findings**:

**Current Bundle** (from package.json check-size script):
- Target: <200KB gzipped
- Current: ~150-170KB estimated (React 19, React Router, TanStack Query, axios)

**New Dependencies Size Estimates** (tree-shaken, gzipped):
- Tailwind CSS: ~10KB (only used utilities)
- Radix UI primitives: ~25-35KB (Button, Dialog, Select, etc. - 11 components)
- CVA + clsx + tailwind-merge: ~8KB
- lucide-react: ~15KB (10 icons)
- **Total Added**: ~58-68KB

**Projected Bundle After Migration**:
- Current: ~165KB
- Added: ~63KB
- **Projected Total**: ~228KB ⚠️ **EXCEEDS TARGET**

**Mitigation Strategies**:

1. **Remove Custom CSS** (~10-15KB savings):
   - App.css (form styles, card styles)
   - FolderDialog.css
   - FolderTree.css

2. **Code Splitting** (~20-30KB savings for initial load):
   - Lazy load search page: `React.lazy(() => import('./pages/SearchPage'))`
   - Lazy load template use page
   - These aren't on critical path (most users browse templates first)

3. **Optimize Icon Imports** (~5KB savings):
   - Only import specific icons: `import { Search } from 'lucide-react'`
   - Don't import entire library

4. **Verify Tree-Shaking** (ensure no bloat):
   - Check Radix UI tree-shaking works correctly
   - Use `vite build --mode production` and analyze bundle

**Revised Projection with Mitigations**:
- Current: ~165KB
- Added: ~63KB
- Removed CSS: -12KB
- Code splitting initial load: -25KB
- **Projected Initial Bundle**: ~191KB ✅ **UNDER TARGET**

**Decision**: **Proceed with migration + implement all mitigation strategies**

**Rationale**:
- With mitigations, stays under 200KB target
- Code splitting improves initial load time (not just size)
- Removing custom CSS is core migration goal anyway
- Risk is manageable: can optimize further if needed (lazy-load more routes, optimize Radix usage)

**Monitoring Plan**:
- Run `npm run check-size` after each migration phase
- If exceeds 200KB: apply aggressive code splitting
- If still exceeds: consider using fewer shadcn components (e.g., skip Sonner, use simpler toast)

**Alternatives Considered**:
- **CSS Variables approach**: Would reduce Tailwind size but lose benefits, still need Radix UI
- **Abandon migration**: Defeats purpose; custom components have ongoing maintenance cost
- **Increase bundle target**: Not allowed per constitution (200KB is the gate)

---

### 6. TypeScript Path Alias Configuration

**Question**: How should the `@/components/ui` import alias be configured in the existing TypeScript + Vite setup?

**Research Findings**:

shadcn requires the `@/` import alias to be configured so components can be imported as:
```typescript
import { Button } from '@/components/ui/button'
```

**Current Configuration**:
- No path aliases currently configured in tsconfig
- Vite config has no alias resolution

**Decision**: **Add `@/` alias pointing to `src/` directory**

**Rationale**:
- Standard shadcn configuration
- Allows absolute imports (cleaner than relative `../../../../`)
- TypeScript and Vite both need configuration for consistency
- Prevents import errors and IDE autocomplete issues

**Implementation Details**:

Update `tsconfig.app.json`:
```json
{
  "compilerOptions": {
    // ... existing config ...
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

Update `vite.config.ts`:
```typescript
import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  // ... existing build config ...
})
```

**Alternatives Considered**:
- **Relative imports only**: More verbose, harder to refactor, breaks shadcn conventions
- **Different alias** (`~/` or `#/`): Non-standard, confusing for shadcn users
- **No alias**: Would require modifying generated shadcn components (maintenance burden)

---

### 7. Testing Strategy (Deferred Decision)

**Question**: How should testing be handled for the migrated components?

**Research Findings**:

The specification explicitly marks testing as OUT OF SCOPE:
> "Adding unit or integration tests (unless specifically requested later)"

The constitution requires tests, but this is justified in Complexity Tracking as a deferred decision.

**Decision**: **Manual testing via user story acceptance scenarios - automated tests deferred to future work**

**Rationale** (from spec):
- This is a UI component migration preserving existing functionality, not adding new business logic
- No test suite currently exists - would require setting up testing infrastructure first (Vitest, React Testing Library, etc.)
- Manual testing via acceptance scenarios is sufficient for visual/interaction validation
- Adding tests now would delay delivery without preventing regressions (no baseline to regress from)
- Better to validate migration manually, then add test suite in separate feature if desired

**Manual Testing Plan**:

For each user story, validate acceptance scenarios:

**User Story 1 - Core Forms (P1)**:
- Create new template form displays with shadcn components
- Edit template form loads and saves correctly
- Create folder dialog works with shadcn Dialog
- Validation errors display with shadcn error styling
- Loading states show correctly on buttons/inputs

**User Story 2 - Cards & Lists (P2)**:
- Template list renders as shadcn Cards
- Button hover states work correctly
- Empty state displays with shadcn typography
- Badge components show counts correctly

**User Story 3 - Navigation (P3)**:
- Folder tree expands/collapses correctly
- Search input with icon works
- Keyboard navigation functions properly
- Drag-and-drop still works with new components

**User Story 4 - Feedback (P4)**:
- Delete confirmation shows AlertDialog
- Loading shows Skeleton components
- Success toasts appear with Sonner
- Errors display with Alert component

**Future Testing Recommendations** (if requested):
- Setup: Vitest + React Testing Library + Testing Library User Event
- Unit tests: Component rendering, prop passing, event handlers
- Integration tests: Form submission flows, navigation flows
- E2E tests: Playwright for critical user paths
- Accessibility tests: axe-core for WCAG compliance validation

**Alternatives Considered**:
- **Add testing now**: Would add 2-3 days of work to set up infrastructure and write tests. Spec explicitly excludes this.
- **Skip manual testing**: Too risky - need to verify functionality preserved
- **Only visual testing**: Insufficient - need to test interactions, keyboard navigation, etc.

---

## Summary of Decisions

| Decision Area | Choice | Key Rationale |
|---------------|--------|---------------|
| Styling Approach | Tailwind CSS | Default shadcn configuration, best optimization, most flexible |
| Theme Configuration | Slate base + custom CSS variables | Matches existing dark theme, supports future theme switching |
| Icon Library | lucide-react | shadcn recommendation, tree-shakeable, ~15KB for needed icons |
| Migration Strategy | Incremental by user story | Lowest risk, easier review, aligns with spec priorities |
| Bundle Size | Proceed with mitigations | With CSS removal + code splitting, stays under 200KB target |
| Path Alias | @/ → src/ | shadcn standard, enables absolute imports |
| Testing | Manual via acceptance scenarios | Spec explicitly defers automated tests; manual validation sufficient |

## Implementation Readiness

✅ All technical decisions made
✅ Bundle size risk mitigated
✅ Migration strategy defined
✅ Configuration approach clear
✅ No blocking unknowns remain

**Ready to proceed to Phase 1 (Design & Contracts)**
