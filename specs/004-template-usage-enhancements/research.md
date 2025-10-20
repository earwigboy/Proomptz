# Phase 0: Technical Research & Decisions

**Feature**: Template Usage Screen Enhancements
**Date**: 2025-10-19
**Status**: Complete

## Research Questions

### 1. Form Validation Library Selection

**Question**: Should we use React Hook Form for enhanced placeholder form validation, or implement custom validation with React state?

**Research Findings**:

**Option A: React Hook Form**
- **Pros**:
  - Battle-tested library (7.3M+ weekly downloads)
  - Built-in validation, error handling, and form state management
  - Excellent TypeScript support
  - Minimal re-renders (uncontrolled components by default)
  - Integrates well with shadcn/ui components
  - Accessible form validation out of the box
- **Cons**:
  - Additional dependency (~35KB gzipped)
  - Learning curve for team if not familiar
  - May be overkill for simple placeholder form

**Option B: Custom React State Validation**
- **Pros**:
  - No additional dependencies
  - Full control over validation logic
  - Simpler for basic use case (check if all placeholders filled)
  - Team already familiar with React state patterns
- **Cons**:
  - Must manually implement validation state tracking
  - Need to handle error messages, touched fields, etc.
  - More boilerplate code
  - Accessibility features need manual implementation

**Decision**: **Custom React State Validation**

**Rationale**:
1. Validation requirements are simple: check if required placeholders have non-empty values
2. Existing `usePlaceholders` hook can be extended with validation state
3. No complex validation rules (regex, async validation, etc.)
4. Avoids 35KB bundle impact for minimal functionality gain
5. Maintains consistency with existing codebase patterns (no React Hook Form usage detected)
6. Faster implementation for this specific use case

**Implementation Notes**:
- Extend `/frontend/src/lib/hooks/usePlaceholders.ts` with `isValid` computed state
- Add `errors` map for per-placeholder validation messages
- Use native HTML5 validation attributes (`required`, `minLength`) for accessibility
- Leverage shadcn/ui `Label` and form field patterns for error display

**Alternatives Considered**:
- Formik: Heavier than React Hook Form, declining popularity
- Zod + React Hook Form: Overkill for simple presence validation
- Plain uncontrolled components: Loses reactivity needed for button state

---

### 2. Template Editor Component Strategy

**Question**: What's the best approach for implementing editable template content - inline editing in preview, separate editor component, or modal dialog?

**Research Findings**:

**Option A: Inline Editing (Preview becomes editable)**
- **Pros**:
  - Seamless user experience (no mode switching)
  - Live preview while editing
  - Minimal UI changes
- **Cons**:
  - Harder to distinguish "preview" vs "edit" mode
  - Risk of accidental edits
  - Complex state management (edit vs. view mode)
  - Difficult to implement "revert" clearly

**Option B: Separate Editor Component (Tabs or side-by-side)**
- **Pros**:
  - Clear separation of preview and edit modes
  - Easy to show original vs. edited state
  - Simple revert functionality (clear edit state)
  - Can use tab component for mode switching
- **Cons**:
  - Requires more screen space
  - User must switch between tabs to see preview
  - More complex layout

**Option C: Modal Dialog Editor**
- **Pros**:
  - Focused editing experience (no distractions)
  - Clear entry/exit points
  - Can show before/after side-by-side
  - Easy to cancel without saving
- **Cons**:
  - Interrupts workflow (modal takes over screen)
  - Cannot see placeholder form while editing
  - Extra click to open modal
  - Not ideal for frequent small edits

**Decision**: **Separate Editor Component with Tab UI**

**Rationale**:
1. Meets spec requirement: "possible to edit the template contents prior to sending"
2. Clear visual distinction between preview and edit modes (addresses User Story 3, Scenario 4)
3. Easy to implement revert/reset functionality (User Story 3, Scenario 5)
4. Can use shadcn/ui Tabs component (already in project)
5. Supports larger preview requirement (User Story 4) - both preview and edit panes can be sized appropriately
6. No modal interruption to workflow
7. Maintains context (placeholder form still visible)

**Implementation Notes**:
- Use shadcn/ui `Tabs` component with "Preview" and "Edit" tabs
- Edit tab contains `<Textarea>` with resolved template content
- Preview tab shows read-only rendered content
- Maintain edit state in `useTemplateEditor` hook
- Show visual indicator (badge/icon) when content has been edited
- "Reset" button in Edit tab clears edit state

