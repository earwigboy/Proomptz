# Data Model: shadcn/ui Migration

**Feature**: 002-shadcn-migration
**Date**: 2025-10-19

## Overview

This feature is a **UI component migration** with **no data model changes**. All existing backend entities, database schemas, and API contracts remain unchanged. This document describes the component state models and prop interfaces that will be affected by the migration to shadcn/ui components.

## Backend Data Models

### No Changes

The following backend entities are **unchanged** by this migration:
- `Template` - Template entity with name, content, placeholders, folder relationships
- `Folder` - Folder entity with hierarchical structure
- `Placeholder` - Placeholder definitions within templates
- All database schemas remain identical
- All API request/response models remain identical

**Rationale**: This is purely a frontend UI component migration. The backend continues to serve the same data through the same API contracts.

## Frontend Component State Models

These are the TypeScript interfaces and state management patterns in the frontend components that will be migrated. The **structure** of these interfaces remains the same, but the **rendering** changes from custom components to shadcn components.

### Component Props Interfaces

#### 1. TemplateForm Component

**File**: `frontend/src/components/TemplateForm.tsx`

**Current Interface** (preserved):
```typescript
interface TemplateFormProps {
  template: Template | null;           // null = create mode, Template = edit mode
  selectedFolderId?: string | null;    // pre-selected folder for new templates
  onClose: () => void;                 // callback when form closes
}
```

**Internal State** (preserved):
```typescript
const [name, setName] = useState<string>('');
const [content, setContent] = useState<string>('');
const [folderId, setFolderId] = useState<string | null>(null);
const [error, setError] = useState<string | null>(null);
```

**Migration Impact**:
- Props interface: **No changes** - parent components continue passing same props
- State management: **No changes** - same useState hooks
- Rendering: **Changed** - Replace HTML `<input>`, `<textarea>`, `<select>`, `<button>` with shadcn `Input`, `Textarea`, `Select`, `Button`
- Validation: **Preserved** - Same error state handling, displayed via shadcn error patterns

---

#### 2. FolderDialog Component

**File**: `frontend/src/components/folders/FolderDialog.tsx`

**Current Interface** (preserved):
```typescript
interface FolderDialogProps {
  folder: Folder | null;               // null = create mode, Folder = rename mode
  parentFolderId: string | null;       // parent for new folders
  onSave: (name: string, parentFolderId: string | null) => void;
  onClose: () => void;
  isLoading?: boolean;
}
```

**Internal State** (preserved):
```typescript
const [name, setName] = useState<string>('');
const [error, setError] = useState<string | null>(null);
```

**Migration Impact**:
- Props interface: **No changes**
- State: **No changes**
- Rendering: **Changed** - Replace custom dialog overlay with shadcn `Dialog`, `Input`, `Button`
- Keyboard handling: **Enhanced** - shadcn Dialog provides built-in Escape key handling, focus trapping

---

#### 3. FolderTree Component

**File**: `frontend/src/components/folders/FolderTree.tsx`

**Current Interface** (preserved):
```typescript
interface FolderTreeProps {
  folders: FolderTreeNode[];
  selectedFolderId: string | null;
  onFolderSelect: (folderId: string | null) => void;
  onFolderContextMenu: (folderId: string, event: React.MouseEvent) => void;
  onCreateSubfolder: (parentId: string) => void;
  onTemplateDrop?: (templateId: string, targetFolderId: string | null) => void;
}
```

**Internal State** (per folder item, preserved):
```typescript
const [isExpanded, setExpanded] = useState<boolean>(true);
const [isDragOver, setIsDragOver] = useState<boolean>(false);
```

**Migration Impact**:
- Props interface: **No changes**
- State: **No changes**
- Rendering: **Potentially changed** - May use shadcn `Collapsible` for expand/collapse, or keep custom tree structure with shadcn styling
- Drag-drop: **Preserved** - Native HTML5 drag-drop API continues working with shadcn components
- Keyboard navigation: **Preserved/Enhanced** - Maintain existing keyboard handlers, shadcn may provide additional focus management

