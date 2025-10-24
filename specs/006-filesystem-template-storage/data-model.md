# Phase 1: Data Model

**Feature**: Filesystem Template Storage
**Date**: 2025-10-22
**Purpose**: Define entities, file structures, and relationships for filesystem-based storage

---

## Entity Definitions

### 1. Template Entity

**Purpose**: Represents a prompt template with content and metadata

**Properties**:
```csharp
public class Template
{
    public Guid Id { get; set; }              // Unique identifier (persists across renames)
    public string Name { get; set; }          // Display name (max 200 chars)
    public string Content { get; set; }       // Template content with placeholders
    public Guid? FolderId { get; set; }       // Parent folder ID (null = root level)
    public DateTime CreatedAt { get; set; }   // Creation timestamp (UTC)
    public DateTime UpdatedAt { get; set; }   // Last modification timestamp (UTC)

    // Navigation property (not stored in file, computed at runtime)
    public Folder? Folder { get; set; }
}
```

**Validation Rules**:
- `Name`: Required, 1-200 characters, must be unique within parent folder
- `Content`: Required, no max length (but performance tested up to 100KB)
- `Id`: Auto-generated GUID, immutable
- `FolderId`: Optional, must reference existing folder if provided
- `CreatedAt`: Auto-set on creation, immutable
- `UpdatedAt`: Auto-set on creation/update

**File Representation**:
```markdown
---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:30:00.000Z
updated: 2025-10-22T15:45:30.000Z
---
This is the template content with {{placeholder}} syntax.

It can contain multiple lines, code blocks, and **markdown formatting**.
```

**File Location**:
- Root-level template: `/data/templates/{sanitized-name}.md`
- Nested template: `/data/templates/{folder-path}/{sanitized-name}.md`

**Filename Sanitization**:
- Original name: `"My Template: Draft #2"`
- Sanitized filename: `"My Template - Draft _2.md"`
- See research.md Research Task 5 for complete sanitization algorithm

---

### 2. Folder Entity

**Purpose**: Represents a hierarchical folder for organizing templates

**Properties**:
```csharp
public class Folder
{
    public Guid Id { get; set; }              // Unique identifier (persists across renames)
    public string Name { get; set; }          // Display name (max 100 chars)
    public Guid? ParentFolderId { get; set; } // Parent folder ID (null = root level)
    public DateTime CreatedAt { get; set; }   // Creation timestamp (UTC)
    public DateTime UpdatedAt { get; set; }   // Last modification timestamp (UTC)

    // Navigation properties (not stored, computed at runtime)
    public Folder? ParentFolder { get; set; }
    public ICollection<Folder> ChildFolders { get; set; }
    public ICollection<Template> Templates { get; set; }
}
```

**Validation Rules**:
- `Name`: Required, 1-100 characters, must be unique among siblings (same parent)
- `ParentFolderId`: Optional, must reference existing folder, no circular references
- `Id`: Auto-generated GUID, immutable
- `CreatedAt`: Auto-set on creation, immutable
- `UpdatedAt`: Auto-set on creation/update
- **Max depth**: 5 levels (enforced in FolderService, see spec.md SC-005)

**Directory Representation**:
Folders are represented as directories on disk:
```
/data/templates/
├── Work/                          # Root-level folder
│   ├── .folder-meta               # Metadata file
│   ├── Daily Standup.md           # Template in "Work" folder
│   └── Sprint Planning.md
├── Personal/                      # Root-level folder
│   ├── .folder-meta
│   ├── Nested/                    # Nested folder (depth 1)
│   │   ├── .folder-meta
│   │   └── Deep Template.md
│   └── Journal Template.md
└── Quick Template.md              # Root-level template (no folder)
```

**Folder Metadata File** (`.folder-meta`):
```yaml
id: 7c9e6679-7425-40de-944b-e07fc1f90ae7
created: 2025-10-22T09:15:00.000Z
updated: 2025-10-22T12:30:00.000Z
```

**Why Separate Metadata File**:
- Directory names don't support metadata embedding (unlike files with frontmatter)
- Enables stable folder IDs that persist across renames/moves
- Consistent with template metadata approach
- Hidden file (`.folder-meta`) doesn't clutter user's file explorer

---

### 3. Template Metadata (YAML Frontmatter)

**Purpose**: Stores template metadata within the markdown file itself

**Schema**:
```csharp
public class TemplateMetadata
{
    [YamlMember(Alias = "id")]
    public Guid Id { get; set; }

    [YamlMember(Alias = "created")]
    public DateTime Created { get; set; }

    [YamlMember(Alias = "updated")]
    public DateTime Updated { get; set; }
}
```

