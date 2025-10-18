# API Contracts

This directory contains the OpenAPI specification for the Prompt Template Manager API.

## Files

- **openapi.yaml**: OpenAPI 3.0.3 specification defining all REST endpoints, request/response schemas, and error responses

## API Overview

### Base URL

`http://localhost:5000/api`

### Endpoints Summary

| Group | Endpoint | Method | Description |
|-------|----------|--------|-------------|
| **Templates** | `/templates` | GET | List templates (paginated, filterable by folder) |
| | `/templates` | POST | Create new template |
| | `/templates/{id}` | GET | Get template details |
| | `/templates/{id}` | PUT | Update template |
| | `/templates/{id}` | DELETE | Delete template |
| **Placeholders** | `/templates/{id}/placeholders` | GET | Extract placeholders from template |
| | `/templates/{id}/generate` | POST | Generate prompt with placeholder substitution |
| | `/templates/{id}/send` | POST | Send generated prompt to Devin LLM |
| **Folders** | `/folders` | GET | Get complete folder tree |
| | `/folders` | POST | Create new folder |
| | `/folders/{id}` | GET | Get folder details with children |
| | `/folders/{id}` | PUT | Update folder (rename or move) |
| | `/folders/{id}` | DELETE | Delete folder (only if empty) |
| **Search** | `/search` | GET | Full-text search across templates |

### Authentication

None (local single-user application)

### Content Type

All endpoints accept and return `application/json`

### Error Handling

Standard HTTP status codes with consistent error response format:

```json
{
  "error": "ErrorType",
  "message": "Human-readable error message",
  "details": ["Additional context if available"]
}
```

**Status Codes**:
- `200 OK`: Successful GET/PUT
- `201 Created`: Successful POST
- `204 No Content`: Successful DELETE
- `400 Bad Request`: Validation errors, malformed input
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate name, circular reference
- `502 Bad Gateway`: External service (Devin LLM) error

## TypeScript Client Generation

Frontend consumes this spec via **openapi-typescript-codegen**:

```bash
# From frontend directory
npm run generate:api
```

This generates:
- Type-safe API client functions
- Request/response TypeScript interfaces
- Enums for tags and error types

**Generated Location**: `frontend/src/lib/api/`

## Validation

The spec can be validated using:

```bash
# Install OpenAPI tools
npm install -g @stoplight/spectral-cli

# Validate spec
spectral lint contracts/openapi.yaml
```

## Contract Testing

API implementation will be validated against this spec using:

- **Backend**: Swashbuckle generates spec from C# controllers (compared against this canonical spec)
- **Frontend**: Generated TypeScript client ensures compile-time contract adherence
- **Runtime**: Optional integration tests with `schemathesis` or `dredd`

## Key Design Decisions

### Pagination

- Default: 50 items per page
- Max: 200 items per page
- 1-based page numbering (page=1 is first page)
- Response includes `hasMore` flag for infinite scroll UX

### Identifiers

- UUIDs (Guid) for all entity IDs
- Represented as strings in JSON (RFC 4122 format)

### Timestamps

- ISO 8601 format with UTC timezone
- Example: `2025-10-18T14:30:00Z`

### Nullable Fields

- `folderId`: null represents root-level (no parent folder)
- `parentFolderId`: null represents root-level folder
- Optional fields marked with `nullable: true` in schema

### Placeholder Substitution

- Two-step process:
  1. `GET /templates/{id}/placeholders` - Discover required inputs
  2. `POST /templates/{id}/generate` - Substitute values and generate prompt
- Alternative: `POST /templates/{id}/send` - Generate and send to Devin in one call

## Changelog

### v1.0.0 (2025-10-18)

- Initial API specification
- Templates CRUD endpoints
- Folders hierarchy endpoints
- Placeholder extraction and prompt generation
- Full-text search
- Devin LLM integration placeholder
