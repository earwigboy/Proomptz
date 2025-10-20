# Phase 1: Data Model & Entities

**Feature**: Template Usage Screen Enhancements
**Date**: 2025-10-19
**Status**: Complete

## Overview

This feature operates entirely within the React frontend and does not introduce new backend entities or database schema changes. The data model consists of TypeScript interfaces representing component props, hook return types, and internal state structures.

All entities are **transient client-side state** managed by React hooks and components. No persistence layer changes are required.

## Entity Definitions

### 1. PlaceholderInfo

**Description**: Represents metadata and state for a single template placeholder.

**Location**: `/frontend/src/lib/hooks/usePlaceholders.ts`

**Interface**:
```typescript
interface PlaceholderInfo {
  name: string;           // Placeholder identifier (e.g., "project_name")
  value: string;          // User-provided value
  displayName?: string;   // Optional human-readable label
  isRequired: boolean;    // Whether placeholder must have value
  error?: string;         // Validation error message (if any)
  touched: boolean;       // Whether user has interacted with field
}
```

**Validation Rules**:
- `name`: Non-empty string, extracted from template via regex `{{([a-zA-Z_][a-zA-Z0-9_]*)}}`
- `value`: String (may be empty initially)
- `isRequired`: Always `true` in current implementation (all placeholders required)
- `error`: Set when `isRequired && value.trim() === ""`
- `touched`: Set to `true` on first blur/change event

**State Transitions**:
1. **Initial**: `{ value: "", touched: false, error: undefined }`
2. **User types**: `{ value: "...", touched: true, error: undefined }` (if valid)
3. **User clears**: `{ value: "", touched: true, error: "This field is required" }`
4. **User refills**: `{ value: "...", touched: true, error: undefined }`

**Usage**: Managed by `usePlaceholders` hook; drives form field rendering and validation state.

---

### 2. PlaceholderFormState

**Description**: Aggregated state for all placeholders in a template.

**Location**: `/frontend/src/lib/hooks/usePlaceholders.ts`

**Interface**:
```typescript
interface PlaceholderFormState {
  placeholders: Record<string, PlaceholderInfo>; // Map of placeholder name -> info
  isValid: boolean;                              // True if all required fields filled
  errors: Record<string, string>;                // Map of placeholder name -> error message
  touchedFields: Set<string>;                    // Set of placeholder names user has interacted with
}
```

**Derived Properties**:
- `isValid`: Computed as `Object.values(placeholders).every(p => !p.isRequired || p.value.trim() !== "")`
- `errors`: Filtered map of placeholders with non-empty `error` property
- `touchedFields`: Set of placeholder names where `touched === true`

**Invariants**:
- If `isValid === true`, then `errors` is empty
- If `isValid === false`, then at least one placeholder in `placeholders` has `error` set
- `touchedFields` is monotonic (never shrinks, only grows)

**Usage**: Returned by `usePlaceholders` hook; consumed by `PlaceholderForm` component to render fields and by `TemplateUsage` to control button state.

---

### 3. TemplateEditorState

**Description**: State for template content editing functionality.

**Location**: `/frontend/src/lib/hooks/useTemplateEditor.ts` (new file)

**Interface**:
```typescript
interface TemplateEditorState {
  originalContent: string;        // Template content from API
  resolvedContent: string;        // Template with placeholders substituted
  editedContent: string | null;  // User-modified content (null if not edited)
  hasEdits: boolean;             // True if editedContent !== null
  finalContent: string;          // Content to display/send (edited OR resolved)
}
```

**Relationships**:
- `originalContent`: Sourced from `template.content` (API response)
- `resolvedContent`: Computed via `generatePrompt(originalContent, placeholderValues)`
- `editedContent`: User input from textarea in Edit tab (initially `null`)
- `hasEdits`: Computed as `editedContent !== null`
- `finalContent`: Computed as `editedContent ?? resolvedContent`

**State Transitions**:
```
Initial: { editedContent: null, hasEdits: false, finalContent: resolvedContent }
    ↓ (user switches to Edit tab and modifies content)
Edited: { editedContent: "...", hasEdits: true, finalContent: editedContent }
    ↓ (user clicks Reset button)
Reset:  { editedContent: null, hasEdits: false, finalContent: resolvedContent }
```

**Edge Cases**:
- **Placeholder changes after editing**: `editedContent` remains unchanged; user's edits preserved until explicit reset
- **Template changes (navigation)**: Hook re-initializes; `editedContent` cleared (component unmounts/remounts)
- **Empty edit**: If user deletes all content, `editedContent === ""` (not `null`); `hasEdits === true`

**Usage**: Managed by `useTemplateEditor` hook; consumed by `TemplateEditor` and `PromptPreview` components.

---

### 4. ValidationState

**Description**: Validation state for the entire template usage form.

**Location**: `/frontend/src/lib/hooks/useFormValidation.ts` (new file)