**Serialization Format** (YAML 1.1):
```yaml
---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:30:00.000Z
updated: 2025-10-22T15:45:30.000Z
---
```

**Parsing Logic**:
```csharp
public static (TemplateMetadata metadata, string content) ParseTemplateFile(string fileContents)
{
    var frontmatterPattern = @"^---\s*\n(.*?)\n---\s*\n(.*)$";
    var match = Regex.Match(fileContents, frontmatterPattern, RegexOptions.Singleline);

    if (!match.Success)
        throw new InvalidTemplateFormatException("Missing or invalid YAML frontmatter");

    var yamlText = match.Groups[1].Value;
    var content = match.Groups[2].Value;

    var deserializer = new DeserializerBuilder().Build();
    var metadata = deserializer.Deserialize<TemplateMetadata>(yamlText);

    return (metadata, content);
}
```

**Edge Cases**:
- **Missing frontmatter**: Reject file as invalid (migration creates frontmatter for all existing templates)
- **Invalid YAML**: Throw deserialization exception, log error, exclude from template list
- **Missing fields**: YamlDotNet throws exception for required properties
- **Extra fields**: Ignored by deserializer (forward compatibility)

---

## Relationships

### Template ↔ Folder

**Relationship Type**: Many-to-One (Many templates belong to one folder)

**Storage Mechanism**: Directory hierarchy
```
/data/templates/{folder-name}/     ← Folder
    └── {template-name}.md         ← Template
```

**Orphan Templates** (FolderId = null):
- Stored at `/data/templates/{template-name}.md` (root level)
- No parent directory (except root templates folder)

**Cascading Behavior**:
- **Folder deleted**: Move contained templates to root level (set FolderId = null)
  - Alternative: Prevent deletion of non-empty folders (current FolderService behavior)
- **Folder moved**: Move entire directory tree (templates move with folder)
- **Folder renamed**: Rename directory (templates stay in renamed folder)

### Folder ↔ Folder (Self-Referential)

**Relationship Type**: Many-to-One (Many child folders belong to one parent folder)

**Storage Mechanism**: Nested directories
```
/data/templates/
    └── Parent/                    ← Parent folder (ParentFolderId = null)
        ├── .folder-meta
        └── Child/                 ← Child folder (ParentFolderId = Parent.Id)
            └── .folder-meta
```

**Circular Reference Prevention**:
- Before moving folder: Check if destination is a descendant of source
- Algorithm: Traverse parent chain from destination; if source ID found → reject move
- Implementation: FolderRepository.HasCircularReferenceAsync (already exists)

**Max Depth Enforcement**:
- Before creating/moving folder: Calculate depth from root
- Algorithm: Count parent hops until ParentFolderId = null
- Limit: 5 levels (spec.md SC-005)
- Implementation: FolderRepository.GetDepthAsync (already exists)

---

## State Transitions

### Template Lifecycle

```
[Non-existent]
      ↓ CreateAsync(template)
   [Created] ← File written with frontmatter
      ↓ UpdateAsync(template)
   [Updated] ← File overwritten, UpdatedAt timestamp refreshed
      ↓ DeleteAsync(id)
  [Deleted] ← File removed from disk
```

**Metadata Evolution**:
- `CreatedAt`: Set once on creation, never changes
- `UpdatedAt`: Updated on every save (content or folder move)
- `Id`: Immutable, generated once

**External Modifications**:
- File edited externally → `UpdatedAt` NOT automatically updated (no filesystem watcher for timestamps)
- File deleted externally → Removed from in-memory cache on next scan (FileSystemWatcher triggers)
- File created externally → Rejected if missing frontmatter (or auto-generate ID? See edge cases)

### Folder Lifecycle

```
[Non-existent]
      ↓ CreateAsync(folder)
   [Created] ← Directory + .folder-meta created
      ↓ UpdateAsync(folder) [rename or move]
   [Updated] ← Directory renamed/moved, .folder-meta updated
      ↓ DeleteAsync(id) [if empty]
  [Deleted] ← Directory + .folder-meta removed
```

**Constraints**:
- Cannot delete non-empty folder (FolderService validates before deletion)
- Cannot move folder to descendant (creates circular reference)
- Cannot create folder beyond max depth (5 levels)

---

## Indexing Strategy (Search)

### Lucene.NET Index Schema

