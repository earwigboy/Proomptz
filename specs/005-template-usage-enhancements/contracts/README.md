# API Contracts: Template Usage Enhancements

**Feature**: `005-template-usage-enhancements`
**Date**: 2025-10-21

## Overview

This feature requires **NO API changes** to the backend. All functionality is implemented client-side using existing endpoints.

## Existing API Endpoints Used

This feature leverages the following existing endpoints without modification:

### 1. Folders API

**Endpoint**: `GET /api/Folders/tree`
**Purpose**: Retrieve folder tree for validation of selected folder existence
**Contract**: Unchanged (defined in existing OpenAPI spec)
**Used by**: Folder selection persistence hook for validating folder IDs

**Endpoint**: `GET /api/Folders/{id}`
**Purpose**: Validate a specific folder exists (implicit via TanStack Query error handling)
**Contract**: Unchanged (defined in existing OpenAPI spec)
**Used by**: TanStack Query validation in folder selection persistence

### 2. Templates API

**Endpoint**: `GET /api/Templates/{id}`
**Purpose**: Retrieve template content for editing
**Contract**: Unchanged (defined in existing OpenAPI spec)
**Used by**: Template content text area

**Endpoint**: `GET /api/Templates/{id}/placeholders`
**Purpose**: Extract placeholders from template for form generation
**Contract**: Unchanged (defined in existing OpenAPI spec)
**Used by**: Placeholder form fields

## Frontend-Only Changes

All changes are implemented via:
- React Router URL search params (folder selection persistence)
- CSS layout modifications (grid + containment)
- React component state (textarea value management)

## API Client

The existing auto-generated TypeScript API client (`frontend/src/lib/api/`) remains unchanged.

**Generation command**: `npm run generate:api` (no re-generation needed for this feature)

---

## No New Contracts

Since this is a pure UI/UX enhancement feature with no backend changes:
- ✅ No new REST endpoints
- ✅ No new request/response schemas
- ✅ No OpenAPI spec modifications
- ✅ No GraphQL schema changes
- ✅ No WebSocket contracts

The existing API contract defined in `/shared/openapi/swagger.json` is sufficient.
