# Phase 1: Integration Quickstart

**Feature**: Filesystem Template Storage
**Date**: 2025-10-22
**Purpose**: Define end-to-end integration scenarios and workflows

---

## Scenario 1: Fresh Installation (No Migration)

**User Context**: New user installing application for first time, no existing templates

**Workflow**:
```
1. User starts application (Docker container)
   → Application starts, checks for /data/templates/ directory
   → Directory doesn't exist → Creates /data/templates/
   → No .migrated marker → Skips migration
   → Initializes empty Lucene index at /data/search-index/

2. User creates first folder "Work" via UI
   → Frontend: POST /api/folders { "name": "Work", "parentFolderId": null }
   → Backend: FolderService validates, calls FileSystemFolderRepository
   → Repository: Creates /data/templates/Work/ directory
   → Repository: Writes /data/templates/Work/.folder-meta with YAML (id, created, updated)
   → Returns folder DTO to frontend

3. User creates template "Daily Standup" in "Work" folder
   → Frontend: POST /api/templates { "name": "Daily Standup", "content": "...", "folderId": "<work-id>" }
   → Backend: TemplateService validates (no duplicate name, folder exists)
   → Repository: Generates GUID for template ID
   → Repository: Sanitizes filename "Daily Standup" → "Daily Standup.md"
   → Repository: Creates /data/templates/Work/Daily Standup.md with:
       ---
       id: <guid>
       created: 2025-10-22T10:00:00Z
       updated: 2025-10-22T10:00:00Z
       ---
       Template content here...
   → Search engine: Indexes template in Lucene (id, name, content fields)
   → Returns template DTO to frontend

4. User searches for "standup"
   → Frontend: GET /api/search/templates?query=standup
   → Backend: FileSystemSearchEngine queries Lucene index
   → Lucene returns document IDs ranked by relevance
   → Repository loads template files by IDs (from cache if available)
   → Returns ranked search results to frontend
```

**Success Criteria**:
- ✅ Folder created with correct directory and metadata file
- ✅ Template created with YAML frontmatter and content
- ✅ Search returns template with relevance score >0.5

---

## Scenario 2: Migration from SQLite (Existing User)

**User Context**: Existing user with 100 templates in SQLite database upgrading to filesystem storage

**Workflow**:
```
1. User updates to new version with filesystem storage
   → Docker container starts with new codebase
   → Application starts, checks for .migrated marker
   → Marker not found → Triggers DatabaseToFileSystemMigrator

2. Migration begins
   → Migrator: Checks /data/prompttemplates.db exists
   → Migrator: Creates backup /data/prompttemplates.db.backup.20251022103000
   → Migrator: Loads all folders from database (parent folders first)
   → Migrator: Creates directory structure:
       /data/templates/Work/ (with .folder-meta)
       /data/templates/Work/Meetings/ (with .folder-meta)
       /data/templates/Personal/ (with .folder-meta)
   → Migrator: Loads all 100 templates from database
   → Migrator: For each template, creates .md file:
       - Sanitizes template name to filename
       - Generates YAML frontmatter from database fields (id, created, updated)
       - Writes content below frontmatter
       - Saves to correct folder path
   → Migrator: Builds Lucene index from all .md files
   → Migrator: Verifies file count (100) matches database count (100)
   → Migrator: Writes /data/.migrated marker with timestamp
   → Migrator: Logs success message with backup location

3. Application continues startup with filesystem storage
   → FileSystemTemplateRepository registered in DI (not SQLite repository)
   → Application loads folder tree from directory structure
   → User sees all 100 templates in UI (unchanged experience)

4. User verifies migration
   → User clicks on template "Sprint Planning"
   → Template loads correctly with preserved content
   → User edits template, saves successfully
   → File updated: /data/templates/Work/Sprint Planning.md
   → UpdatedAt timestamp refreshed in frontmatter
   → Search index updated

5. User manually inspects files (optional)
   → User navigates to /data/templates/ via file explorer
   → Sees human-readable .md files with YAML frontmatter
   → Can open/edit files in VSCode or any text editor
   → Can commit /data/templates/ to git for version control
```

