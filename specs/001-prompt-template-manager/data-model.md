# Data Model: Prompt Template Manager

**Feature**: 001-prompt-template-manager
**Date**: 2025-10-18
**Phase**: Phase 1 - Design

## Entity Relationship Diagram (Textual)

```
┌─────────────────┐         ┌─────────────────┐
│     Folder      │◄───┐    │    Template     │
├─────────────────┤    │    ├─────────────────┤
│ Id (PK)         │    └────│ FolderId (FK)   │
│ Name            │         │ Id (PK)         │
│ ParentFolderId  │─────┐   │ Name            │
│   (FK, nullable)│     │   │ Content         │
│ CreatedAt       │     │   │ CreatedAt       │
│ UpdatedAt       │     │   │ UpdatedAt       │
└─────────────────┘     │   └─────────────────┘
         ▲              │
         └──────────────┘
      (self-referential)
```

## Core Entities

### Template

**Purpose**: Represents a reusable prompt template with markdown content and optional placeholders.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique identifier |
| Name | string | Required, MaxLength(200), Unique per folder | Human-readable template name |
| Content | string | Required, MaxLength(1000000) | Markdown content with optional `{{placeholders}}` |
| FolderId | Guid? | FK to Folder, Nullable | Parent folder (null = root level) |
| CreatedAt | DateTime | Required, Default(UTC Now) | Timestamp of creation |
| UpdatedAt | DateTime | Required, Default(UTC Now) | Timestamp of last modification |

**Indexes**:
- Primary key on `Id`
- Foreign key index on `FolderId`
- Index on `Name` for search
- Full-text search index on `Name` + `Content` (SQLite FTS5)

**Validation Rules**:
- Name cannot be empty or whitespace
- Name must be unique within the same folder (case-insensitive)
- Content cannot be null (empty string allowed for new templates)
- Content size limit: 1MB (approximately 10,000 lines of markdown)
- FolderId must reference existing Folder if not null
- UpdatedAt must be >= CreatedAt

**Business Rules**:
- Placeholders in Content must match pattern: `{{[a-zA-Z_][a-zA-Z0-9_]*}}`
- Deleting a Template is hard delete (no soft delete for MVP)
- Moving a Template between folders updates FolderId and UpdatedAt

**State Transitions**: None (templates have no lifecycle states)

### Folder

**Purpose**: Organizational container for templates, supports hierarchical nesting.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique identifier |
| Name | string | Required, MaxLength(100) | Folder display name |
| ParentFolderId | Guid? | FK to Folder (self), Nullable | Parent folder (null = root level) |
| CreatedAt | DateTime | Required, Default(UTC Now) | Timestamp of creation |
| UpdatedAt | DateTime | Required, Default(UTC Now) | Timestamp of last modification |

**Relationships**:
- Self-referential: `ParentFolderId` → `Folder.Id` (for nested folders)
- One-to-many: One Folder has many Templates (via `Template.FolderId`)
- One-to-many: One Folder has many child Folders (via `Folder.ParentFolderId`)

**Indexes**:
- Primary key on `Id`
- Foreign key index on `ParentFolderId`
- Composite index on `ParentFolderId, Name` for uniqueness enforcement

**Validation Rules**:
- Name cannot be empty or whitespace
- Name must be unique among sibling folders (same ParentFolderId, case-insensitive)
- ParentFolderId must reference existing Folder if not null
- Circular references prohibited (folder cannot be ancestor of itself)
- Maximum nesting depth: 10 levels (validated on create/move)

**Business Rules**:
- Root folders have `ParentFolderId = null`
- Deleting a folder requires handling child templates and subfolders:
  - **Option 1**: Cascade delete (delete all children)
  - **Option 2**: Move children to parent folder
  - **Option 3**: Prevent deletion if not empty (MVP approach)
- Renaming a folder updates `UpdatedAt`
- Moving a folder to a new parent validates no circular references

**State Transitions**: None (folders have no lifecycle states)

## Derived Entities (Not Persisted)

### PlaceholderInfo

**Purpose**: Runtime representation of a placeholder extracted from template content.

