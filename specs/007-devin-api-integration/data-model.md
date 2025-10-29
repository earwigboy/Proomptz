# Phase 1: Data Model - Devin API Integration

**Date**: 2025-10-26
**Feature**: Devin API Integration
**Branch**: `007-devin-api-integration`

## Overview

This document defines the data entities, DTOs, and models required for the Devin API integration feature. The models follow the existing layered architecture pattern (Core → Application → Infrastructure).

## Entity Definitions

### Configuration Entities

#### DevinApiOptions

**Location**: `backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiOptions.cs`

**Purpose**: Configuration model for Devin API settings

**Fields**:

| Field | Type | Required | Default | Description | Validation |
|-------|------|----------|---------|-------------|------------|
| `BaseUrl` | `string` | Yes | `"https://api.devin.ai"` | Devin API base URL | Must be valid HTTPS URL |
| `ApiKey` | `string` | Yes | `""` | Devin API authentication key | Non-empty at runtime |
| `TimeoutSeconds` | `int` | No | `30` | HTTP request timeout | 1-300 seconds |

**Configuration Binding**:
```json
{
  "DevinApi": {
    "BaseUrl": "https://api.devin.ai",
    "ApiKey": "",
    "TimeoutSeconds": 30
  }
}
```

**Validation Rules**:
- `ApiKey`: Required for operation, validated on first use
- `BaseUrl`: Must be valid HTTPS URI
- `TimeoutSeconds`: Must be between 1 and 300

---

### Request/Response Models

#### DevinSessionRequest

**Location**: `backend/src/PromptTemplateManager.Infrastructure/DevinIntegration/DevinApiModels.cs`

**Purpose**: HTTP request body for creating a Devin session

**Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Prompt` | `string` | Yes | The generated prompt text to send to Devin |
| `idempotent` | `bool` | No | Enable idempotent session creation |

**JSON Example**:
```json
{
  "prompt": "Create a REST API endpoint for user authentication...",
  idempotent: true
}
```

**Serialization**: System.Text.Json with camelCase naming policy

---


#### DevinSessionResponse

**Location**: `backend/src/PromptTemplateManager.Application/DTOs/DevinSessionResponse.cs`

**Purpose**: Parsed response from Devin API session creation

**Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `SessionId` | `string` | Yes | Unique identifier for the created Devin session |
| `Url` | `string` | Yes | Full HTTPS URL to view the session in browser |
| `IsNewSession` | `boolean` | No | Indicates if a new session was created (only present if idempotent=true) |

**JSON Example**:
```json
{
  "session_id": "devin_session_abc123",
  "session_url": "https://devin.ai/sessions/devin_session_abc123",
  "is_new_session": "true",
}
```

**Deserialization**: System.Text.Json with camelCase naming policy

---

#### DevinErrorResponse

**Location**: `backend/src/PromptTemplateManager.Application/DTOs/DevinErrorResponse.cs`

**Purpose**: Parsed error response from Devin API

**Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Detail` | `string` | Yes | Error details |

**JSON Example**:
```json
{
  "detail": "The provided API key is invalid or has been revoked",
}
```

---

### Application DTOs

#### SendPromptResponse (Existing)

**Location**: `backend/src/PromptTemplateManager.Application/DTOs/SendPromptResponse.cs`

**Purpose**: Response DTO for the `/api/templates/{id}/send` endpoint

**Current Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Success` | `bool` | Yes | Whether the operation succeeded |
| `Message` | `string` | Yes | User-friendly status or error message |
| `DevinResponseId` | `string?` | No | Session ID from Devin (null on error) |

**Updates Required**:
- Add `SessionUrl` field for clickable link
- Update existing usage to populate `SessionUrl` from DevinSessionResponse

**Updated Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Success` | `bool` | Yes | Whether the operation succeeded |
| `Message` | `string` | Yes | User-friendly status or error message |
| `DevinResponseId` | `string?` | No | Session ID from Devin (null on error) |
| `SessionUrl` | `string?` | No | Full HTTPS URL to Devin session (null on error) |

**Example Success Response**:
```json
{
  "success": true,
  "message": "Template sent to Devin successfully",
  "devinResponseId": "devin_session_abc123",
  "sessionUrl": "https://devin.ai/sessions/devin_session_abc123"
}
```

**Example Error Response**:
```json
{
  "success": false,
  "message": "Devin API key is invalid or missing. Please check your configuration.",
  "devinResponseId": null,
  "sessionUrl": null
}
```

---

### Exception Models

#### DevinApiException

**Location**: `backend/src/PromptTemplateManager.Core/Exceptions/DevinApiException.cs`

**Purpose**: Domain exception for Devin API errors

