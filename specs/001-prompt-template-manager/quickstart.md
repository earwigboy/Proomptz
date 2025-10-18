# Quickstart Guide: Prompt Template Manager

**Feature**: 001-prompt-template-manager
**Date**: 2025-10-18
**Phase**: Phase 1 - Integration Scenarios

## Purpose

This document provides end-to-end integration scenarios that demonstrate how the frontend and backend work together to implement each user story. These scenarios serve as acceptance tests and implementation guides.

## Prerequisites

- Backend API running on `http://localhost:5000`
- Frontend UI running on `http://localhost:5173`
- Empty SQLite database (clean state for testing)

## Scenario 1: Create and Edit a Template (User Story 1 - MVP)

**Goal**: User creates a new template, edits it, views it, and deletes it.

### Step 1: Create a Template

**User Action**: Click "New Template" button, fill form, save

**Frontend**:
```typescript
// POST /api/templates
const response = await apiClient.createTemplate({
  name: "Bug Fix Request",
  content: "# Bug Fix\n\n**Issue**: {{issue_description}}\n**Priority**: {{priority}}",
  folderId: null  // Root level
});
// Response: TemplateResponse with id, timestamps
```

**Backend Flow**:
1. TemplatesController.CreateTemplate() receives request
2. Validates: name 1-200 chars, content not null
3. Checks uniqueness: no existing template with same name in root
4. TemplateService creates entity with generated Guid
5. Repository saves to SQLite `Templates` table
6. Returns TemplateResponse DTO

**Database State**:
```sql
-- Templates table
INSERT INTO Templates (Id, Name, Content, FolderId, CreatedAt, UpdatedAt)
VALUES ('a1b2c3...', 'Bug Fix Request', '# Bug Fix\n\n...', NULL, '2025-10-18T10:00:00Z', '2025-10-18T10:00:00Z');
```

**UI Update**: Template appears in template list

### Step 2: Edit the Template

**User Action**: Click template in list, click "Edit", modify content, save

**Frontend**:
```typescript
// PUT /api/templates/{id}
const response = await apiClient.updateTemplate('a1b2c3...', {
  name: "Bug Fix Request",
  content: "# Bug Fix\n\n**Issue**: {{issue_description}}\n**Priority**: {{priority}}\n**Steps to Reproduce**: {{steps}}",
  folderId: null
});
```

**Backend Flow**:
1. TemplatesController.UpdateTemplate() receives request
2. Loads existing template by Id (404 if not found)
3. Validates changes (name unique in folder)
4. Updates entity, sets UpdatedAt to current UTC time
5. Repository persists changes
6. Returns updated TemplateResponse

**Database State**:
```sql
-- Templates table updated
UPDATE Templates
SET Content = '# Bug Fix\n\n**Issue**: {{issue_description}}\n**Priority**: {{priority}}\n**Steps to Reproduce**: {{steps}}',
    UpdatedAt = '2025-10-18T10:05:00Z'
WHERE Id = 'a1b2c3...';
```

**UI Update**: Template content refreshed in editor

### Step 3: View Template Details

**User Action**: Click template name to view full details

**Frontend**:
```typescript
// GET /api/templates/{id}
const template = await apiClient.getTemplate('a1b2c3...');
// Display: name, content, folder, timestamps
```

**Backend Flow**:
1. TemplatesController.GetTemplate() receives request
2. Repository queries by Id, eager loads Folder navigation property
3. Returns TemplateResponse with full content

**UI Display**: Modal/page showing all template fields

### Step 4: Delete Template

**User Action**: Click "Delete" button, confirm dialog

**Frontend**:
```typescript
// DELETE /api/templates/{id}
await apiClient.deleteTemplate('a1b2c3...');
// 204 No Content response
```

**Backend Flow**:
1. TemplatesController.DeleteTemplate() receives request
2. Loads template by Id (404 if not found)
3. Repository deletes entity (hard delete)
4. Returns 204 No Content

**Database State**:
```sql
DELETE FROM Templates WHERE Id = 'a1b2c3...';
```