**Document Fields**:
```csharp
var doc = new Document();

// Stored fields (returned in search results)
doc.Add(new StringField("id", template.Id.ToString(), Field.Store.YES));
doc.Add(new StringField("name", template.Name, Field.Store.YES));
doc.Add(new StringField("folderId", template.FolderId?.ToString() ?? "", Field.Store.YES));

// Indexed fields (searchable, not stored)
doc.Add(new TextField("nameSearchable", template.Name, Field.Store.NO));
doc.Add(new TextField("content", template.Content, Field.Store.NO));

// Date fields (for sorting/filtering)
doc.Add(new StringField("created", template.CreatedAt.ToString("O"), Field.Store.YES));
doc.Add(new StringField("updated", template.UpdatedAt.ToString("O"), Field.Store.YES));
```

**Index Location**: `/data/search-index/`

**Index Lifecycle**:
- **Application startup**: Open existing index or create new if missing
- **Template created**: Add document to index
- **Template updated**: Delete old document, add new document (by ID)
- **Template deleted**: Delete document from index (by ID)
- **Index corruption**: Rebuild from filesystem (scan all .md files)

**Search Query Example**:
```csharp
var queryParser = new MultiFieldQueryParser(
    LuceneVersion.LUCENE_48,
    new[] { "nameSearchable", "content" },
    new StandardAnalyzer(LuceneVersion.LUCENE_48)
);

var query = queryParser.Parse(searchTerm);  // e.g., "meeting notes"
var hits = searcher.Search(query, 100);     // Top 100 results

// Hits are ranked by TF-IDF relevance (Lucene default)
```

**Performance Expectations**:
- Index build: ~1000 templates/second
- Search latency: <100ms for simple queries, <500ms for complex
- Meets SC-004: <2s for 10k templates (actual: ~100ms)

---

## File System Layout Reference

### Production Layout

```
/data/
├── templates/                         # Template storage root
│   ├── .folder-meta                   # Metadata for root (if needed)
│   ├── Work/
│   │   ├── .folder-meta               # Folder metadata
│   │   ├── Daily Standup.md           # Template
│   │   ├── Sprint Planning.md
│   │   └── Meetings/
│   │       ├── .folder-meta
│   │       └── Retrospective.md
│   ├── Personal/
│   │   ├── .folder-meta
│   │   └── Journal.md
│   └── Quick Note.md                  # Root-level template
├── search-index/                      # Lucene.NET index
│   ├── segments_1
│   ├── _0.cfs
│   └── write.lock
├── prompttemplates.db                 # [DEPRECATED] Pre-migration database
├── prompttemplates.db.backup.20251022 # Migration backup
└── .migrated                          # Migration completion marker
```

### Development Layout

Same as production, but located in:
- **Backend project**: `backend/src/PromptTemplateManager.Api/data/`
- **Docker volume**: `/app/data/` (mounted from host `./data/`)

---

## Migration Mapping

### SQLite → Filesystem

| SQLite Entity | Filesystem Entity | Mapping |
|---------------|-------------------|---------|
| Templates table row | `.md` file | `Id`, `Name`, `Content`, `CreatedAt`, `UpdatedAt` → YAML frontmatter + content |
| Folders table row | Directory + `.folder-meta` | `Id`, `Name`, `ParentFolderId`, `CreatedAt`, `UpdatedAt` → `.folder-meta` YAML |
| TemplatesFts virtual table | Lucene.NET index | Full-text search index rebuilt from `.md` files |
| Composite unique index (FolderId, Name) | Directory structure | Uniqueness enforced by filesystem (no duplicate filenames in same directory) |

**Migration Order**:
1. Folders (parents first, children last) → Create directories
2. Templates (any order) → Create `.md` files
3. Search index → Build from all `.md` files

**Verification**:
- File count = Templates table row count
- Directory count = Folders table row count
- Search index document count = Templates table row count

---

## Summary

**Entities**: Template, Folder, TemplateMetadata
**Storage Format**: Markdown with YAML frontmatter (templates), YAML files (folders)
**Relationships**: Many-to-One (Template → Folder), Self-Referential (Folder → Folder)
**Search**: Lucene.NET with TF-IDF ranking
**Migration**: One-to-one mapping from SQLite to filesystem

**Key Design Decisions**:
1. YAML frontmatter keeps all template data in one human-readable file
2. `.folder-meta` provides stable folder IDs independent of directory names
3. Directory hierarchy naturally enforces folder relationships
4. Lucene.NET provides production-grade search without external dependencies
5. FileSystemWatcher enables real-time sync with external file changes

**Next Phase**: API Contracts (Phase 1 continued)