**Fields**:

| Field | Type | Description |
|-------|------|-------------|
| `Message` | `string` | Technical error message (for logging) |
| `UserFriendlyMessage` | `string` | Message to display to user |
| `HttpStatusCode` | `int?` | HTTP status code from API (if applicable) |
| `ErrorCode` | `string?` | Error code from Devin API (if applicable) |
| `InnerException` | `Exception?` | Underlying exception (if any) |

**Constructor**:
```csharp
public DevinApiException(
    string message,
    string userFriendlyMessage,
    Exception? innerException = null,
    int? httpStatusCode = null,
    string? errorCode = null)
```

**Usage Example**:
```csharp
throw new DevinApiException(
    message: "HTTP 401 Unauthorized from Devin API",
    userFriendlyMessage: "Devin API key is invalid or missing. Please check your configuration.",
    httpStatusCode: 401,
    errorCode: "invalid_api_key"
);
```

---

## Data Flow

### Success Flow

```
1. User clicks "Send to Devin" button
   ↓
2. Frontend: POST /api/templates/{id}/send with placeholder values
   ↓
3. TemplatesController generates prompt from template
   ↓
4. TemplatesController calls IDevinClient.SendPromptAsync(prompt)
   ↓
5. DevinClient constructs DevinSessionRequest
   ↓
6. DevinClient sends HTTP POST to Devin API
   ↓
7. Devin API returns 200 OK with DevinSessionResponse
   ↓
8. DevinClient parses response and returns (Success=true, SessionId, SessionUrl)
   ↓
9. TemplatesController maps to SendPromptResponse
   ↓
10. Frontend receives response and displays clickable link
```

### Error Flow

```
1. User clicks "Send to Devin" button
   ↓
2. Frontend: POST /api/templates/{id}/send
   ↓
3. TemplatesController calls IDevinClient.SendPromptAsync(prompt)
   ↓
4. DevinClient sends HTTP POST to Devin API
   ↓
5. Devin API returns 401 Unauthorized
   ↓
6. DevinClient catches HttpRequestException
   ↓
7. DevinClient parses DevinErrorResponse
   ↓
8. DevinClient throws DevinApiException with user-friendly message
   ↓
9. TemplatesController catches DevinApiException
   ↓
10. TemplatesController returns SendPromptResponse with Success=false
   ↓
11. Frontend displays error message in toast notification
```

---

## State Transitions

### Devin Session States (as understood)

```
[NOT CREATED]
    ↓ (User clicks "Send to Devin")
[CREATING] (HTTP request in flight)
    ↓
[CREATED] (Session exists, URL available)
    ↓ (User clicks session URL)
[VIEWING IN BROWSER] (external to application)
```

**Note**: The application only handles the transition from NOT CREATED → CREATING → CREATED. All subsequent session states are managed by Devin AI platform.

---

## Relationships

```
IDevinClient (interface)
    ↓ (implements)
DevinClient (implementation)
    ↓ (depends on)
DevinApiOptions (configuration)
    ↓ (uses)
DevinSessionRequest (request DTO)
    ↓ (sends to Devin API)
DevinSessionResponse (response DTO)
    ↓ (mapped to)
SendPromptResponse (API response DTO)
    ↓ (returned to)
Frontend (React)
```

---

## Validation Rules

### DevinApiOptions Validation

- `ApiKey`: Not null or whitespace at runtime (checked on first use)
- `BaseUrl`: Valid HTTPS URL format
- `TimeoutSeconds`: Range [1, 300]

**Validation Location**: DevinClient constructor or first use

### DevinSessionRequest Validation

- `Prompt`: Not null or empty
- `Prompt`: Maximum length 100,000 characters (arbitrary large limit)

**Validation Location**: DevinClient.SendPromptAsync

### SendPromptResponse Validation

- `Success = true` requires `DevinResponseId` and `SessionUrl` to be non-null
- `Success = false` requires `Message` to be non-empty

**Validation Location**: TemplatesController

---

## Serialization Configuration

### JSON Settings

**System.Text.Json** (default in .NET 9)

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

**Field Naming**:
- C# properties: PascalCase (e.g., `SessionId`)
- JSON fields: camelCase (e.g., `session_id`)
- Handled by `JsonSerializerOptions` configuration

---

## Database Impact

**No database changes required** for this feature.

- Devin session data is NOT persisted locally
- API key stored in configuration, not database
- Prompt generation uses existing Template entities from filesystem

---

## Migration Plan

**No migrations required** - this is a pure addition feature with no schema changes.

---

## Phase 1 Data Model Complete

All entities, DTOs, request/response models, and validation rules have been defined. Ready to proceed to contract generation.
