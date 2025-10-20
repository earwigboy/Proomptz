# Component & Hook Contracts

**Feature**: Template Usage Screen Enhancements
**Date**: 2025-10-19
**Status**: Complete

## Overview

This document defines the TypeScript interfaces (contracts) for all React components and hooks in this feature. These contracts serve as the API specification for component composition and integration.

**Note**: This feature has no REST API changes. All contracts are TypeScript interfaces for frontend components and hooks.

---

## Hook Contracts

### 1. usePlaceholders

**Purpose**: Manage placeholder extraction, values, and validation state for a template.

**Location**: `/frontend/src/lib/hooks/usePlaceholders.ts`

**Signature**:
```typescript
function usePlaceholders(templateContent: string): UsePlaceholdersResult
```

**Input**:
```typescript
templateContent: string  // Template content with {{placeholder}} syntax
```

**Output**:
```typescript
interface UsePlaceholdersResult {
  /** Array of placeholder metadata and current values */
  placeholders: PlaceholderInfo[];

  /** Map of placeholder name → value for easy lookup */
  placeholderValues: Record<string, string>;

  /** Whether all required placeholders have non-empty values */
  isValid: boolean;

  /** Map of placeholder name → error message (only placeholders with errors) */
  errors: Record<string, string>;

  /** Update the value for a specific placeholder */
  updatePlaceholder: (name: string, value: string) => void;

  /** Mark a placeholder as "touched" (user has interacted) */
  touchPlaceholder: (name: string) => void;

  /** Get all placeholder values as a Record */
  getPlaceholderValues: () => Record<string, string>;

  /** Reset all placeholders to empty and clear touched state */
  reset: () => void;
}

interface PlaceholderInfo {
  name: string;          // e.g., "project_name"
  value: string;         // User input
  displayName?: string;  // Optional human-readable label
  isRequired: boolean;   // All placeholders required in v1
  error?: string;        // Validation error message
  touched: boolean;      // Has user interacted with field?
}
```

**Behavior**:
- Extracts placeholders from `templateContent` using regex `/\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}/g`
- Initializes each placeholder with `{ value: "", touched: false, isRequired: true }`
- Recomputes `isValid` on every `updatePlaceholder` call
- Sets `error` when `isRequired && value.trim() === ""`
- `touchPlaceholder` sets `touched: true` (triggered on blur events)

**Example Usage**:
```typescript
const { placeholders, isValid, updatePlaceholder } = usePlaceholders(template.content);

// Render form fields
{placeholders.map(p => (
  <Input
    value={p.value}
    onChange={(e) => updatePlaceholder(p.name, e.target.value)}
  />
))}

// Check validation before submit
<Button disabled={!isValid}>Send to Devin</Button>
```

---

### 2. useTemplateEditor

**Purpose**: Manage template content editing state and derived content (resolved vs. edited).

**Location**: `/frontend/src/lib/hooks/useTemplateEditor.ts` (new file)

**Signature**:
```typescript
function useTemplateEditor(
  templateContent: string,
  placeholderValues: Record<string, string>
): UseTemplateEditorResult
```

**Input**:
```typescript
templateContent: string                 // Original template from API
placeholderValues: Record<string, string>  // Current placeholder values
```

**Output**:
```typescript
interface UseTemplateEditorResult {
  /** Template with placeholders substituted (computed via generatePrompt) */
  resolvedContent: string;

  /** Final content to display/send (edited content OR resolved content) */
  finalContent: string;

  /** Whether user has modified the resolved content */
  hasEdits: boolean;

  /** Set edited content (user modifies in Edit tab) */
  setEditedContent: (content: string) => void;

  /** Clear edited content and revert to resolved */
  resetEdits: () => void;

  /** Get content to send to Devin (alias for finalContent) */
  getContentToSend: () => string;
}
```