**UI Update**: Template removed from list

---

## Scenario 2: Organize Templates into Folders (User Story 2)

**Goal**: User creates folders, moves templates between folders, creates nested structure.

### Step 1: Create Root Folder

**User Action**: Click "New Folder", enter "Web Development"

**Frontend**:
```typescript
// POST /api/folders
const folder = await apiClient.createFolder({
  name: "Web Development",
  parentFolderId: null
});
// Response: FolderResponse with id 'f1...'
```

**Backend Flow**:
1. FoldersController.CreateFolder() receives request
2. Validates: name unique among root folders
3. FolderService creates entity
4. Repository saves to `Folders` table
5. Returns FolderResponse

**Database State**:
```sql
INSERT INTO Folders (Id, Name, ParentFolderId, CreatedAt, UpdatedAt)
VALUES ('f1...', 'Web Development', NULL, '2025-10-18T11:00:00Z', '2025-10-18T11:00:00Z');
```

### Step 2: Create Subfolder

**User Action**: Right-click "Web Development", click "New Subfolder", enter "React"

**Frontend**:
```typescript
// POST /api/folders
const subfolder = await apiClient.createFolder({
  name: "React",
  parentFolderId: 'f1...'  // Web Development
});
// Response: FolderResponse with id 'f2...'
```

**Backend Flow**:
1. Validates: name unique among children of 'f1...'
2. Validates: depth check (parent depth + 1 <= 10)
3. Creates folder with parent reference

**Database State**:
```sql
INSERT INTO Folders (Id, Name, ParentFolderId, CreatedAt, UpdatedAt)
VALUES ('f2...', 'React', 'f1...', '2025-10-18T11:02:00Z', '2025-10-18T11:02:00Z');
```

### Step 3: Create Template in Subfolder

**User Action**: Click "React" folder, click "New Template"

**Frontend**:
```typescript
// POST /api/templates
const template = await apiClient.createTemplate({
  name: "Component Template",
  content: "import React from 'react';\n\nfunction {{component_name}}() {...}",
  folderId: 'f2...'  // React folder
});
```

**Backend Flow**:
1. Validates: name unique within folder 'f2...'
2. Creates template associated with folder

**Database State**:
```sql
INSERT INTO Templates (Id, Name, Content, FolderId, CreatedAt, UpdatedAt)
VALUES ('t1...', 'Component Template', '...', 'f2...', '2025-10-18T11:05:00Z', '2025-10-18T11:05:00Z');
```

### Step 4: Move Template to Different Folder

**User Action**: Drag "Component Template" from "React" to "Web Development"

**Frontend**:
```typescript
// PUT /api/templates/{id}
await apiClient.updateTemplate('t1...', {
  name: "Component Template",
  content: "...",  // Unchanged
  folderId: 'f1...'  // Move to Web Development
});
```

**Backend Flow**:
1. Validates: name unique in destination folder 'f1...'
2. Updates FolderId and UpdatedAt

**Database State**:
```sql
UPDATE Templates
SET FolderId = 'f1...', UpdatedAt = '2025-10-18T11:10:00Z'
WHERE Id = 't1...';
```

### Step 5: Get Folder Tree

**User Action**: UI loads folder tree on app startup

**Frontend**:
```typescript
// GET /api/folders
const tree = await apiClient.getFolderTree();
// Returns nested structure
```

**Backend Flow**:
1. FoldersController.GetFolderTree() executes recursive query
2. Returns hierarchical FolderTreeNode structure

**Response**:
```json
{
  "rootFolders": [
    {
      "id": "f1...",
      "name": "Web Development",
      "depth": 0,
      "templateCount": 1,
      "childFolders": [
        {
          "id": "f2...",
          "name": "React",
          "depth": 1,
          "templateCount": 0,
          "childFolders": []
        }
      ]
    }
  ]
}
```

### Step 6: Delete Empty Folder

**User Action**: Right-click "React" (empty), click "Delete"

