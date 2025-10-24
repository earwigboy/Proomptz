# Feature Specification: Filesystem Template Storage

**Feature Branch**: `006-filesystem-template-storage`
**Created**: 2025-10-22
**Status**: Draft
**Input**: User description: "I want to change the way templates are stored. I want them to be stored in the filesytem"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Store Templates as Files (Priority: P1)

Users can create, read, update, and delete templates that are persisted as individual files on the filesystem rather than records in a database. Each template is stored as a separate markdown file within a structured directory hierarchy that mirrors the existing folder organization.

**Why this priority**: This is the core functionality change that enables all other benefits of filesystem storage - version control, external editing, portability, and human-readable storage. Without this, the feature cannot deliver any value.

**Independent Test**: Can be fully tested by creating a template through the UI, verifying the corresponding .md file exists on disk with correct content, modifying the file externally, refreshing the UI to see changes, and deleting the template to verify file removal.

**Acceptance Scenarios**:

1. **Given** a user has the application running, **When** they create a new template named "Test Template" with content "Hello {{name}}", **Then** a file named "Test Template.md" is created in the templates directory with the markdown content "Hello {{name}}"

2. **Given** template files exist in the filesystem, **When** the user opens the application, **Then** all templates are loaded from the filesystem and displayed in the UI with their correct names, content, and folder associations

3. **Given** a template file exists on disk, **When** the user updates the template through the UI with new content, **Then** the corresponding file is updated with the new content while preserving the filename and folder location

4. **Given** a template file exists on disk, **When** the user deletes the template through the UI, **Then** the corresponding file is removed from the filesystem

5. **Given** a template file is modified externally (e.g., using a text editor), **When** the user refreshes or reopens the application, **Then** the updated content is reflected in the UI

---

### User Story 2 - Maintain Folder Hierarchy (Priority: P2)

Users can organize templates into folders, with the folder structure represented as a directory hierarchy on the filesystem. Moving templates between folders updates the file location, and folder operations (create, rename, delete) are reflected in the filesystem.

**Why this priority**: Folder organization is essential for managing multiple templates, but the basic template CRUD operations (P1) must work first. This enhances usability but isn't required for minimal viable functionality.

**Independent Test**: Can be fully tested by creating a folder structure in the UI, verifying corresponding directories are created, moving a template between folders to verify file relocation, and deleting a folder to verify directory removal (with appropriate handling of contained templates).

**Acceptance Scenarios**:

1. **Given** a user creates a folder named "Work", **When** the folder is saved, **Then** a directory named "Work" is created in the templates root directory

2. **Given** a template exists in a folder, **When** the template is moved to a different folder through the UI, **Then** the template file is moved from the source folder directory to the destination folder directory

3. **Given** a folder contains templates, **When** the user attempts to delete the folder, **Then** the system prevents the deletion and displays an error message indicating that the folder must be empty before it can be deleted

4. **Given** a folder is renamed, **When** the rename is saved, **Then** the corresponding directory is renamed on the filesystem and all contained template files remain accessible

---

### User Story 3 - Metadata Storage (Priority: P3)

System maintains template metadata (ID, created date, updated date) alongside the template content without storing it in a database. Metadata is stored in a way that supports search functionality and unique identification.

**Why this priority**: Metadata enhances the user experience with features like search, sorting, and tracking changes, but the core template storage and organization (P1 and P2) can function without it.

**Independent Test**: Can be fully tested by creating a template and verifying metadata file creation, checking that created/updated timestamps are accurate, performing searches that utilize metadata, and ensuring the application can reconstruct the template list with proper IDs after restart.

**Acceptance Scenarios**:

1. **Given** a template is created, **When** it is saved to the filesystem, **Then** metadata (ID, created timestamp, updated timestamp) is stored as YAML frontmatter within the .md file itself, making it human-readable and keeping all template data in a single file

2. **Given** templates with metadata exist, **When** a user searches for templates, **Then** the search functionality can access and utilize the metadata for ranking and filtering results

3. **Given** a template exists with metadata, **When** the template is updated, **Then** the updated timestamp in the metadata is automatically refreshed while preserving the created timestamp and ID

---

### Edge Cases

- What happens when a template file is manually deleted from the filesystem while the application is running?
- How does the system handle duplicate filenames in the same folder (e.g., two files both named "Template.md")?
- What happens when a folder contains files that are not valid template markdown files?
- How does the system handle special characters or illegal filesystem characters in template names (e.g., /, \, :, *, ?, ", <, >, |)?
- What happens when the filesystem directory structure is modified externally (folders created, moved, or deleted)?
- How does the system handle file permission errors (read-only directories, insufficient permissions)?
- What happens during migration from the existing database storage to filesystem storage - how are existing templates converted?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST store each template as an individual markdown (.md) file on the filesystem
- **FR-002**: System MUST organize template files into directory hierarchies that mirror the folder structure shown in the UI
- **FR-003**: System MUST generate valid filesystem-safe filenames from template names, handling special characters appropriately
- **FR-004**: System MUST preserve template content exactly as entered by users, including all formatting and placeholder syntax
- **FR-005**: System MUST support all CRUD operations (Create, Read, Update, Delete) on templates by manipulating files on the filesystem
- **FR-006**: System MUST maintain unique identifiers for templates that persist across application restarts
- **FR-007**: System MUST track creation and modification timestamps for each template
- **FR-008**: System MUST reload templates from the filesystem on application startup to reflect any external changes
- **FR-009**: System MUST provide error handling for filesystem operations (permission errors, disk full, invalid paths)
- **FR-010**: System MUST migrate existing templates from the database to the filesystem when transitioning to the new storage method
- **FR-011**: System MUST support full-text search across template content stored in files
- **FR-012**: System MUST handle concurrent access to template files safely (prevent data loss from simultaneous edits)
- **FR-013**: System MUST validate that template names do not contain illegal filesystem characters before creating files
- **FR-014**: System MUST support folder operations (create, rename, delete, move) by manipulating directories on the filesystem

### Key Entities *(include if feature involves data)*

- **Template File**: A markdown file representing a single template, containing the template content and optionally embedded metadata (frontmatter)
- **Template Directory**: A folder on the filesystem that corresponds to a user-created folder in the application, containing template files and potentially nested subdirectories
- **Template Metadata**: Information about a template including unique identifier (GUID), creation timestamp, last modified timestamp, and potentially cached search data
- **Storage Root**: The base directory where all template files and folders are stored, configurable via application settings

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create, edit, and delete templates with the same performance characteristics as the current database implementation (operations complete in under 1 second for typical template sizes up to 100KB)
- **SC-002**: Template files are human-readable and can be edited with any standard text editor without corrupting data or losing functionality when reopened in the application
- **SC-003**: The system successfully migrates 100% of existing templates from database to filesystem without data loss or corruption
- **SC-004**: Template search returns relevant results in under 2 seconds for collections of up to 10,000 templates
- **SC-005**: Users can organize templates into nested folder structures at least 5 levels deep without performance degradation
- **SC-006**: The application correctly handles and recovers from at least 95% of filesystem errors (permissions, disk full, external modifications) without crashing or losing user data

