# API Contracts

**Feature**: Filesystem Template Storage
**Date**: 2025-10-22
**Status**: **NO CHANGES REQUIRED**

---

## Overview

The filesystem storage implementation is a **backend-only change** that swaps the repository layer while maintaining complete API contract compatibility. All existing endpoints, request/response schemas, and behavior remain unchanged.

**Contract Preservation Strategy**:
- ✅ Controllers (TemplatesController, FoldersController) unchanged
- ✅ DTOs (request/response models) unchanged
- ✅ Service interfaces (ITemplateService, IFolderService) unchanged
- ✅ HTTP methods, routes, status codes unchanged
- ✅ Validation rules unchanged
- ✅ Error responses unchanged

---

## Existing API Contracts

### 1. Templates API

**Base Path**: `/api/templates`

#### GET /api/templates

**Purpose**: List all templates with optional pagination

**Request**:
```http
GET /api/templates?page=1&pageSize=50
```

**Query Parameters**:
- `page` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 50): Number of items per page

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Daily Standup",
      "content": "What did you do yesterday?\n\nWhat will you do today?\n\nAny blockers?",
      "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "createdAt": "2025-10-22T10:30:00Z",
      "updatedAt": "2025-10-22T15:45:00Z"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

**Implementation Note**: Pagination is now applied to in-memory list after loading all templates from filesystem (acceptable for <10k templates per SC-004).

---

#### GET /api/templates/{id}

**Purpose**: Get single template by ID

**Request**:
```http
GET /api/templates/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Daily Standup",
  "content": "What did you do yesterday?\n\nWhat will you do today?\n\nAny blockers?",
  "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "createdAt": "2025-10-22T10:30:00Z",
  "updatedAt": "2025-10-22T15:45:00Z"
}
```

**Response** (404 Not Found):
```json
{
  "error": "Template not found",
  "templateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Implementation Note**: ID is now matched against YAML frontmatter `id` field, not database primary key.

---

#### POST /api/templates

**Purpose**: Create new template

**Request**:
```http
POST /api/templates
Content-Type: application/json

{
  "name": "Meeting Notes",
  "content": "# Meeting Notes\n\n**Date**: {{date}}\n**Attendees**: {{attendees}}",
  "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Request Body Schema**:
```typescript
interface CreateTemplateRequest {
  name: string;        // Required, 1-200 chars
  content: string;     // Required
  folderId?: string;   // Optional UUID, null = root level
}
```

**Response** (201 Created):
```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "name": "Meeting Notes",
  "content": "# Meeting Notes\n\n**Date**: {{date}}\n**Attendees**: {{attendees}}",
  "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "createdAt": "2025-10-22T16:20:00Z",
  "updatedAt": "2025-10-22T16:20:00Z"
}
```

**Response** (400 Bad Request) - Duplicate Name:
```json
{
  "error": "Template with this name already exists in the specified folder"
}
```

**Response** (400 Bad Request) - Invalid Folder:
```json
{
  "error": "Folder not found",
  "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Implementation Note**:
- ID auto-generated as GUID
- Timestamps auto-set to current UTC time
- File created at `/data/templates/{folder-path}/{sanitized-name}.md`
- Lucene index updated with new document

---

#### PUT /api/templates/{id}

**Purpose**: Update existing template

**Request**:
```http
PUT /api/templates/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "name": "Daily Standup (Updated)",
  "content": "New content here",
  "folderId": null
}
```

**Request Body Schema**: Same as CreateTemplateRequest

**Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Daily Standup (Updated)",
  "content": "New content here",
  "folderId": null,
  "createdAt": "2025-10-22T10:30:00Z",
  "updatedAt": "2025-10-22T16:45:00Z"
}
```

**Response** (404 Not Found): Same as GET /{id}

**Implementation Note**:
- `CreatedAt` preserved, `UpdatedAt` refreshed
- If name changed: file renamed
- If folder changed: file moved to new directory
- If both changed: file moved and renamed
- Old file deleted, new file created (atomic operation via temp file)
- Lucene index document updated

---

#### DELETE /api/templates/{id}

**Purpose**: Delete template

**Request**:
```http
DELETE /api/templates/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response** (204 No Content): Empty body

**Response** (404 Not Found): Same as GET /{id}

**Implementation Note**:
- File deleted from filesystem
- Lucene index document deleted
- Cache invalidated

---

### 2. Folders API

**Base Path**: `/api/folders`

#### GET /api/folders/tree

**Purpose**: Get hierarchical folder tree

**Request**:
```http
GET /api/folders/tree
```

**Response** (200 OK):
```json
[
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Work",
    "parentFolderId": null,
    "createdAt": "2025-10-22T09:00:00Z",
    "updatedAt": "2025-10-22T09:00:00Z",
    "children": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "name": "Meetings",
        "parentFolderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        "createdAt": "2025-10-22T09:15:00Z",
        "updatedAt": "2025-10-22T09:15:00Z",
        "children": []
      }
    ]
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "name": "Personal",
    "parentFolderId": null,
    "createdAt": "2025-10-22T09:30:00Z",
    "updatedAt": "2025-10-22T09:30:00Z",
    "children": []
  }
]
```

**Implementation Note**:
- Tree built by scanning `/data/templates/` directory structure
- Each directory's `.folder-meta` file provides ID and timestamps
- Recursive tree construction (same algorithm as current FolderService.BuildTreeNode)

---

#### GET /api/folders/{id}

**Purpose**: Get single folder by ID

**Request**:
```http
GET /api/folders/7c9e6679-7425-40de-944b-e07fc1f90ae7
```

**Response** (200 OK):
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Work",
  "parentFolderId": null,
  "createdAt": "2025-10-22T09:00:00Z",
  "updatedAt": "2025-10-22T09:00:00Z"
}
```