**Alternatives Considered**:
- ContentEditable div: Complex, accessibility issues, no clear boundary between preview/edit
- Split pane (side-by-side): Screen real estate issues, especially on smaller viewports
- Toggle button (same component switches mode): Confusing UX, state management complexity

---

### 3. Preview Box Sizing Strategy

**Question**: How should the preview box be enlarged while maintaining responsive design and not overwhelming the page layout?

**Research Findings**:

**Current State Analysis**:
- Need to examine current TemplateUsage.tsx layout
- Likely using CSS Grid or Flexbox for layout
- Preview probably has fixed height or max-height constraint
- Mobile/tablet responsiveness must be maintained

**Sizing Approaches**:

**Option A: Increase Fixed Height**
- **Pros**: Simple CSS change
- **Cons**: Not responsive, may cause scrolling issues

**Option B: Percentage-Based Height (e.g., 60vh)**
- **Pros**:
  - Responsive to viewport
  - Scales automatically
  - Easy to implement
- **Cons**:
  - May be too large on small screens
  - May be too small on large screens

**Option C: Dynamic Height Based on Content**
- **Pros**:
  - Shows as much content as possible
  - No unnecessary scrolling for short templates
- **Cons**:
  - Can cause layout shift
  - May push other elements off screen
  - Hard to limit maximum size

**Option D: Flex-Based Layout with Priority**
- **Pros**:
  - Preview gets remaining space after form
  - Responsive by design
  - Works across device sizes
  - Can set min/max constraints
- **Cons**:
  - Requires layout refactoring
  - More complex CSS

**Decision**: **Flex-Based Layout with Minimum Height**

**Rationale**:
1. Meets spec requirement: "preview area takes up significantly more screen real estate" (User Story 4, Scenario 1)
2. Responsive design requirement: "scales appropriately to maximize visibility" (User Story 4, Scenario 3)
3. Handles both short and long templates gracefully
4. Provides at least 50% more visible lines (Success Criteria SC-004)
5. Constitution requires responsive design 320px-4K viewports

**Implementation Notes**:
- Use CSS Flexbox column layout for page
- Placeholder form: `flex-shrink: 0` (fixed size based on content)
- Preview/editor area: `flex: 1` (takes remaining space)
- Set `min-height: 400px` on desktop, `min-height: 300px` on mobile
- Add `overflow-y: auto` for scrolling within preview when content exceeds viewport
- Use Tailwind responsive utilities: `min-h-[300px] md:min-h-[400px] lg:min-h-[500px]`
- Test on 320px (mobile), 768px (tablet), 1920px (desktop) viewports

**Success Metric**: User Story 4, SC-004: "Preview box displays at least 50% more visible lines"
- Current: Estimate ~15-20 lines visible (need to measure current implementation)
- Target: 25-30+ lines visible on standard 1080p desktop viewport

**Alternatives Considered**:
- CSS Grid: Similar result but less flexible for dynamic content sizes
- Modal full-screen preview: Removes context, interrupts workflow
- Collapsible placeholder form: Hidden form makes editing harder

---

### 4. Accessibility Implementation for Form Validation

**Question**: How do we ensure WCAG 2.1 AA compliance for form validation, especially error states and button disabled feedback?

**Research Findings**:

**WCAG Requirements**:
- **1.3.1 Info and Relationships**: Form fields must have programmatic labels
- **3.3.1 Error Identification**: Errors must be clearly identified in text
- **3.3.2 Labels or Instructions**: Form fields must have clear labels/instructions
- **3.3.3 Error Suggestion**: Error messages should suggest corrections
- **4.1.3 Status Messages**: Dynamic updates must use ARIA live regions

**Best Practices for Disabled Button**:
- **Option A**: Add `aria-label` explaining why disabled
- **Option B**: Use tooltip (Radix Tooltip) on hover/focus
- **Option C**: Show static text above/below button explaining state
- **Option D**: Keep button enabled but show validation errors on click

**Decision**: **Combination of Tooltip + aria-describedby**