**Interface**:
```typescript
interface ValidationState {
  isFormValid: boolean;              // True if all validation passes
  canSubmit: boolean;               // True if form can be submitted
  validationMessage: string | null; // Message explaining why form is invalid
  missingPlaceholders: string[];    // List of placeholder names that are empty
}
```

**Validation Logic**:
```typescript
const isFormValid = placeholders.every(p => p.value.trim() !== "");
const missingPlaceholders = placeholders
  .filter(p => p.value.trim() === "")
  .map(p => p.name);
const canSubmit = isFormValid;
const validationMessage = canSubmit
  ? null
  : `${missingPlaceholders.length} placeholder(s) required: ${missingPlaceholders.join(", ")}`;
```

**Usage**: Consumed by "Send to Devin" button to control disabled state and tooltip message.

---

### 5. TemplateUsagePageState

**Description**: Top-level state for the TemplateUsage page component.

**Location**: `/frontend/src/pages/TemplateUsage.tsx`

**Interface** (conceptual, not a standalone type):
```typescript
// Composed from multiple hooks
const TemplateUsagePage = () => {
  // Template data from API (TanStack Query)
  const { data: template, isLoading, error } = useQuery({ ... });

  // Placeholder form state
  const {
    placeholders,
    isValid: isPlaceholderFormValid,
    updatePlaceholder,
    getPlaceholderValues,
  } = usePlaceholders(template?.content ?? "");

  // Template editor state
  const {
    finalContent,
    hasEdits,
    setEditedContent,
    resetEdits,
  } = useTemplateEditor(template?.content ?? "", getPlaceholderValues());

  // Validation state
  const {
    canSubmit,
    validationMessage,
  } = useFormValidation(placeholders);

  // Send mutation (TanStack Query)
  const sendToDevin = useMutation({ ... });

  // Current tab (Preview vs Edit)
  const [activeTab, setActiveTab] = useState<"preview" | "edit">("preview");

  // ... render logic
};
```

**State Flow**:
1. Template loaded via `useQuery` → `template.content`
2. Placeholders extracted → `usePlaceholders` hook
3. User fills placeholders → `updatePlaceholder` called → `placeholderValues` updated
4. Resolved content computed → `useTemplateEditor` hook → `resolvedContent` updated
5. User switches to Edit tab → modifies content → `setEditedContent` called → `editedContent` updated
6. User clicks "Send to Devin" → `sendToDevin` mutation → sends `finalContent`

**Data Dependencies**:
```
template (API)
  ↓
originalContent
  ↓
extractPlaceholders() → placeholders[]
  ↓
user input → placeholderValues
  ↓
generatePrompt() → resolvedContent
  ↓
user edits (optional) → editedContent
  ↓
finalContent → Send to Devin
```

---

## Entity Relationships

```
┌─────────────────────┐
│  Template (API)     │
│  - id: string       │
│  - content: string  │
│  - name: string     │
└──────────┬──────────┘
           │
           ↓ (extract placeholders)
┌─────────────────────────────────┐
│  PlaceholderInfo[]              │
│  - name: string                 │
│  - value: string (user input)   │
│  - isRequired: boolean          │
│  - error?: string               │
└──────────┬──────────────────────┘
           │
           ↓ (aggregate)
┌─────────────────────────────────┐
│  PlaceholderFormState           │
│  - placeholders: Record<...>    │
│  - isValid: boolean             │
└──────────┬──────────────────────┘
           │
           ↓ (get values)
┌─────────────────────────────────┐
│  placeholderValues              │
│  Record<string, string>         │
└──────────┬──────────────────────┘
           │
           ↓ (generatePrompt)
┌─────────────────────────────────┐
│  TemplateEditorState            │
│  - originalContent: string      │
│  - resolvedContent: string      │
│  - editedContent: string | null │
│  - finalContent: string         │
└──────────┬──────────────────────┘
           │
           ↓ (submit)
┌─────────────────────────────────┐
│  Send to Devin API Call         │
│  POST /api/devin                │
│  body: { content: finalContent }│
└─────────────────────────────────┘
```

---

## Backend Entities (Unchanged)

This feature does **not** modify backend entities. For reference, existing backend entities:

### Template (existing)
- **Source**: `/backend/src/PromptTemplateManager.Core/Models/Template.cs`
- **Fields**: `Id`, `Name`, `Content`, `FolderId`, `CreatedAt`, `UpdatedAt`
- **Persistence**: SQLite database (`prompttemplates.db`)
- **API Endpoint**: `GET /api/templates/{id}` (returns template with content)

**No changes required**: Template content remains stored as-is with placeholder syntax `{{placeholder_name}}`.

---

## Type Definitions (to be created)

### `/frontend/src/lib/types/placeholders.ts` (new file)