---

#### 4. TemplateList Component

**File**: `frontend/src/components/TemplateList.tsx`

**Current Interface** (preserved):
```typescript
interface TemplateListProps {
  onEdit: (template: Template) => void;
  onCreate: () => void;
  selectedFolderId?: string | null;
}
```

**Internal State** (preserved):
```typescript
const [loadingTemplateId, setLoadingTemplateId] = useState<string | null>(null);
```

**Data Fetching** (unchanged):
```typescript
const { data, isLoading, error } = useQuery({
  queryKey: ['templates', selectedFolderId],
  queryFn: () => templatesApi.getAll({ folderId: selectedFolderId }),
});
```

**Migration Impact**:
- Props interface: **No changes**
- State: **No changes**
- Data fetching: **No changes** - TanStack Query hooks unchanged
- Rendering: **Changed** - Replace custom template cards with shadcn `Card`, custom badges with shadcn `Badge`, custom buttons with shadcn `Button`
- Empty state: **Changed** - Use shadcn typography patterns instead of custom CSS

---

#### 5. SearchBar Component

**File**: `frontend/src/components/search/SearchBar.tsx`

**Current Interface** (preserved):
```typescript
interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isSearching?: boolean;
}
```

**Migration Impact**:
- Props interface: **No changes**
- Rendering: **Changed** - Replace custom styled `<input>` with shadcn `Input`, add lucide-react `Search` icon
- Clear button: **Enhanced** - Replace custom button with shadcn `Button` variant

---

#### 6. SearchResults Component

**File**: `frontend/src/components/search/SearchResults.tsx`

**Current Interface** (inferred, to be preserved):
```typescript
interface SearchResultsProps {
  results: Template[];
  isLoading: boolean;
  onTemplateClick: (templateId: string) => void;
}
```

**Migration Impact**:
- Props interface: **No changes**
- Rendering: **Changed** - Use shadcn `Card` for result items, same as TemplateList migration

---

#### 7. PlaceholderForm Component

**File**: `frontend/src/components/placeholders/PlaceholderForm.tsx`

**Current Interface** (inferred, to be preserved):
```typescript
interface PlaceholderFormProps {
  placeholders: Record<string, string>;
  onPlaceholderChange: (name: string, value: string) => void;
}
```

**Migration Impact**:
- Props interface: **No changes**
- Rendering: **Changed** - Replace custom inputs with shadcn `Input`, `Label`

---

#### 8. PromptPreview Component

**File**: `frontend/src/components/placeholders/PromptPreview.tsx`

**Current Interface** (inferred, to be preserved):
```typescript
interface PromptPreviewProps {
  content: string;             // template content with placeholders filled
}
```

**Migration Impact**:
- Props interface: **No changes**
- Rendering: **Changed** - Wrap content in shadcn `Card` for consistent presentation

---

#### 9. FolderContextMenu Component

**File**: `frontend/src/components/folders/FolderContextMenu.tsx`

**Current Interface** (inferred, to be preserved):
```typescript
interface FolderContextMenuProps {
  folderId: string;
  x: number;                   // context menu position
  y: number;
  onRename: () => void;
  onDelete: () => void;
  onCreateSubfolder: () => void;
  onClose: () => void;
}
```

**Migration Impact**:
- Props interface: **No changes**
- Rendering: **Changed** - May replace with shadcn `ContextMenu` or `DropdownMenu` component
- Positioning: **shadcn-managed** - shadcn components handle positioning automatically

---

## State Management

### No Changes to Architecture

The application's state management architecture remains **unchanged**:

- **Server State**: TanStack Query (React Query) - All `useQuery`, `useMutation` hooks unchanged
- **Local Component State**: React `useState` - All component state hooks preserved
- **Form State**: Uncontrolled components with local state - No change to form handling patterns
- **Route State**: React Router - No changes to routing or navigation state