**Rationale**:
1. Tooltip provides immediate feedback on hover/focus (visual users)
2. `aria-describedby` connects button to validation message (screen readers)
3. Radix UI Tooltip already available (no new dependency)
4. Meets WCAG 2.1 AA requirements for status messages
5. Non-intrusive (doesn't clutter UI with permanent text)

**Implementation for Placeholder Form Validation**:

```typescript
// Each input field structure
<div>
  <Label htmlFor="placeholder-name">Placeholder Name</Label>
  <Input
    id="placeholder-name"
    value={value}
    onChange={handleChange}
    required
    aria-invalid={hasError}
    aria-describedby={hasError ? "error-placeholder-name" : undefined}
  />
  {hasError && (
    <p id="error-placeholder-name" className="text-destructive text-sm" role="alert">
      This field is required
    </p>
  )}
</div>
```

**Implementation for Send to Devin Button**:

```typescript
<Tooltip>
  <TooltipTrigger asChild>
    <Button
      disabled={!isValid}
      aria-describedby={!isValid ? "send-button-status" : undefined}
    >
      Send to Devin
    </Button>
  </TooltipTrigger>
  <TooltipContent>
    {!isValid ? "Fill all required placeholders to enable" : "Send template to Devin"}
  </TooltipContent>
</Tooltip>
{!isValid && (
  <p id="send-button-status" className="text-sm text-muted-foreground sr-only" aria-live="polite">
    Button disabled: {missingPlaceholders.length} placeholder(s) required
  </p>
)}
```

**Testing Approach**:
- Manual keyboard navigation testing (tab through form, reach button)
- Screen reader testing (NVDA/JAWS on Windows, VoiceOver on macOS)
- Axe DevTools automated scan
- Color contrast validation for error messages (4.5:1 minimum)

**Alternatives Considered**:
- Always-enabled button with error dialog on click: Frustrating UX, doesn't prevent submission attempt
- Static warning text: Clutters UI, not dynamic
- ARIA live region only: Misses visual tooltip for sighted users

---

### 5. State Management for Template Editing

**Question**: How should we manage the state for original template, resolved template (with placeholders), and user-edited content?

**Research Findings**:

**State Requirements**:
1. Original template content (from API)
2. Placeholder values (user input)
3. Resolved template (original + substituted placeholders)
4. Edited template content (user modifications after resolution)
5. "Has been edited" flag
6. Reset functionality

**Option A: Single State Object**
```typescript
const [templateState, setTemplateState] = useState({
  original: "",
  placeholderValues: {},
  editedContent: null,
});
```

**Option B: Separate State Variables**
```typescript
const [originalContent] = useState("");
const [placeholderValues, setPlaceholderValues] = useState({});
const [editedContent, setEditedContent] = useState<string | null>(null);
```

**Option C: Custom Hook with Derived State**
```typescript
const useTemplateEditor = (template, placeholders) => {
  const [editedContent, setEditedContent] = useState<string | null>(null);
  const resolvedContent = useMemo(() =>
    generatePrompt(template.content, placeholders),
    [template, placeholders]
  );
  const finalContent = editedContent ?? resolvedContent;
  const hasEdits = editedContent !== null;
  // ...
};
```

**Decision**: **Custom Hook with Derived State (Option C)**

**Rationale**:
1. Encapsulates complex state logic in reusable hook
2. Automatic recomputation of resolved content when placeholders change
3. Clear separation of concerns (hook manages state, components consume)
4. Easy to test (hook can be tested independently)
5. Matches existing pattern (`usePlaceholders` hook already exists)
6. Prevents state synchronization bugs (derived state always correct)

**Hook Interface**:

```typescript
// frontend/src/lib/hooks/useTemplateEditor.ts
interface UseTemplateEditorResult {
  // Content values
  resolvedContent: string;      // Template with placeholders substituted
  finalContent: string;         // Edited content OR resolved content
  hasEdits: boolean;           // True if user has modified content

  // Actions
  setEditedContent: (content: string) => void;
  resetEdits: () => void;

  // For sending to Devin
  getContentToSend: () => string;
}

const useTemplateEditor = (
  templateContent: string,
  placeholderValues: Record<string, string>
): UseTemplateEditorResult => {
  const [editedContent, setEditedContent] = useState<string | null>(null);

  const resolvedContent = useMemo(
    () => generatePrompt(templateContent, placeholderValues),
    [templateContent, placeholderValues]
  );

  const finalContent = editedContent ?? resolvedContent;
  const hasEdits = editedContent !== null;

  const resetEdits = useCallback(() => {
    setEditedContent(null);
  }, []);

  const getContentToSend = useCallback(() => finalContent, [finalContent]);

  return {
    resolvedContent,
    finalContent,
    hasEdits,
    setEditedContent,
    resetEdits,
    getContentToSend,
  };
};
```

**Behavior**:
- When placeholders change, `resolvedContent` auto-updates
- If user has NOT edited (`editedContent === null`), `finalContent` follows `resolvedContent`
- If user HAS edited (`editedContent !== null`), `finalContent` is frozen to user's version
- Reset clears `editedContent`, causing `finalContent` to revert to current `resolvedContent`

**Edge Case Handling**:
- **User edits, then changes placeholder**: Edited content remains (user's changes preserved)
- **User fills placeholder after editing**: Placeholder change doesn't affect edited content
- **User resets**: Edited content cleared, resolved content shown with current placeholder values

**Alternatives Considered**:
- Redux/Zustand: Overkill for local component state
- Context API: Unnecessary, state doesn't need to be shared beyond TemplateUsage page
- Direct state in component: Too complex, harder to test

---

## Research Summary

### Technology Decisions

| Question | Decision | Key Rationale |
|----------|----------|---------------|
| Form validation library | Custom React state validation | Simple requirements; avoid 35KB dependency; matches existing patterns |
| Template editor UI pattern | Separate tabs (Preview/Edit) | Clear mode distinction; easy revert; uses existing shadcn/ui Tabs |
| Preview box sizing | Flex-based layout with min-height | Responsive 320px-4K; handles variable content; meets 50%+ size increase |
| Accessibility strategy | Tooltip + aria-describedby + role=alert | WCAG 2.1 AA compliant; screen reader + visual feedback |
| State management | Custom hook with derived state | Encapsulated logic; automatic updates; matches existing patterns |

### Dependencies

**No New NPM Packages Required**:
- All functionality achievable with existing dependencies:
  - React 19.1.1 (hooks, state)
  - shadcn/ui 3.4.2 (Tabs, Tooltip, Textarea, Button, Label, Input)
  - Radix UI (accessibility primitives)
  - Tailwind CSS 3.4.18 (responsive layout)
  - Existing `/frontend/src/lib/utils/placeholders.ts` utilities

**Avoided Dependencies**:
- ❌ React Hook Form (~35KB) - not needed for simple validation
- ❌ Zod (~15KB) - presence validation doesn't need schema library
- ❌ react-contenteditable - accessibility and complexity issues

### Performance Validation

**Bundle Impact**: ~0KB (no new dependencies)
**Render Performance**:
- Form validation: O(n) where n = placeholder count (typically <10)
- Memoized `resolvedContent` prevents unnecessary regeneration
- Tab switching: No performance impact (React conditional rendering)

**Meets Constitution Performance Requirements**:
- ✅ Form validation <100ms (synchronous JavaScript)
- ✅ Placeholder substitution <1s (existing `generatePrompt` utility)
- ✅ Bundle size <200KB (no new dependencies added)
- ✅ First Contentful Paint <1.5s (no lazy loading changes)

### Testing Strategy

**Manual Testing** (per Complexity Tracking decision):
- Test all acceptance scenarios in spec.md (User Stories 1-4)
- Keyboard navigation testing (accessibility)
- Screen reader testing (NVDA, VoiceOver)
- Responsive design testing (320px, 768px, 1920px viewports)
- Cross-browser testing (Chrome, Firefox, Safari, Edge)

**If Test Framework Added** (optional per constitution):
- Unit tests for `useTemplateEditor` hook (state transitions, edge cases)
- Unit tests for validation logic in `usePlaceholders`
- Integration tests for PlaceholderForm component (fill fields, validation)
- Integration tests for TemplateEditor component (edit, reset, tab switching)

### Risks Identified

| Risk | Mitigation Strategy |
|------|---------------------|
| User accidentally edits template and doesn't realize | Clear "Edited" badge on Edit tab; Reset button prominently displayed |
| Preview box too large on small screens | Responsive min-height (300px mobile, 400px desktop); test on actual devices |
| Form validation not accessible to screen readers | Use `role="alert"`, `aria-describedby`, `aria-invalid`; test with screen readers |
| Placeholder changes after editing causes confusion | Document behavior: edits are preserved until reset; show warning in UI |
| Performance regression with many placeholders | Memoize resolved content; validate with 20+ placeholder template |

### Open Questions (Resolved)

All research questions resolved. No clarifications needed before proceeding to Phase 1 design.

---

**Phase 0 Complete** ✅

**Next Step**: Phase 1 - Data Model & Contracts