```typescript
/**
 * Information about a single template placeholder
 */
export interface PlaceholderInfo {
  /** Placeholder identifier extracted from template (e.g., "project_name") */
  name: string;

  /** User-provided value for this placeholder */
  value: string;

  /** Optional human-readable label for the placeholder */
  displayName?: string;

  /** Whether this placeholder must have a value */
  isRequired: boolean;

  /** Validation error message, if any */
  error?: string;

  /** Whether the user has interacted with this field */
  touched: boolean;
}

/**
 * Aggregated state for all placeholders in a template
 */
export interface PlaceholderFormState {
  /** Map of placeholder name to placeholder info */
  placeholders: Record<string, PlaceholderInfo>;

  /** Whether all required placeholders have valid values */
  isValid: boolean;

  /** Map of placeholder name to error message (only includes fields with errors) */
  errors: Record<string, string>;

  /** Set of placeholder names the user has interacted with */
  touchedFields: Set<string>;
}

/**
 * Return type for usePlaceholders hook
 */
export interface UsePlaceholdersResult {
  /** Array of all placeholder info objects */
  placeholders: PlaceholderInfo[];

  /** Map of placeholder name to current value */
  placeholderValues: Record<string, string>;

  /** Whether all required placeholders are filled */
  isValid: boolean;

  /** Map of placeholder name to error message */
  errors: Record<string, string>;

  /** Update a placeholder's value */
  updatePlaceholder: (name: string, value: string) => void;

  /** Mark a placeholder as touched */
  touchPlaceholder: (name: string) => void;

  /** Get all placeholder values as a map */
  getPlaceholderValues: () => Record<string, string>;

  /** Reset all placeholder values and validation state */
  reset: () => void;
}
```

### `/frontend/src/lib/types/templateEditor.ts` (new file)

```typescript
/**
 * State for template content editing
 */
export interface TemplateEditorState {
  /** Original template content from API */
  originalContent: string;

  /** Template with placeholders substituted */
  resolvedContent: string;

  /** User-modified content (null if not edited) */
  editedContent: string | null;

  /** Whether the user has made edits */
  hasEdits: boolean;

  /** Final content to display/send (edited OR resolved) */
  finalContent: string;
}

/**
 * Return type for useTemplateEditor hook
 */
export interface UseTemplateEditorResult {
  /** Template content with placeholders substituted */
  resolvedContent: string;

  /** Final content (edited if user modified, otherwise resolved) */
  finalContent: string;

  /** Whether user has made edits to the template */
  hasEdits: boolean;

  /** Set edited content (switches to edit mode) */
  setEditedContent: (content: string) => void;

  /** Clear edits and revert to resolved content */
  resetEdits: () => void;

  /** Get content that should be sent to Devin */
  getContentToSend: () => string;
}
```

### `/frontend/src/lib/types/validation.ts` (new file)

```typescript
/**
 * Validation state for template usage form
 */
export interface ValidationState {
  /** Whether the form passes all validation */
  isFormValid: boolean;

  /** Whether the form can be submitted */
  canSubmit: boolean;

  /** Human-readable message explaining validation state */
  validationMessage: string | null;

  /** List of placeholder names that are missing values */
  missingPlaceholders: string[];
}

/**
 * Return type for useFormValidation hook
 */
export interface UseFormValidationResult extends ValidationState {
  /** Validate the current form state */
  validate: () => boolean;
}
```

---

## Migration Notes

**No database migrations required**: This feature operates entirely in the frontend with transient client-side state.

**Backward compatibility**:
- ✅ Existing templates with placeholders work unchanged
- ✅ Placeholder extraction logic reuses existing regex pattern
- ✅ API contracts unchanged (no new endpoints, no modified request/response)
- ✅ Backend code untouched

**Browser storage**: Not used in initial implementation. Future enhancement could use `localStorage` to persist:
- Placeholder values for a template (auto-save draft)
- Edited content (recover after accidental navigation)

---

## Data Flow Summary

```
User Action          │ State Update                    │ Component Re-render
─────────────────────┼─────────────────────────────────┼──────────────────────────
Load template        │ template loaded via useQuery    │ PlaceholderForm, PromptPreview
Fill placeholder     │ updatePlaceholder() called      │ PlaceholderForm (specific field)
                     │ → placeholderValues updated     │ PromptPreview (resolved content)
                     │ → isValid recomputed            │ Send button (enabled state)
─────────────────────┼─────────────────────────────────┼──────────────────────────
Switch to Edit tab   │ activeTab state updated         │ Tabs component
Edit content         │ setEditedContent() called       │ TemplateEditor (textarea)
                     │ → editedContent updated         │ PromptPreview (if on preview tab)
                     │ → hasEdits = true               │ Edit tab badge ("Edited" indicator)
─────────────────────┼─────────────────────────────────┼──────────────────────────
Reset edits          │ resetEdits() called             │ TemplateEditor
                     │ → editedContent = null          │ PromptPreview (reverts to resolved)
                     │ → hasEdits = false              │ Edit tab badge (removed)
─────────────────────┼─────────────────────────────────┼──────────────────────────
Click Send to Devin  │ sendToDevin mutation triggered  │ Button (loading state)
                     │ → API call with finalContent    │ → Toast notification on success
```

---

**Phase 1: Data Model Complete** ✅

**Next Step**: Phase 1 - API Contracts