**Response** (404 Not Found):
```json
{
  "error": "Folder not found",
  "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Implementation Note**: ID matched against `.folder-meta` files, not database.

---

#### POST /api/folders

**Purpose**: Create new folder

**Request**:
```http
POST /api/folders
Content-Type: application/json

{
  "name": "Quarterly Reviews",
  "parentFolderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Request Body Schema**:
```typescript
interface CreateFolderRequest {
  name: string;          // Required, 1-100 chars
  parentFolderId?: string; // Optional UUID, null = root level
}
```

**Response** (201 Created):
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "name": "Quarterly Reviews",
  "parentFolderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "createdAt": "2025-10-22T17:00:00Z",
  "updatedAt": "2025-10-22T17:00:00Z"
}
```

**Response** (400 Bad Request) - Max Depth Exceeded:
```json
{
  "error": "Maximum folder depth (5 levels) exceeded"
}
```

**Response** (400 Bad Request) - Duplicate Name:
```json
{
  "error": "Folder with this name already exists at this level"
}
```

**Implementation Note**:
- Directory created at `/data/templates/{parent-path}/{sanitized-name}/`
- `.folder-meta` file created with YAML metadata
- Depth validation enforced before creation

---

#### PUT /api/folders/{id}

**Purpose**: Update folder (rename or move)

**Request**:
```http
PUT /api/folders/7c9e6679-7425-40de-944b-e07fc1f90ae7
Content-Type: application/json

{
  "name": "Work Projects",
  "parentFolderId": null
}
```

**Request Body Schema**: Same as CreateFolderRequest

**Response** (200 OK):
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Work Projects",
  "parentFolderId": null,
  "createdAt": "2025-10-22T09:00:00Z",
  "updatedAt": "2025-10-22T17:15:00Z"
}
```

**Response** (400 Bad Request) - Circular Reference:
```json
{
  "error": "Cannot move folder to its own descendant (circular reference)"
}
```

**Implementation Note**:
- If name changed: directory renamed
- If parent changed: directory moved
- If both: directory moved and renamed
- All contained templates and subfolders move automatically (filesystem behavior)
- Circular reference check before move operation

---

#### DELETE /api/folders/{id}

**Purpose**: Delete empty folder

**Request**:
```http
DELETE /api/folders/7c9e6679-7425-40de-944b-e07fc1f90ae7
```

**Response** (204 No Content): Empty body

**Response** (400 Bad Request) - Folder Not Empty:
```json
{
  "error": "Cannot delete folder that contains templates or subfolders"
}
```

**Response** (404 Not Found): Same as GET /{id}

**Implementation Note**:
- Check directory is empty before deletion
- Delete directory and `.folder-meta` file
- Templates must be moved or deleted separately before folder deletion

---

### 3. Search API

**Base Path**: `/api/search`

#### GET /api/search/templates

**Purpose**: Full-text search across template names and content

**Request**:
```http
GET /api/search/templates?query=meeting+notes&page=1&pageSize=20
```

**Query Parameters**:
- `query` (required): Search terms (Lucene query syntax)
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Results per page

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Meeting Notes Template",
      "content": "# Meeting Notes\n\n**Date**: {{date}}...",
      "folderId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "createdAt": "2025-10-22T10:30:00Z",
      "updatedAt": "2025-10-22T15:45:00Z",
      "score": 0.85
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

**Implementation Note**:
- Search powered by Lucene.NET instead of SQLite FTS5
- Scoring algorithm: TF-IDF (Lucene default)
- Results ranked by relevance score
- Performance target: <2s for 10k templates (SC-004)

---

## OpenAPI Specification

**Location**: `/shared/openapi/swagger.json`

**Status**: **NO CHANGES REQUIRED**

The existing OpenAPI specification accurately describes all endpoints and schemas. The filesystem implementation is transparent to API consumers.

**Auto-Generated Client**: Frontend TypeScript client regenerated from swagger.json remains compatible.

---

## Breaking Changes

**None**. All API contracts remain 100% backward compatible.

---

## Testing Strategy

**Contract Tests**:
- Verify response schemas match OpenAPI specification
- Verify HTTP status codes for success/error cases
- Verify request validation rules
- Compare responses before/after filesystem migration (should be identical)

**Test Framework**: xUnit with FluentAssertions (see research.md)

**Sample Contract Test**:
```csharp
[Fact]
public async Task CreateTemplate_ValidRequest_Returns201WithCorrectSchema()
{
    // Arrange
    var request = new CreateTemplateRequest
    {
        Name = "Test Template",
        Content = "Test content"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/templates", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var template = await response.Content.ReadFromJsonAsync<TemplateDto>();
    template.Should().NotBeNull();
    template!.Id.Should().NotBeEmpty();
    template.Name.Should().Be(request.Name);
    template.Content.Should().Be(request.Content);
    template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    template.UpdatedAt.Should().Be(template.CreatedAt);
}
```

---

## Summary

**API Contract Strategy**: **Zero Changes**

All existing API endpoints, request/response schemas, validation rules, and error handling remain unchanged. The filesystem storage layer is a drop-in replacement for the SQLite repository layer, enabled by the existing clean architecture with repository pattern.

**Frontend Impact**: None (API contracts preserved)
**Client Code Changes**: None (auto-generated client remains compatible)
**Postman/Integration Tests**: No updates required (same endpoints, same behavior)

**Next Document**: quickstart.md (integration scenarios)