**Fields**:

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Placeholder variable name (without `{{}}`) |
| DisplayName | string | Human-readable name (convert snake_case to Title Case) |
| DefaultValue | string? | Optional default value (future enhancement) |

**Usage**: Generated on-the-fly when user selects a template for usage. Not stored in database.

### PromptInstance

**Purpose**: Completed prompt with placeholder values substituted, ready to send to Devin.

**Fields**:

| Field | Type | Description |
|-------|------|-------------|
| TemplateId | Guid | Source template |
| TemplateName | string | Template name at time of usage |
| FinalContent | string | Markdown with all placeholders replaced |
| PlaceholderValues | Dictionary<string, string> | Key-value pairs of placeholder substitutions |
| GeneratedAt | DateTime | Timestamp of generation |

**Usage**: Created transiently when user fills placeholders and previews/sends prompt. **Not persisted in MVP** (future enhancement: save history).

## Relationships Summary

- **Folder ↔ Folder**: Self-referential hierarchy (tree structure)
- **Folder → Template**: One-to-many (one folder contains many templates)
- **Template → Folder**: Many-to-one (each template belongs to zero or one folder)

## Database Schema (SQLite)

### Templates Table

```sql
CREATE TABLE Templates (
    Id TEXT PRIMARY KEY,  -- Guid stored as string
    Name TEXT NOT NULL,
    Content TEXT NOT NULL,
    FolderId TEXT,  -- Nullable FK
    CreatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    UpdatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    FOREIGN KEY (FolderId) REFERENCES Folders(Id) ON DELETE SET NULL,
    UNIQUE (FolderId, Name COLLATE NOCASE)  -- Unique name per folder
);

CREATE INDEX IX_Templates_FolderId ON Templates(FolderId);
CREATE INDEX IX_Templates_Name ON Templates(Name COLLATE NOCASE);

-- Full-text search virtual table
CREATE VIRTUAL TABLE Templates_FTS USING fts5(
    Name,
    Content,
    content='Templates',
    content_rowid='rowid'
);

-- Triggers to keep FTS in sync
CREATE TRIGGER Templates_FTS_Insert AFTER INSERT ON Templates BEGIN
    INSERT INTO Templates_FTS(rowid, Name, Content)
    VALUES (new.rowid, new.Name, new.Content);
END;

CREATE TRIGGER Templates_FTS_Delete AFTER DELETE ON Templates BEGIN
    DELETE FROM Templates_FTS WHERE rowid = old.rowid;
END;

CREATE TRIGGER Templates_FTS_Update AFTER UPDATE ON Templates BEGIN
    UPDATE Templates_FTS SET Name = new.Name, Content = new.Content
    WHERE rowid = old.rowid;
END;
```

### Folders Table

```sql
CREATE TABLE Folders (
    Id TEXT PRIMARY KEY,  -- Guid stored as string
    Name TEXT NOT NULL,
    ParentFolderId TEXT,  -- Nullable FK (self-referential)
    CreatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    UpdatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    FOREIGN KEY (ParentFolderId) REFERENCES Folders(Id) ON DELETE RESTRICT,
    UNIQUE (ParentFolderId, Name COLLATE NOCASE)  -- Unique name per parent
);

CREATE INDEX IX_Folders_ParentFolderId ON Folders(ParentFolderId);
```

## Entity Framework Core Mappings

### Template Entity

```csharp
public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Folder? Folder { get; set; }
}
```

**Configuration**:
```csharp
builder.Entity<Template>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Content).IsRequired();
    entity.Property(e => e.CreatedAt).IsRequired();
    entity.Property(e => e.UpdatedAt).IsRequired();

    entity.HasOne(e => e.Folder)
          .WithMany()
          .HasForeignKey(e => e.FolderId)
          .OnDelete(DeleteBehavior.SetNull);

    entity.HasIndex(e => e.FolderId);
    entity.HasIndex(e => e.Name);

    // Unique constraint: Name per folder (including root)
    entity.HasIndex(e => new { e.FolderId, e.Name }).IsUnique();
});
```

### Folder Entity

```csharp
public class Folder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Folder? ParentFolder { get; set; }
    public ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
    public ICollection<Template> Templates { get; set; } = new List<Template>();
}
```