**Frontend**:
```typescript
// DELETE /api/folders/{id}
await apiClient.deleteFolder('f2...');
```

**Backend Flow**:
1. FoldersController.DeleteFolder() checks if folder is empty
2. Validates: no child folders, no templates (returns 400 if not empty)
3. Deletes folder if empty

**Database State**:
```sql
DELETE FROM Folders WHERE Id = 'f2...';
```

---

## Scenario 3: Use Template with Placeholder Substitution (User Story 3)

**Goal**: User selects template, fills placeholders, previews prompt, sends to Devin.

### Step 1: Extract Placeholders from Template

**User Action**: Click "Use Template" on "Bug Fix Request"

**Frontend**:
```typescript
// GET /api/templates/{id}/placeholders
const placeholders = await apiClient.getTemplatePlaceholders('t1...');
```

**Backend Flow**:
1. TemplatesController.GetTemplatePlaceholders() loads template
2. PlaceholderService parses content with regex `/\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}/g`
3. Extracts unique placeholder names
4. Generates display names (snake_case → Title Case)

**Response**:
```json
{
  "placeholders": [
    {
      "name": "issue_description",
      "displayName": "Issue Description",
      "defaultValue": null
    },
    {
      "name": "priority",
      "displayName": "Priority",
      "defaultValue": null
    },
    {
      "name": "steps",
      "displayName": "Steps",
      "defaultValue": null
    }
  ]
}
```

**UI Update**: Form displayed with 3 text inputs

### Step 2: Fill Placeholder Values

**User Action**: User enters:
- Issue Description: "Login button not responsive"
- Priority: "High"
- Steps: "1. Navigate to login\n2. Click button\n3. No response"

### Step 3: Generate Preview

**User Action**: Click "Preview Prompt"

**Frontend**:
```typescript
// POST /api/templates/{id}/generate
const prompt = await apiClient.generatePrompt('t1...', {
  placeholderValues: {
    issue_description: "Login button not responsive",
    priority: "High",
    steps: "1. Navigate to login\n2. Click button\n3. No response"
  }
});
```

**Backend Flow**:
1. TemplatesController.GeneratePrompt() loads template
2. PlaceholderService substitutes each `{{name}}` with provided value
3. Validates: all required placeholders have values (400 if missing)
4. Returns PromptInstanceResponse

**Response**:
```json
{
  "templateId": "t1...",
  "templateName": "Bug Fix Request",
  "finalContent": "# Bug Fix\n\n**Issue**: Login button not responsive\n**Priority**: High\n**Steps to Reproduce**: 1. Navigate to login\n2. Click button\n3. No response",
  "placeholderValues": {
    "issue_description": "Login button not responsive",
    "priority": "High",
    "steps": "1. Navigate to login\n2. Click button\n3. No response"
  },
  "generatedAt": "2025-10-18T12:00:00Z"
}
```

**UI Update**: Preview panel shows final markdown

### Step 4: Send to Devin LLM

**User Action**: Click "Send to Devin"

**Frontend**:
```typescript
// POST /api/templates/{id}/send
const result = await apiClient.sendPromptToDevin('t1...', {
  placeholderValues: { /* same as above */ }
});
```

**Backend Flow**:
1. TemplatesController.SendPromptToDevin() generates prompt (same as preview)
2. DevinClient.SendPrompt() sends finalContent to Devin API
3. **MVP Stub**: Returns success immediately (real Devin integration deferred)
4. Returns SendPromptResponse

**Response**:
```json
{
  "success": true,
  "message": "Prompt sent to Devin successfully",
  "devinResponseId": null
}
```

**UI Update**: Success toast notification

---

## Scenario 4: Search Templates (User Story 4)

**Goal**: User searches for templates by keywords.

### Setup: Create Multiple Templates

