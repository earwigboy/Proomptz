# API Contracts: shadcn/ui Migration

**Feature**: 002-shadcn-migration
**Date**: 2025-10-19

## Overview

This feature involves **NO API CONTRACT CHANGES**.

The shadcn/ui migration is a frontend-only UI component upgrade. All backend API endpoints, request/response models, and data contracts remain completely unchanged.

## Unchanged API Contracts

The following APIs continue to be consumed by the frontend exactly as before:

### Templates API
- `GET /api/templates` - List all templates (with optional folderId filter)
- `GET /api/templates/{id}` - Get template by ID
- `POST /api/templates` - Create new template
- `PUT /api/templates/{id}` - Update existing template
- `DELETE /api/templates/{id}` - Delete template

### Folders API
- `GET /api/folders` - Get folder tree
- `GET /api/folders/{id}` - Get folder details
- `POST /api/folders` - Create new folder
- `PUT /api/folders/{id}` - Rename folder
- `DELETE /api/folders/{id}` - Delete folder
- `PUT /api/templates/{templateId}/move` - Move template to folder (drag-drop)

### Search API
- `GET /api/search?q={query}` - Full-text search templates

## Frontend Consumption

### No Changes to API Client

The auto-generated OpenAPI client remains unchanged:
- **Location**: `frontend/src/lib/api/`
- **Generation**: `npm run generate:api` (from `shared/openapi/swagger.json`)
- **Usage**: Imported by components via `import { templatesApi } from '../lib/api-client'`

### No Changes to Query Hooks

TanStack Query (React Query) hooks remain unchanged:
```typescript
// Example: TemplateList component
const { data, isLoading, error } = useQuery({
  queryKey: ['templates', selectedFolderId],
  queryFn: () => templatesApi.getAll({ folderId: selectedFolderId }),
});
```

### No Changes to Mutations

Mutation hooks for create/update/delete operations remain unchanged:
```typescript
// Example: TemplateForm component
const createMutation = useMutation({
  mutationFn: (data: CreateTemplateRequest) => templatesApi.create(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['templates'] });
    onClose();
  },
});
```

## Migration Impact

| API Aspect | Status | Notes |
|------------|--------|-------|
| Endpoints | ✅ Unchanged | All URLs, methods, parameters unchanged |
| Request Models | ✅ Unchanged | All request DTOs unchanged |
| Response Models | ✅ Unchanged | All response DTOs unchanged |
| Error Responses | ✅ Unchanged | Error handling logic preserved |
| Authentication | ✅ Unchanged | No auth changes (if applicable) |
| Headers | ✅ Unchanged | All request headers preserved |
| Query Parameters | ✅ Unchanged | Filter, pagination unchanged |

## Component Integration

### API Call Sites Preserved

All components that call APIs maintain the same integration pattern:

**TemplateForm**:
- Calls `templatesApi.create()` on new template submission ✅
- Calls `templatesApi.update()` on edit template submission ✅
- Invalidates `['templates']` query cache on success ✅

**TemplateList**:
- Calls `templatesApi.getAll()` with optional `folderId` filter ✅
- Calls `templatesApi.delete()` on delete action ✅
- Displays error states from query errors ✅

**FolderTree**:
- Calls `foldersApi.getTree()` to load folder structure ✅
- Calls `templatesApi.move()` on drag-drop ✅

**SearchBar/SearchResults**:
- Calls `searchApi.search(query)` on debounced input ✅

### Error Handling Preserved

All API error handling remains unchanged:
- Network errors: Display via error state, now styled with shadcn `Alert` component
- Validation errors: Display via form error state, now styled with shadcn error patterns
- Success notifications: Now displayed via shadcn `Toast` (Sonner) instead of custom UI

## Backend Compatibility

This migration maintains **100% backward compatibility** with the existing backend:
- Backend continues serving same OpenAPI spec
- No database migrations required
- No API version changes
- No breaking changes to contracts

## Testing API Integration

Manual testing will verify API integration unchanged:

1. **Template CRUD**:
   - Create template → POST /api/templates ✅
   - Edit template → PUT /api/templates/{id} ✅
   - Delete template → DELETE /api/templates/{id} ✅
   - List templates → GET /api/templates ✅

2. **Folder Operations**:
   - Create folder → POST /api/folders ✅
   - Rename folder → PUT /api/folders/{id} ✅
   - Delete folder → DELETE /api/folders/{id} ✅
   - Move template → PUT /api/templates/{id}/move ✅

3. **Search**:
   - Search templates → GET /api/search?q={query} ✅

4. **Error Scenarios**:
   - Network failure → Error state displayed
   - 400 validation error → Error message displayed
   - 404 not found → Error message displayed
   - 500 server error → Error message displayed

All scenarios should work identically before and after migration, with only the visual presentation of errors and success messages changing (now using shadcn components).

## Summary

**No contract artifacts to generate** - This directory exists for framework compliance but contains no actual contract definitions because:
1. This is a UI-only migration
2. All API contracts defined in `shared/openapi/swagger.json` (unchanged)
3. Frontend auto-generates client from OpenAPI spec (unchanged)
4. No new endpoints, request models, or response models

**Migration Principle**: "UI components change, API contracts don't"