**Configuration**:
```csharp
builder.Entity<Folder>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.CreatedAt).IsRequired();
    entity.Property(e => e.UpdatedAt).IsRequired();

    entity.HasOne(e => e.ParentFolder)
          .WithMany(e => e.ChildFolders)
          .HasForeignKey(e => e.ParentFolderId)
          .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

    entity.HasMany(e => e.Templates)
          .WithOne(e => e.Folder)
          .HasForeignKey(e => e.FolderId);

    entity.HasIndex(e => e.ParentFolderId);

    // Unique constraint: Name per parent folder
    entity.HasIndex(e => new { e.ParentFolderId, e.Name }).IsUnique();
});
```

## Data Access Patterns

### Common Queries

**Get all root-level templates** (no folder):
```sql
SELECT * FROM Templates WHERE FolderId IS NULL ORDER BY Name;
```

**Get all templates in a folder**:
```sql
SELECT * FROM Templates WHERE FolderId = ? ORDER BY Name;
```

**Get folder hierarchy** (recursive CTE):
```sql
WITH RECURSIVE FolderTree AS (
    SELECT Id, Name, ParentFolderId, 0 AS Depth
    FROM Folders
    WHERE ParentFolderId IS NULL

    UNION ALL

    SELECT f.Id, f.Name, f.ParentFolderId, ft.Depth + 1
    FROM Folders f
    INNER JOIN FolderTree ft ON f.ParentFolderId = ft.Id
    WHERE ft.Depth < 10  -- Max depth limit
)
SELECT * FROM FolderTree ORDER BY Depth, Name;
```

**Search templates** (FTS5):
```sql
SELECT t.Id, t.Name, t.Content, rank
FROM Templates t
JOIN Templates_FTS fts ON t.rowid = fts.rowid
WHERE Templates_FTS MATCH ?  -- Search query
ORDER BY rank
LIMIT 50;
```

**Check for circular folder reference** (before move):
```sql
WITH RECURSIVE Ancestors AS (
    SELECT Id, ParentFolderId FROM Folders WHERE Id = ?  -- Target parent
    UNION ALL
    SELECT f.Id, f.ParentFolderId FROM Folders f
    INNER JOIN Ancestors a ON f.Id = a.ParentFolderId
)
SELECT COUNT(*) FROM Ancestors WHERE Id = ?;  -- Folder being moved
-- If count > 0, circular reference detected
```

## Validation Summary

### Template Validation

- ✅ Name: Required, 1-200 characters, unique per folder
- ✅ Content: Required, max 1MB
- ✅ Placeholders: Valid syntax `{{name}}`
- ✅ FolderId: Must exist or be null
- ✅ Timestamps: CreatedAt <= UpdatedAt

### Folder Validation

- ✅ Name: Required, 1-100 characters, unique per parent
- ✅ ParentFolderId: Must exist or be null
- ✅ Circular references: Prevented via recursive check
- ✅ Max depth: 10 levels
- ✅ Timestamps: CreatedAt <= UpdatedAt

## Migration Strategy

**Initial Migration**: Create Tables + Indexes + FTS
**Seed Data**: None (user starts with empty library)

**Future Migrations**:
- Add `PromptHistory` table (deferred)
- Add `TemplateVersion` for change tracking (deferred)
- Add `Tag` and `TemplateTag` for tagging (deferred)

## Performance Considerations

- **Indexes**: Cover all FK lookups and search operations
- **FTS5**: Enables sub-second full-text search on 500+ templates
- **Pagination**: Queries limited to 50-200 results
- **Eager Loading**: Use `.Include()` for folder → templates to avoid N+1
- **Caching**: Folder tree cached in memory (rarely changes)

## Constraints & Invariants

1. **No orphaned templates**: If folder deleted, templates either moved or deleted
2. **Unique names per folder**: Enforced by composite index
3. **No circular folders**: Validated before parent change
4. **Max folder depth**: 10 levels enforced
5. **Timestamps always UTC**: All DateTime values stored in UTC
6. **Guids for Ids**: Ensures globally unique identifiers, no auto-increment conflicts