**Success Criteria**:
- ✅ All 100 templates migrated without data loss (SC-003)
- ✅ Folder hierarchy preserved (nested folders intact)
- ✅ Search functionality works with Lucene (returns same results as SQLite FTS5)
- ✅ Backup created successfully before migration
- ✅ Migration idempotent (running again doesn't duplicate files)

---

## Scenario 3: External File Editing (Real-Time Sync)

**User Context**: User wants to edit template in external editor (VSCode) while app is running

**Workflow**:
```
1. User has template "Meeting Notes" open in application UI
   → Template loaded from cache (in-memory MemoryCache)

2. User opens /data/templates/Work/Meeting Notes.md in VSCode
   → File opened with shared read lock (FileShare.Read)
   → Application doesn't block external access

3. User edits content in VSCode and saves
   → VSCode writes file (acquires exclusive write lock temporarily)
   → FileSystemWatcher detects FileChanged event
   → Watcher handler:
       - Invalidates cache for "Meeting Notes" template ID
       - Updates Lucene index with new content
       - Triggers UI refresh via SignalR/polling (optional future enhancement)

4. User refreshes application UI or navigates away and back
   → Application re-reads file from disk (cache miss)
   → Parses YAML frontmatter (id, created, updated)
   → Displays updated content in UI
   → UpdatedAt timestamp NOT automatically changed (user didn't save via UI)

5. User makes additional edit in application UI
   → Frontend: PUT /api/templates/{id} { "name": "...", "content": "NEW CONTENT" }
   → Backend: Acquires exclusive write lock on file
   → Backend: Overwrites file with new content + refreshed UpdatedAt timestamp
   → Backend: Invalidates cache
   → Backend: Updates Lucene index
   → Returns updated template to frontend
```

**Success Criteria**:
- ✅ External edits reflected in application after cache invalidation
- ✅ No file corruption from concurrent access (file locking prevents)
- ✅ Application UI updates correctly when template re-loaded
- ✅ Human-readable files enable external editing (SC-002)

---

## Scenario 4: Template Search with Lucene

**User Context**: User with 10,000 templates wants to find all templates mentioning "retrospective"

**Workflow**:
```
1. User enters "retrospective" in search box
   → Frontend: Debounces input (300ms delay)
   → Frontend: GET /api/search/templates?query=retrospective&page=1&pageSize=20

2. Backend processes search request
   → FileSystemSearchEngine: Parses query with Lucene QueryParser
   → Lucene index: Searches "nameSearchable" and "content" fields
   → Lucene: Returns top 20 document IDs ranked by TF-IDF score
   → Search completes in <100ms for 10k templates (SC-004: <2s)

3. Backend loads template details
   → For each document ID, load template from cache or disk
   → Templates already in cache return instantly (~1ms)
   → Templates not cached read from disk (~5-10ms each)
   → Total response time: ~150ms for 20 results

4. Frontend displays results
   → Shows template names with snippets highlighting "retrospective"
   → Shows relevance scores (optional UI enhancement)
   → User clicks on result → Template opens in editor

5. User refines search with phrase query
   → User searches for "sprint retrospective" (with quotes)
   → Lucene interprets as phrase query (terms must be adjacent)
   → Returns more precise results (fewer false positives)
   → Response time still <200ms
```

**Success Criteria**:
- ✅ Search returns relevant results in <2 seconds (SC-004)
- ✅ Results ranked by relevance (TF-IDF score)
- ✅ Phrase queries supported for precise matching
- ✅ Search scales to 10,000 templates without performance degradation

---

## Scenario 5: Folder Operations (Move, Rename, Delete)

**User Context**: User reorganizing folder structure by moving "Meetings" folder from "Work" to "Archive"

**Workflow**:
```
1. User creates new folder "Archive" at root level
   → Frontend: POST /api/folders { "name": "Archive", "parentFolderId": null }
   → Backend: Creates /data/templates/Archive/ directory with .folder-meta

2. User drags "Meetings" folder from "Work" to "Archive"
   → Frontend: PUT /api/folders/{meetings-id} { "name": "Meetings", "parentFolderId": "<archive-id>" }
   → Backend: FolderService validates move (not to descendant, max depth)
   → Backend: Calculates new path: /data/templates/Archive/Meetings/
   → Repository: Moves directory recursively (OS-level move operation)
       /data/templates/Work/Meetings/ → /data/templates/Archive/Meetings/
   → Repository: Updates .folder-meta UpdatedAt timestamp
   → Repository: All contained templates and subfolders move automatically
   → Lucene index: No update needed (template IDs unchanged, content unchanged)

3. User renames "Meetings" folder to "Meeting Notes"
   → Frontend: PUT /api/folders/{meetings-id} { "name": "Meeting Notes", "parentFolderId": "<archive-id>" }
   → Repository: Renames directory:
       /data/templates/Archive/Meetings/ → /data/templates/Archive/Meeting Notes/
   → Repository: Updates .folder-meta UpdatedAt timestamp

4. User attempts to delete "Work" folder (contains templates)
   → Frontend: DELETE /api/folders/{work-id}
   → Backend: FolderService checks if folder is empty
   → Repository: Lists directory contents (finds templates)
   → Backend: Returns 400 Bad Request "Cannot delete non-empty folder"
   → Frontend: Displays error toast notification

5. User moves all templates out of "Work" folder
   → Frontend: For each template, PUT /api/templates/{id} { "folderId": null }
   → Repository: Moves files to root:
       /data/templates/Work/Daily Standup.md → /data/templates/Daily Standup.md

6. User deletes now-empty "Work" folder
   → Frontend: DELETE /api/folders/{work-id}
   → Backend: Verifies folder is empty
   → Repository: Deletes directory and .folder-meta
   → Returns 204 No Content
```

**Success Criteria**:
- ✅ Folder moves update directory structure correctly
- ✅ Templates and subfolders move with parent folder
- ✅ Folder renames preserve all metadata (id, timestamps)
- ✅ Non-empty folder deletion prevented (error returned)
- ✅ Circular reference moves prevented (SC validated)

---

## Scenario 6: Error Handling (Filesystem Failures)

**User Context**: User encounters filesystem error (disk full, permission denied)

**Workflow**:
```
1. Disk full scenario
   → User creates template with 1MB content
   → Backend: Attempts to write file /data/templates/Large Template.md
   → FileStream throws IOException (disk full)
   → Backend: Catches exception in repository layer
   → Backend: Returns 500 Internal Server Error with message:
       "Failed to save template: insufficient disk space"
   → Frontend: Displays error toast with actionable message:
       "Unable to save template. Please free up disk space and try again."
   → User frees disk space, retries save → Succeeds

2. Permission denied scenario
   → User runs container with read-only volume mount (misconfiguration)
   → User attempts to create template
   → Backend: Attempts to write file
   → FileStream throws UnauthorizedAccessException
   → Backend: Returns 500 Internal Server Error:
       "Failed to save template: permission denied"
   → Frontend: Displays error with instructions to check Docker volume permissions

3. File lock conflict scenario
   → User saves template while external editor has exclusive lock
   → Backend: Attempts to write file
   → FileStream throws IOException (file locked)
   → Backend: Retry logic activates (3 attempts with exponential backoff)
       - Attempt 1: Wait 100ms, retry → Still locked
       - Attempt 2: Wait 200ms, retry → Still locked
       - Attempt 3: Wait 400ms, retry → Success
   → Backend: Returns 200 OK (transparent retry)

   OR (if all retries fail):

   → Backend: Returns 409 Conflict:
       "Template is locked by another process. Please close external editors and try again."
   → Frontend: Displays error with retry button

4. Corrupted YAML frontmatter scenario
   → User manually edits .md file and breaks YAML syntax
   → Application attempts to load template (on startup or cache miss)
   → Repository: Catches YamlException during deserialization
   → Repository: Logs error with file path and line number
   → Repository: Excludes template from list (skips corrupted file)
   → Application startup continues (doesn't crash)
   → User sees missing template in UI, checks logs for details

5. Missing file scenario (external deletion)
   → User deletes template file via file explorer while app running
   → FileSystemWatcher detects FileDeleted event
   → Watcher: Invalidates cache for deleted template
   → Watcher: Removes document from Lucene index
   → User refreshes UI → Template no longer appears (consistent state)
```

**Success Criteria**:
- ✅ 95% of filesystem errors handled gracefully without crash (SC-006)
- ✅ User-facing error messages actionable (not stack traces)
- ✅ Retry logic handles transient failures (file locks)
- ✅ Corrupted files logged and skipped (app continues)
- ✅ External deletions detected and UI stays consistent

---

## Scenario 7: Performance at Scale (10,000 Templates)

**User Context**: Power user with 10,000 templates organized in 500 folders (5 levels deep)

**Workflow**:
```
1. Application startup with 10k templates
   → Scans /data/templates/ directory recursively
   → Reads all .folder-meta files (500 folders)
   → Builds folder tree in memory (recursive tree construction)
   → Startup time: ~1-2 seconds (acceptable)

   → Checks Lucene index exists and is up-to-date
   → Index exists → Skips rebuild (index already has 10k documents)
   → Opens Lucene IndexSearcher (near-instant)

2. User requests folder tree
   → Frontend: GET /api/folders/tree
   → Backend: Returns cached folder tree from memory (built on startup)
   → Response time: <50ms (in-memory operation)

3. User requests template list (paginated)
   → Frontend: GET /api/templates?page=1&pageSize=50
   → Backend: Loads all template IDs from cache (or scans directory)
   → Applies pagination (skip 0, take 50)
   → Loads 50 template files from disk (or cache)
   → Response time: <200ms (most from cache, ~10 disk reads)

4. User searches 10k templates
   → Frontend: GET /api/search/templates?query=sprint+planning
   → Lucene: Searches index (10k documents)
   → Returns top 100 document IDs in <100ms
   → Backend loads top 20 templates for first page
   → Total response time: <200ms (well below 2s target, SC-004)

5. User creates template in deeply nested folder (level 5)
   → Frontend: POST /api/templates { "folderId": "<level-5-folder-id>" }
   → Backend: Validates folder exists and depth ≤5
   → Repository: Constructs file path (5 nested directories)
   → Repository: Writes file to /data/templates/A/B/C/D/E/Template.md
   → Lucene: Adds document to index
   → Response time: <100ms (SC-001: <1s)

6. User moves folder with 1000 templates
   → Frontend: PUT /api/folders/{id} { "parentFolderId": "<new-parent>" }
   → Backend: Validates move (no circular reference)
   → Repository: Moves directory recursively (OS operation)
       /data/templates/Old/Location/ → /data/templates/New/Location/
   → OS moves all 1000 files in one operation (atomic, fast)
   → Response time: <500ms for 1000 files (filesystem optimized for directory moves)
   → Lucene: No index update needed (template IDs and content unchanged)
```

**Success Criteria**:
- ✅ Application startup <5 seconds with 10k templates
- ✅ Folder tree loads <100ms (cached in memory)
- ✅ Paginated template list loads <500ms
- ✅ Search <2 seconds for 10k templates (SC-004)
- ✅ Template CRUD operations <1 second (SC-001)
- ✅ No performance degradation with 5-level deep folders (SC-005)

---

## Integration Checklist

**Pre-Implementation Validation**:
- [ ] xUnit test project created with FluentAssertions and Moq
- [ ] YamlDotNet package installed and YAML parsing tested
- [ ] Lucene.NET package installed and index creation tested
- [ ] FileSystemWatcher tested for external file change detection
- [ ] File locking retry logic validated with concurrent access test
- [ ] Filename sanitization tested with illegal characters and edge cases

**Post-Implementation Validation**:
- [ ] Migration script tested with real SQLite database (100+ templates)
- [ ] All API contracts tested (request/response schemas match pre-migration behavior)
- [ ] Search results verified identical to SQLite FTS5 results (for same query)
- [ ] External file editing workflow tested (VSCode + application running)
- [ ] Folder move/rename operations tested (templates move correctly)
- [ ] Error handling tested (disk full, permission denied, file locked)
- [ ] Performance tested at scale (10k templates, search <2s)
- [ ] Docker volume mount tested (data persists across container restarts)
- [ ] Backup created during migration and restoration tested

---

## Summary

These integration scenarios cover:
1. **Happy path**: Fresh install, normal operations
2. **Migration**: Existing user upgrade with data preservation
3. **External editing**: VSCode/editor integration with real-time sync
4. **Search**: Lucene.NET full-text search at scale
5. **Folder operations**: Move, rename, delete with validation
6. **Error handling**: Graceful degradation for filesystem failures
7. **Performance**: Scale testing with 10k templates

All scenarios align with success criteria (SC-001 through SC-006) and validate the implementation meets requirements without changing API contracts.

**Next Phase**: Phase 2 - Task Generation (via `/speckit.tasks` command, NOT part of `/speckit.plan`)