**What Changes**:
- Only the **presentation layer** - HTML elements replaced with shadcn components
- Component styling approach - Custom CSS replaced with Tailwind utilities and shadcn variants

**What Doesn't Change**:
- Data fetching logic
- Mutation logic
- State update patterns
- Event handlers (onChange, onClick, etc.)
- Validation logic
- Error handling logic

---

## Configuration Models

### New: shadcn Configuration

**File**: `frontend/components.json` (new)

This configuration file is created by `npx shadcn@latest init` and defines how shadcn components are generated.

```json
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "default",
  "rsc": false,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.js",
    "css": "src/index.css",
    "baseColor": "slate",
    "cssVariables": true
  },
  "aliases": {
    "components": "@/components",
    "utils": "@/lib/utils"
  }
}
```

**Purpose**:
- Defines component generation paths
- Configures styling approach (Tailwind + CSS variables)
- Sets up import aliases
- Enables TypeScript

---

### New: Tailwind Configuration

**File**: `frontend/tailwind.config.js` (new)

```javascript
/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],
  content: [
    './pages/**/*.{ts,tsx}',
    './components/**/*.{ts,tsx}',
    './app/**/*.{ts,tsx}',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        // ... additional color mappings
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
      keyframes: {
        // ... animation keyframes
      },
      animation: {
        // ... animations
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
}
```

**Purpose**:
- Maps CSS variables to Tailwind utilities
- Defines design tokens (colors, spacing, border radius)
- Enables dark mode with class strategy
- Provides animation utilities

---

## Type Definitions

### No New Backend Types

All backend TypeScript types (generated from OpenAPI spec) remain unchanged:
- `Template`
- `Folder`
- `FolderTreeNode`
- `CreateTemplateRequest`
- `UpdateTemplateRequest`
- etc.

### New Frontend Utility Types

**File**: `frontend/src/lib/utils.ts` (new)

```typescript
import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

**Purpose**:
- Utility function for merging Tailwind classes
- Handles conditional class application
- Resolves class conflicts (twMerge)

**Usage**:
```typescript
<Button className={cn("default-classes", condition && "conditional-classes")} />
```

---

## Validation Rules

### Preserved from Existing Components

All validation logic remains **unchanged**:

**TemplateForm**:
- Name required: `if (!name.trim()) setError('Name is required')`
- Content required: `if (!content.trim()) setError('Content is required')`

**FolderDialog**:
- Name required: `if (!trimmedName) setError('Folder name is required')`
- Length limit: `if (trimmedName.length > 100) setError('Folder name must not exceed 100 characters')`
- Invalid characters: `if (/[/\\:*?"<>|]/.test(trimmedName)) setError('Folder name contains invalid characters')`

**Migration Impact**:
- Validation logic: **No changes** - Same state-based validation
- Error display: **Changed** - Use shadcn error styling patterns instead of custom CSS
- Error patterns: `<Input {...props} aria-invalid={error} />` with associated error message

---

## Summary

| Aspect | Status | Details |
|--------|--------|---------|
| Backend Data Models | ✅ Unchanged | No database, entity, or API contract changes |
| Component Props | ✅ Preserved | All interfaces remain backward compatible |
| Component State | ✅ Preserved | All useState hooks unchanged |
| State Management | ✅ Unchanged | TanStack Query, React state patterns preserved |
| Validation Rules | ✅ Preserved | Same validation logic, different error display |
| Data Fetching | ✅ Unchanged | All useQuery, useMutation hooks identical |
| Event Handlers | ✅ Preserved | Same callback signatures and logic |
| Configuration | ➕ New | components.json, tailwind.config.js for shadcn |
| Utilities | ➕ New | lib/utils.ts for className merging |

**Key Insight**: This migration changes **only the presentation layer**. All data models, business logic, state management, and API interactions remain completely unchanged. The migration is purely a UI component upgrade from custom CSS-styled components to standardized shadcn/ui components.