**Behavior**:
- `resolvedContent` is **memoized** and recomputed when `templateContent` or `placeholderValues` change
- `editedContent` is internal state (nullable string)
- `finalContent = editedContent ?? resolvedContent` (edited takes precedence)
- `hasEdits = editedContent !== null`
- `resetEdits()` sets `editedContent = null`
- **Edge case**: If user edits, then changes placeholder values, `editedContent` is **not** automatically updated (user's edits preserved until reset)

**Example Usage**:
```typescript
const { finalContent, hasEdits, setEditedContent, resetEdits } = useTemplateEditor(
  template.content,
  getPlaceholderValues()
);

// Edit tab textarea
<Textarea
  value={finalContent}
  onChange={(e) => setEditedContent(e.target.value)}
/>

// Reset button
<Button onClick={resetEdits}>Reset to Original</Button>

// Preview tab
<pre>{finalContent}</pre>

// Send to Devin
sendToDevin({ content: getContentToSend() });
```

---

### 3. useFormValidation

**Purpose**: Validate template usage form and provide user-facing validation messages.

**Location**: `/frontend/src/lib/hooks/useFormValidation.ts` (new file)

**Signature**:
```typescript
function useFormValidation(placeholders: PlaceholderInfo[]): UseFormValidationResult
```

**Input**:
```typescript
placeholders: PlaceholderInfo[]  // Array of placeholders from usePlaceholders
```

**Output**:
```typescript
interface UseFormValidationResult {
  /** Whether form passes all validation checks */
  isFormValid: boolean;

  /** Whether form can be submitted (same as isFormValid in v1) */
  canSubmit: boolean;

  /** Human-readable validation message (null if valid) */
  validationMessage: string | null;

  /** Array of placeholder names that are missing values */
  missingPlaceholders: string[];

  /** Trigger validation and return result */
  validate: () => boolean;
}
```

**Behavior**:
- `isFormValid = placeholders.every(p => p.value.trim() !== "")`
- `canSubmit = isFormValid`
- `missingPlaceholders = placeholders.filter(p => p.value.trim() === "").map(p => p.name)`
- `validationMessage`:
  - If valid: `null`
  - If invalid: `"${count} placeholder(s) required: ${names.join(", ")}"`
- `validate()` re-runs validation and returns boolean (useful for explicit validation on submit)

**Example Usage**:
```typescript
const { canSubmit, validationMessage, missingPlaceholders } = useFormValidation(placeholders);

// Button with tooltip
<Tooltip>
  <TooltipTrigger asChild>
    <Button disabled={!canSubmit}>Send to Devin</Button>
  </TooltipTrigger>
  <TooltipContent>
    {validationMessage ?? "Send template to Devin"}
  </TooltipContent>
</Tooltip>

// Validation feedback
{!canSubmit && (
  <p className="text-sm text-destructive">
    Missing: {missingPlaceholders.join(", ")}
  </p>
)}
```

---

## Component Contracts

### 4. PlaceholderForm (Modified)

**Purpose**: Render form fields for each placeholder with validation and labels.

**Location**: `/frontend/src/components/placeholders/PlaceholderForm.tsx`

**Props**:
```typescript
interface PlaceholderFormProps {
  /** Array of placeholders with current values and validation state */
  placeholders: PlaceholderInfo[];

  /** Callback when placeholder value changes */
  onPlaceholderChange: (name: string, value: string) => void;

  /** Callback when placeholder field is blurred (for touched state) */
  onPlaceholderBlur?: (name: string) => void;

  /** Whether form is currently disabled (e.g., during submission) */
  disabled?: boolean;

  /** Optional CSS class for container */
  className?: string;
}
```

**Emitted Events**:
- `onPlaceholderChange(name, value)`: Triggered on every input change
- `onPlaceholderBlur(name)`: Triggered when field loses focus

**Rendering Logic**:
- For each placeholder:
  - If `value.length < 50` (estimated): Render `<Input>` (single-line text)
  - If `value.length >= 50` or `displayName` contains "description": Render `<Textarea>` (multi-line)
- Each field includes:
  - `<Label>` with `displayName` or `name`
  - Required indicator (`*`) for required fields
  - Error message below field (shown only if `touched && error`)
  - ARIA attributes: `aria-invalid`, `aria-describedby`, `required`

**Example Usage**:
```typescript
<PlaceholderForm
  placeholders={placeholders}
  onPlaceholderChange={updatePlaceholder}
  onPlaceholderBlur={touchPlaceholder}
  disabled={isSubmitting}
/>
```

**Accessibility**:
- Each input has associated `<Label>` with `htmlFor` attribute
- Error messages use `role="alert"` for screen reader announcements
- Invalid fields have `aria-invalid="true"`
- Error messages linked via `aria-describedby`

---

### 5. PromptPreview (Modified)

**Purpose**: Display template content (resolved or edited) with larger preview area.

**Location**: `/frontend/src/components/placeholders/PromptPreview.tsx`

**Props**:
```typescript
interface PromptPreviewProps {
  /** Content to display (resolved or edited template) */
  content: string;

  /** Whether to show loading skeleton */
  isLoading?: boolean;

  /** Whether content has been edited by user */
  hasEdits?: boolean;

  /** Optional CSS class for container */
  className?: string;

  /** Title for preview section */
  title?: string;
}
```

**Rendering Logic**:
- Container uses flexbox: `flex: 1` to take remaining vertical space
- Minimum height: `min-h-[300px] md:min-h-[400px] lg:min-h-[500px]`
- Scrollable content: `overflow-y-auto`
- Shows "Edited" badge if `hasEdits === true`
- Displays content in `<pre>` tag with monospace font
- Preserves whitespace and line breaks

**Example Usage**:
```typescript
<PromptPreview
  content={finalContent}
  hasEdits={hasEdits}
  title="Template Preview"
  className="flex-1"
/>
```

---

### 6. TemplateEditor (New Component)

**Purpose**: Provide editable textarea for modifying template content with reset functionality.

**Location**: `/frontend/src/components/template/TemplateEditor.tsx` (new file)

**Props**:
```typescript
interface TemplateEditorProps {
  /** Current content to edit */
  content: string;

  /** Callback when content changes */
  onContentChange: (content: string) => void;

  /** Whether content has been edited */
  hasEdits: boolean;

  /** Callback to reset content to original */
  onReset: () => void;

  /** Whether editor is disabled */
  disabled?: boolean;

  /** Optional CSS class for container */
  className?: string;
}
```

**Emitted Events**:
- `onContentChange(content)`: Triggered on every textarea change
- `onReset()`: Triggered when Reset button clicked

**Rendering Logic**:
- `<Textarea>` with monospace font
- Minimum height: `min-h-[300px] md:min-h-[400px]`
- Auto-growing height (or fixed with scroll)
- "Reset to Original" button (only shown if `hasEdits === true`)
- Button uses shadcn/ui Button with variant "outline"

**Example Usage**:
```typescript
<TemplateEditor
  content={finalContent}
  onContentChange={setEditedContent}
  hasEdits={hasEdits}
  onReset={resetEdits}
/>
```

**Accessibility**:
- Textarea has `aria-label="Edit template content"`
- Reset button has descriptive text and tooltip
- Keyboard accessible (tab to textarea, Ctrl+A to select all, tab to Reset button)

---

### 7. TemplateUsage (Modified Page)

**Purpose**: Orchestrate all components and hooks for the template usage workflow.

**Location**: `/frontend/src/pages/TemplateUsage.tsx`

**Props**: None (uses route params via `useParams()`)

**Route**: `/use/:id`

**State Composition**:
```typescript
const TemplateUsage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  // Fetch template
  const { data: template, isLoading } = useQuery({
    queryKey: ["template", id],
    queryFn: () => TemplatesService.getTemplate(id),
  });

  // Placeholder management
  const {
    placeholders,
    isValid,
    updatePlaceholder,
    touchPlaceholder,
    getPlaceholderValues,
  } = usePlaceholders(template?.content ?? "");

  // Template editing
  const {
    finalContent,
    hasEdits,
    setEditedContent,
    resetEdits,
    getContentToSend,
  } = useTemplateEditor(template?.content ?? "", getPlaceholderValues());

  // Form validation
  const {
    canSubmit,
    validationMessage,
  } = useFormValidation(placeholders);

  // Send to Devin mutation
  const sendMutation = useMutation({
    mutationFn: (content: string) => DevinService.send(content),
    onSuccess: () => toast.success("Sent to Devin!"),
  });

  // Tab state
  const [activeTab, setActiveTab] = useState<"preview" | "edit">("preview");

  return (
    <div className="flex flex-col h-full">
      {/* Placeholder Form */}
      <PlaceholderForm
        placeholders={placeholders}
        onPlaceholderChange={updatePlaceholder}
        onPlaceholderBlur={touchPlaceholder}
      />

      {/* Tabs: Preview / Edit */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1 flex flex-col">
        <TabsList>
          <TabsTrigger value="preview">Preview</TabsTrigger>
          <TabsTrigger value="edit">
            Edit {hasEdits && <Badge variant="secondary">Edited</Badge>}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="preview" className="flex-1">
          <PromptPreview content={finalContent} hasEdits={hasEdits} />
        </TabsContent>

        <TabsContent value="edit" className="flex-1">
          <TemplateEditor
            content={finalContent}
            onContentChange={setEditedContent}
            hasEdits={hasEdits}
            onReset={resetEdits}
          />
        </TabsContent>
      </Tabs>

      {/* Send Button */}
      <div className="mt-4">
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              disabled={!canSubmit}
              onClick={() => sendMutation.mutate(getContentToSend())}
            >
              Send to Devin
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            {validationMessage ?? "Send template to Devin"}
          </TooltipContent>
        </Tooltip>
      </div>
    </div>
  );
};
```

**Layout Structure**:
```
┌─────────────────────────────────────┐
│  PlaceholderForm                    │ ← Fixed height (based on placeholders)
│  (flex-shrink: 0)                   │
├─────────────────────────────────────┤
│  Tabs (Preview / Edit)              │ ← Flex: 1 (takes remaining space)
│  ┌─────────────────────────────┐   │
│  │ Preview OR Edit Content     │   │   min-h-[300px] md:min-h-[400px]
│  │ (scrollable if needed)      │   │
│  │                             │   │
│  └─────────────────────────────┘   │
├─────────────────────────────────────┤
│  Send to Devin Button               │ ← Fixed height
│  (with tooltip)                     │
└─────────────────────────────────────┘
```

---

## Type Exports

All types should be exported from a central types file for reusability:

**Location**: `/frontend/src/lib/types/index.ts`

```typescript
// Re-export all types
export type {
  PlaceholderInfo,
  PlaceholderFormState,
  UsePlaceholdersResult,
} from "./placeholders";

export type {
  TemplateEditorState,
  UseTemplateEditorResult,
} from "./templateEditor";

export type {
  ValidationState,
  UseFormValidationResult,
} from "./validation";

export type {
  PlaceholderFormProps,
} from "../components/placeholders/PlaceholderForm";

export type {
  PromptPreviewProps,
} from "../components/placeholders/PromptPreview";

export type {
  TemplateEditorProps,
} from "../components/template/TemplateEditor";
```

---

## API Integration (Existing - No Changes)

### GET /api/templates/:id

**Request**:
```
GET /api/templates/abc-123
```

**Response**:
```json
{
  "id": "abc-123",
  "name": "Feature Specification Template",
  "content": "# Feature: {{feature_name}}\n\n## Description\n{{description}}\n\n## Requirements\n{{requirements}}",
  "folderId": "folder-123",
  "createdAt": "2025-10-15T10:00:00Z",
  "updatedAt": "2025-10-18T14:30:00Z"
}
```

**Used by**: `useQuery` in TemplateUsage page

---

### POST /api/devin (hypothetical - actual endpoint TBD)

**Request**:
```json
{
  "content": "# Feature: User Authentication\n\n## Description\nImplement OAuth2 login...\n\n## Requirements\n- Support Google and GitHub providers..."
}
```

**Response**:
```json
{
  "success": true,
  "message": "Template sent to Devin successfully"
}
```

**Used by**: `useMutation` for "Send to Devin" action

**Note**: Actual Devin API integration details TBD. This contract assumes a simple POST with content payload.

---

## Testing Contracts

**If test framework added** (per constitution discussion):

### usePlaceholders Tests

```typescript
describe("usePlaceholders", () => {
  it("extracts placeholders from template content", () => {
    const { result } = renderHook(() =>
      usePlaceholders("Hello {{name}}, welcome to {{project}}")
    );
    expect(result.current.placeholders).toHaveLength(2);
    expect(result.current.placeholders[0].name).toBe("name");
    expect(result.current.placeholders[1].name).toBe("project");
  });

  it("marks form as invalid when placeholders are empty", () => {
    const { result } = renderHook(() => usePlaceholders("{{field}}"));
    expect(result.current.isValid).toBe(false);
  });

  it("marks form as valid when all placeholders filled", () => {
    const { result } = renderHook(() => usePlaceholders("{{field}}"));
    act(() => result.current.updatePlaceholder("field", "value"));
    expect(result.current.isValid).toBe(true);
  });
});
```

### useTemplateEditor Tests

```typescript
describe("useTemplateEditor", () => {
  it("resolves template with placeholder values", () => {
    const { result } = renderHook(() =>
      useTemplateEditor("Hello {{name}}", { name: "Alice" })
    );
    expect(result.current.resolvedContent).toBe("Hello Alice");
    expect(result.current.finalContent).toBe("Hello Alice");
  });

  it("preserves edited content over resolved content", () => {
    const { result } = renderHook(() =>
      useTemplateEditor("Hello {{name}}", { name: "Alice" })
    );
    act(() => result.current.setEditedContent("Custom content"));
    expect(result.current.finalContent).toBe("Custom content");
    expect(result.current.hasEdits).toBe(true);
  });

  it("resets edited content to resolved on reset", () => {
    const { result } = renderHook(() =>
      useTemplateEditor("Hello {{name}}", { name: "Alice" })
    );
    act(() => result.current.setEditedContent("Custom"));
    act(() => result.current.resetEdits());
    expect(result.current.finalContent).toBe("Hello Alice");
    expect(result.current.hasEdits).toBe(false);
  });
});
```

---

## Contract Validation Checklist

- ✅ All hook signatures defined with input/output types
- ✅ All component props interfaces defined
- ✅ Event callbacks clearly documented
- ✅ Accessibility attributes specified
- ✅ Example usage provided for each contract
- ✅ Type exports centralized
- ✅ API integration contracts documented (no changes required)
- ✅ Testing contracts defined (conditional on test framework)

---

**Phase 1: Contracts Complete** ✅

**Next Step**: Phase 1 - Quickstart Scenarios
