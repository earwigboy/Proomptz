# Data Model: Template Usage Enhancements

**Feature**: `005-template-usage-enhancements`
**Date**: 2025-10-21

## Overview

This feature introduces **client-side only** data models for managing UI state. No backend entities or database schema changes are required.

## Client-Side State Entities

### 1. Folder Selection State

**Purpose**: Persist the currently selected folder ID across navigation within a session

**Storage**: URL Search Params (via React Router `useSearchParams`)

**Schema**:
```typescript
// URL format: /?folder={folderId}
interface FolderSelectionState {
  folderId: string | null;  // null represents "no folder selected"
}
```

**Lifecycle**:
- **Created**: When user clicks a folder in the folder tree
- **Persisted**: Stored in URL search params, survives page refresh
- **Cleared**:
  - User navigates away and clears selection
  - Selected folder is deleted (validated via TanStack Query)
  - Browser session ends (window/tab closed)

**Validation Rules**:
- `folderId` must be a valid folder ID from the existing folder tree
- If `folderId` references a deleted folder, auto-clear to `null`
- Empty/invalid folder IDs treated as `null` (no selection)

**State Transitions**:
```
┌──────────────┐
│ No Selection │ (folderId = null)
└──────┬───────┘
       │ User clicks folder
       v
┌──────────────┐
│   Selected   │ (folderId = "abc-123")
└──────┬───────┘
       │
       ├─→ User navigates away and returns → Restored from URL
       ├─→ User clicks different folder → Update URL param
       ├─→ Folder deleted → Auto-clear to null
       └─→ User clears selection → Remove URL param
```

**Integration Points**:
- React Router: `useSearchParams()` hook
- TanStack Query: Validate folder existence via `queryKey: ['folder', folderId]`
- Components: HomePage, FolderTree

---

### 2. Template Content Editor Layout State

**Purpose**: Manage layout configuration for stable two-column grid

**Storage**: CSS-only (no React state required)

**Configuration**:
```typescript
interface EditorLayoutConfig {
  gridTemplateColumns: 'minmax(0, 1fr) minmax(0, 1fr)';
  gap: '2rem';
  minHeight: 0;
  containment: 'layout'; // Applied to placeholder form container
}
```

**Validation Rules**: N/A (static CSS configuration)

**Implementation**: Applied via inline styles or Tailwind classes in TemplateUsage.tsx

---

### 3. Template Content Value State

**Purpose**: Manage textarea content for large templates (up to 10,000 lines)

**Storage**: React component state (existing)

**Schema**:
```typescript
interface TemplateContentState {
  value: string;           // Immediate UI state for controlled component
  debouncedValue: string;  // Debounced state for expensive operations
  lineCount: number;       // Derived from value.split('\n').length
}
```

**Validation Rules**:
- No max length enforcement (handle up to 10,000 lines gracefully)
- `lineCount` computed dynamically for performance warnings
- Debounce window: 300-500ms for API calls/validation

**State Transitions**:
```
User types → Update value immediately (controlled component)
           → Start debounce timer (300ms)
           → Timer expires → Update debouncedValue → Trigger save/validation

User types again before timer expires → Clear previous timer → Start new timer
```

**Performance Optimizations**:
- Disable spellcheck: `spellCheck="false"`
- Disable autocomplete/autocorrect
- Use monospace font for faster rendering
- Debounce expensive operations (not the UI value itself)

---

### 4. Placeholder Form Fields Layout State

**Purpose**: Prevent placeholder form expansion from affecting adjacent editor

**Storage**: CSS-only (no React state required)

**Configuration**:
```typescript
interface PlaceholderFormLayoutConfig {
  containment: 'layout';  // Isolate layout calculations
  overflow: 'auto';       // Required for containment
  resize: 'vertical';     // Textarea resize direction (existing)
}
```

**Validation Rules**: N/A (static CSS configuration)

**Implementation**: Applied to PlaceholderForm container component

---

## Entity Relationships

```
┌─────────────────────────┐
│  Folder Selection State │ (URL Search Params)
└───────────┬─────────────┘
            │
            │ Used to restore selection on HomePage
            v
┌─────────────────────────┐
│      FolderTree         │ (Component)
│   (highlights folder)   │
└─────────────────────────┘

┌─────────────────────────┐
│ Template Content State  │ (React State)
└───────────┬─────────────┘
            │
            │ Rendered in large textarea
            v
┌─────────────────────────┐
│  Editor Layout Config   │ (CSS Grid + Containment)
│  + Placeholder Form     │
└─────────────────────────┘
```

**No relationships between folder selection and template content** - these are independent UI concerns.

---

## Migration Notes

### Existing State to Modify

**File**: `/frontend/src/App.tsx` (Line 46)

**Current**:
```typescript
const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);
```

**Change to**:
```typescript
const [searchParams, setSearchParams] = useSearchParams();
const selectedFolderId = searchParams.get('folder') || null;
```

**Impact**:
- All `setSelectedFolderId` calls become `setSearchParams({ folder: id })` or `setSearchParams({})`
- handleFolderSelect, handleDeleteFolder, and FolderTree component need updates
- No backend API changes required

---

## Data Flow Diagrams

### Folder Selection Flow
```
┌────────────┐
│ User Click │
└─────┬──────┘
      │
      v
┌─────────────────────────┐
│ setSearchParams()       │
│ { folder: "abc-123" }   │
└─────┬───────────────────┘
      │
      ├─→ URL updates: /?folder=abc-123
      │
      ├─→ Component re-renders with new searchParams
      │
      └─→ TanStack Query validates folder exists
          │
          ├─→ Success: Display selected folder
          └─→ Error (404): Auto-clear URL param
```

### Template Content Editing Flow
```
┌─────────────┐
│ User Types  │
└─────┬───────┘
      │
      v
┌─────────────────────────┐
│ onChange(e)             │
│ setValue(e.target.value)│ ← Immediate UI update
└─────┬───────────────────┘
      │
      v
┌─────────────────────────┐
│ Start/Reset Timer       │
│ (300ms debounce)        │
└─────┬───────────────────┘
      │
      v (after 300ms)
┌─────────────────────────┐
│ setDebouncedValue()     │
│ Trigger save/validation │
└─────────────────────────┘
```

---

## Summary

This feature introduces minimal new state:
1. **Folder selection**: Migrated from React `useState` to URL search params
2. **Layout stability**: Pure CSS (no state)
3. **Template content**: Enhanced existing state with debouncing pattern

**No backend data models, API changes, or database migrations required.** All changes are client-side UI/UX improvements.
