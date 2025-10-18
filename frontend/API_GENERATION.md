# API Client Generation

This document explains how the TypeScript API client is automatically generated from the backend OpenAPI specification.

## Overview

The frontend uses an **auto-generated TypeScript API client** based on the backend's OpenAPI/Swagger specification. This ensures type safety and keeps the frontend in sync with backend API changes.

## Architecture

```
Backend (C#/.NET)
  └─> Generates swagger.json (via Swashbuckle)
      └─> saved to: shared/openapi/swagger.json
          └─> openapi-typescript-codegen reads it
              └─> Generates TypeScript client in: frontend/src/lib/api/
                  └─> Wrapped by: frontend/src/lib/api-client.ts
```

## Files

- **Generated API** (auto-generated, don't edit):
  - `src/lib/api/` - Auto-generated TypeScript client
  - `src/lib/api/services/` - Service classes (TemplatesService, FoldersService)
  - `src/lib/api/models/` - TypeScript interfaces for all DTOs

- **Wrapper** (manually maintained):
  - `src/lib/api-client.ts` - Wrapper providing a simpler interface
  - Exports: `templatesApi`, `foldersApi`, and all types
  - All application code imports from this file

## How to Regenerate

### Manual Regeneration

When you make changes to the backend API:

1. **Build the backend** (this updates `shared/openapi/swagger.json`):
   ```bash
   cd backend
   dotnet build
   ```

2. **Regenerate the TypeScript client**:
   ```bash
   cd frontend
   npm run generate:api
   ```

3. The generated files in `src/lib/api/` will be updated automatically

### Automatic on Build

The TypeScript client is **automatically regenerated** before every production build:

```bash
npm run build  # Runs "generate:api" first via prebuild hook
```

This ensures the frontend always uses the latest API types in production.

## npm Scripts

- `npm run generate:api` - Manually regenerate the API client from swagger.json
- `npm run build` - Build for production (auto-generates API first)
- `npm run dev` - Start dev server (uses existing generated API)

## Configuration

The generation is configured in `package.json`:

```json
{
  "scripts": {
    "generate:api": "openapi --input ../shared/openapi/swagger.json --output ./src/lib/api --client axios --useOptions --useUnionTypes",
    "prebuild": "npm run generate:api"
  }
}
```

Options explained:
- `--input` - Path to OpenAPI spec (swagger.json)
- `--output` - Where to generate files
- `--client axios` - Use Axios as HTTP client
- `--useOptions` - Use options objects for parameters
- `--useUnionTypes` - Generate union types instead of enums

## Development Workflow

### During Development

While developing, you typically **don't need to regenerate** the API client unless:
- Backend API contracts change (new endpoints, new fields, etc.)
- Backend DTOs are modified

### When Backend Changes

1. Make your backend changes
2. Build the backend: `cd backend && dotnet build`
3. Regenerate frontend API: `cd frontend && npm run generate:api`
4. Check the changes: `git diff src/lib/api/`
5. Update `src/lib/api-client.ts` if the wrapper needs changes
6. Test your changes

## Troubleshooting

### Problem: "Cannot find module './api'"

**Solution**: Run `npm run generate:api` to generate the API client.

### Problem: Type errors after backend changes

**Solution**:
1. Regenerate the API client
2. Check if `api-client.ts` wrapper needs updates
3. Update component code to match new types

### Problem: swagger.json is out of date

**Solution**: Rebuild the backend to regenerate swagger.json:
```bash
cd backend
dotnet build
```

### Problem: Generation fails

**Solution**:
1. Check that `shared/openapi/swagger.json` exists and is valid JSON
2. Ensure `openapi-typescript-codegen` is installed: `npm install`
3. Check the swagger.json is valid OpenAPI 3.0 spec

## Best Practices

1. **Never edit generated files** in `src/lib/api/` - they'll be overwritten
2. **Use the wrapper** (`api-client.ts`) for all API calls in components
3. **Regenerate after backend changes** to catch type errors early
4. **Commit generated files** to git so other developers have them
5. **Review diffs** when regenerating to understand API changes

## What Gets Generated

From the OpenAPI spec, the generator creates:

- **Services**: Classes with methods for each endpoint
  - `TemplatesService.getApiTemplates()` → GET /api/Templates
  - `TemplatesService.postApiTemplates()` → POST /api/Templates
  - etc.

- **Models**: TypeScript interfaces for all DTOs
  - `CreateTemplateRequest`
  - `TemplateResponse`
  - `FolderTreeNode`
  - etc.

- **Core utilities**: HTTP request handling, error types, configuration

## Example Usage

```typescript
import { templatesApi, type Template } from '@/lib/api-client';

// Get all templates
const templates = await templatesApi.getAll();

// Get templates in a folder
const folderTemplates = await templatesApi.getAll({ folderId: 'abc-123' });

// Create a template
const newTemplate = await templatesApi.create({
  name: 'My Template',
  content: 'Template content',
  folderId: null,
});

// Type safety is automatic!
const template: Template = await templatesApi.getById('xyz-789');
console.log(template.name); // TypeScript knows about all fields
```

## CI/CD Integration

In a production CI/CD pipeline, you might:

1. Build backend → generates swagger.json
2. Run frontend `npm run generate:api`
3. Run frontend `npm run build` (also runs generate:api via prebuild)
4. Deploy both

This ensures frontend and backend are always in sync in production.