**Database State**:
```sql
INSERT INTO Templates VALUES ('t1...', 'Bug Fix Request', '# Bug Fix\n\n**Issue**: {{issue}}...', NULL, ...);
INSERT INTO Templates VALUES ('t2...', 'Feature Spec', '# Feature\n\n**Name**: {{feature_name}}...', 'f1...', ...);
INSERT INTO Templates VALUES ('t3...', 'Code Review', '# Review\n\n**PR**: {{pr_number}}...', NULL, ...);
```

### Step 1: Search by Name

**User Action**: Enter "Bug" in search box

**Frontend**:
```typescript
// GET /api/search?query=Bug&page=1&pageSize=50
const results = await apiClient.searchTemplates({ query: 'Bug', page: 1, pageSize: 50 });
```

**Backend Flow**:
1. SearchController.SearchTemplates() validates query (min 1 char)
2. Uses SQLite FTS5 query on `Templates_FTS` virtual table
3. Returns paginated results with relevance ranking

**Response**:
```json
{
  "items": [
    {
      "id": "t1...",
      "name": "Bug Fix Request",
      "folderId": null,
      "folderName": null,
      "contentPreview": "# Bug Fix\n\n**Issue**: {{issue}}...",
      "placeholderCount": 3,
      "createdAt": "2025-10-18T10:00:00Z",
      "updatedAt": "2025-10-18T10:05:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50,
  "hasMore": false
}
```

### Step 2: Search by Content

**User Action**: Search for "review"

**Frontend**:
```typescript
const results = await apiClient.searchTemplates({ query: 'review', page: 1, pageSize: 50 });
```

**Backend Flow**: FTS5 searches both name and content

**Response**: Returns "Code Review" template (matches name) and "Feature Spec" if content mentions reviews

### Step 3: Clear Search

**User Action**: Clear search box

**Frontend**: Calls `GET /api/templates` to restore full list

---

## Integration Test Checklist

Use these scenarios as integration tests:

- [ ] **Scenario 1**: Create → Edit → View → Delete template completes successfully
- [ ] **Scenario 2**: Create folders → Create subfolders → Move templates → Delete empty folders works
- [ ] **Scenario 3**: Extract placeholders → Fill values → Preview → Send to Devin completes
- [ ] **Scenario 4**: Search by name and content returns correct results

## Error Handling Scenarios

### Duplicate Template Name in Folder

**Action**: Try to create two templates with same name in same folder

**Expected**: 409 Conflict with error message "Template name already exists in this folder"

### Move Template to Non-existent Folder

**Action**: Update template with FolderId that doesn't exist

**Expected**: 400 Bad Request with error message "Folder not found"

### Delete Folder with Templates

**Action**: Try to delete folder containing templates

**Expected**: 400 Bad Request with error message "Cannot delete folder containing templates"

### Circular Folder Reference

**Action**: Try to move folder to be its own descendant

**Expected**: 400 Bad Request with error message "Circular folder reference detected"

### Send Template with Missing Placeholder Values

**Action**: Send template with `{{name}}` but don't provide value for "name"

**Expected**: 400 Bad Request with error message "Missing required placeholder: name"

## Performance Validation

### Search Performance

**Test**: Create 500 templates, search for keyword

**Expected**: Results return in <1 second (per SC-008)

**Measurement**: Frontend logs `performance.now()` before/after API call

### Template List Performance

**Test**: Load template list with 1000 templates

**Expected**: First page loads in <200ms (per SC-006)

**Measurement**: Backend logs request duration

## End-to-End Flow

**Complete User Journey** (combines all user stories):

1. User opens app → Folder tree loads
2. Create "Projects" folder
3. Create "Feature Request" template in "Projects" folder
4. Edit template to add placeholders: `{{feature_name}}`, `{{description}}`
5. Click "Use Template"
6. Fill placeholders: feature_name="Dark Mode", description="Add dark theme toggle"
7. Preview prompt shows substituted values
8. Click "Send to Devin"
9. Success notification appears
10. Search for "Dark Mode" → Template appears in results

**Success Criteria**: All 10 steps complete without errors, <30 seconds total time (per SC-001, SC-004)

---

This quickstart guide serves as both documentation and an acceptance test suite for the implementation phase.
