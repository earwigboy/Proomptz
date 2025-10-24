# Phase 0: Research & Technology Decisions

**Feature**: Filesystem Template Storage
**Date**: 2025-10-22
**Purpose**: Resolve technical unknowns and establish implementation patterns

---

## Research Task 1: Testing Framework for .NET 9.0

**Decision**: **xUnit** with FluentAssertions and Moq

**Rationale**:
- **xUnit** is the most modern .NET testing framework with excellent .NET 9.0 support
- Preferred by ASP.NET Core team and used in official Microsoft samples
- Strong convention over configuration (no [TestClass] or [TestMethod] attributes needed)
- Excellent integration with test runners (dotnet test, Visual Studio, Rider)
- **FluentAssertions** provides readable assertion syntax: `result.Should().Be(expected)`
- **Moq** enables mocking interfaces (ITemplateRepository, IFolderRepository) for unit tests
- Together they support both TDD and test-after approaches

**Alternatives Considered**:
- **NUnit**: Mature but more verbose attribute model; less commonly used in modern .NET projects
- **MSTest**: Microsoft's original framework; limited features compared to xUnit
- **No tests**: Violates constitution's critical testing requirements

**Implementation**:
```xml
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="FluentAssertions" Version="6.12.1" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
```

**Test Organization**:
- **Unit Tests**: `PromptTemplateManager.Tests.Unit` project
  - Test services with mocked repositories
  - Test filesystem utilities (TemplateFileManager, FileNameSanitizer) with in-memory streams
- **Integration Tests**: `PromptTemplateManager.Tests.Integration` project
  - Test repositories with real filesystem (temporary directories)
  - Test end-to-end workflows with test fixtures

---

## Research Task 2: YAML Frontmatter Parsing in .NET

**Decision**: **YamlDotNet** library for YAML serialization/deserialization

**Rationale**:
- Most mature and widely-used YAML library for .NET (10M+ NuGet downloads)
- Supports YAML 1.1 and subset of 1.2 specification
- Strong type safety with object mapping
- Excellent performance for small documents (typical frontmatter <1KB)
- Active maintenance and .NET 9.0 compatibility

**Alternatives Considered**:
- **Manual parsing**: Error-prone, requires handling edge cases (escaping, multiline, special characters)
- **JSON in frontmatter**: Not standard markdown convention; less human-readable
- **Custom binary format**: Defeats purpose of human-readable storage

**Template File Format**:
```markdown
---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:30:00Z
updated: 2025-10-22T15:45:00Z
---
# Template content starts here

Hello {{name}}, welcome to {{company}}!
```

**Implementation**:
```csharp
using YamlDotNet.Serialization;

public class TemplateMetadata
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

// Deserialization
var deserializer = new DeserializerBuilder().Build();
var metadata = deserializer.Deserialize<TemplateMetadata>(frontmatterText);

// Serialization
var serializer = new SerializerBuilder().Build();
var yaml = serializer.Serialize(metadata);
```

**Package**:
```xml
<PackageReference Include="YamlDotNet" Version="16.2.1" />
```

---

## Research Task 3: Full-Text Search Without SQLite FTS5

**Decision**: **Lucene.NET** for production-grade full-text search

**Rationale**:
- Industry-standard search engine (port of Apache Lucene)
- Supports advanced features: ranking, fuzzy matching, phrase queries, highlighting
- Scales to millions of documents with acceptable performance
- File-based index structure (fits filesystem storage paradigm)
- No external dependencies (self-contained .NET library)
- Can index markdown files directly with custom analyzers

**Alternatives Considered**:
- **Regex/Contains search**: O(n) performance, no ranking, poor UX for large collections
- **Azure Cognitive Search / Elasticsearch**: Over-engineered for single-user app; requires external service
- **Simple file grep**: No ranking, slow for 10k files, no support for complex queries

**Performance Expectations**:
- Indexing: ~1000 documents/second for typical templates
- Search: <100ms for simple queries, <500ms for complex queries
- Disk space: ~10-20% overhead for index files
- Success criteria alignment: SC-004 requires <2s for 10k templates (Lucene.NET easily achieves this)

**Implementation Strategy**:
```csharp
// Index location: /data/search-index/
// Index on template create/update, delete from index on template delete
// Rebuild index on application startup if index missing or corrupt

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;

// Index a template
var doc = new Document();
doc.Add(new StringField("id", template.Id.ToString(), Field.Store.YES));
doc.Add(new TextField("name", template.Name, Field.Store.YES));
doc.Add(new TextField("content", template.Content, Field.Store.YES));
indexWriter.AddDocument(doc);

// Search templates
var query = queryParser.Parse(searchTerm);
var hits = searcher.Search(query, 100);
```

**Package**:
```xml
<PackageReference Include="Lucene.Net" Version="4.8.0-beta00016" />
<PackageReference Include="Lucene.Net.Analysis.Common" Version="4.8.0-beta00016" />
<PackageReference Include="Lucene.Net.QueryParser" Version="4.8.0-beta00016" />
```

---

## Research Task 4: File Locking and Concurrent Access

**Decision**: **FileStream with FileShare.None** for write operations, **FileShare.Read** for read operations

**Rationale**:
- .NET provides built-in file locking via FileStream constructor
- For single-user application, simple locking is sufficient (no distributed lock needed)
- Write operations: Exclusive lock prevents external editors from corrupting during save
- Read operations: Shared lock allows multiple readers (app + external editor viewing file)
- Retry logic with exponential backoff handles transient lock conflicts

**Alternatives Considered**:
- **No locking**: Risk of data corruption if external editor saves while app is writing
- **Database transactions**: No longer applicable (moving away from SQLite)
- **Distributed lock (Redis/ZooKeeper)**: Over-engineered for single-user desktop app

**Implementation Pattern**:
```csharp
// Write with exclusive lock
using var stream = new FileStream(
    path,
    FileMode.Create,
    FileAccess.Write,
    FileShare.None,  // Exclusive access during write
    bufferSize: 4096,
    useAsync: true
);
await stream.WriteAsync(buffer);

// Read with shared lock (allows external viewers)
using var stream = new FileStream(
    path,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read,  // Allow other readers
    bufferSize: 4096,
    useAsync: true
);
var content = await new StreamReader(stream).ReadToEndAsync();
```

**Retry Logic** (for handling lock conflicts):
```csharp
public async Task<T> RetryOnLockAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try { return await operation(); }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            if (i == maxRetries - 1) throw;
            await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, i)));
        }
    }
}
```

---

## Research Task 5: Filename Sanitization for Cross-Platform Compatibility

**Decision**: **Custom sanitizer** that replaces illegal characters with safe alternatives

**Rationale**:
- Template names are user-provided strings that may contain filesystem-illegal characters
- Different OSes have different illegal character sets (Windows most restrictive)
- Must preserve readability: "My Template: Draft #2" → "My Template - Draft _2"
- Must prevent directory traversal attacks: "../../../etc/passwd" → "_.._.._.._etc_passwd"
- Must handle Unicode characters correctly (allow emoji, international characters)

**Illegal Characters**:
- **Windows**: `< > : " / \ | ? *` + control characters (0x00-0x1F)
- **Linux**: `/` + null byte (0x00)
- **macOS**: `/` + `:` (legacy HFS+)
- **Reserved names (Windows)**: CON, PRN, AUX, NUL, COM1-9, LPT1-9

**Replacement Strategy**:
```csharp
public static string SanitizeFileName(string fileName)
{
    // Map problematic characters to safe alternatives
    var replacements = new Dictionary<char, char>
    {
        { ':', '-' },   // "Template: Draft" → "Template - Draft"
        { '/', '_' },   // "Before/After" → "Before_After"
        { '\\', '_' },
        { '<', '_' },
        { '>', '_' },
        { '|', '_' },
        { '?', '_' },
        { '*', '_' },
        { '"', '\'' }   // Preserve quotes as single quotes
    };

    var sanitized = new StringBuilder();
    foreach (var c in fileName)
    {
        if (char.IsControl(c)) continue;  // Skip control characters
        sanitized.Append(replacements.TryGetValue(c, out var replacement)
            ? replacement
            : c);
    }

    // Trim leading/trailing spaces and dots (problematic on Windows)
    var result = sanitized.ToString().Trim(' ', '.');

    // Check for reserved names (Windows)
    if (IsReservedName(result)) result = "_" + result;

    // Ensure not empty
    if (string.IsNullOrWhiteSpace(result)) result = "Untitled";

    // Truncate to max filename length (255 on most systems, 143 on encrypted filesystems)
    return result.Length > 200 ? result.Substring(0, 200) : result;
}
```

**Collision Handling**:
If sanitized name already exists in folder, append incrementing number:
- "Template.md" exists → "Template (2).md"
- "Template (2).md" exists → "Template (3).md"

---

## Research Task 6: Migration Strategy from SQLite to Filesystem

**Decision**: **One-time migration utility** executed on first startup with filesystem storage enabled

**Rationale**:
- Existing users have templates in SQLite database
- Cannot lose data during transition
- Migration should be idempotent (safe to run multiple times)
- Should create backup of database before migration
- Should verify migration success before removing database

**Migration Process**:
1. **Check flag**: Look for `.migrated` marker file in data directory
2. **If already migrated**: Skip migration, proceed with filesystem storage
3. **If not migrated**:
   - Backup `prompttemplates.db` → `prompttemplates.db.backup.{timestamp}`
   - Read all folders from database (hierarchical order: parents before children)
   - Create directory structure in `/data/templates/`
   - Read all templates from database
   - Write each template as `.md` file with YAML frontmatter
   - Verify file count matches database count
   - Create `.migrated` marker file
   - Log success message with backup location

**Implementation**:
```csharp
public class DatabaseToFileSystemMigrator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITemplateRepository _fileSystemRepo;
    private readonly string _dataPath;

    public async Task<MigrationResult> MigrateAsync()
    {
        if (File.Exists(Path.Combine(_dataPath, ".migrated")))
            return MigrationResult.AlreadyMigrated;

        // Backup database
        var backupPath = $"prompttemplates.db.backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
        File.Copy("prompttemplates.db", backupPath);

        // Migrate folders (parent folders first)
        var folders = await _dbContext.Folders
            .OrderBy(f => f.ParentFolderId == null ? 0 : 1)  // Root folders first
            .ToListAsync();

        foreach (var folder in folders)
        {
            var path = GetFolderPath(folder, folders);
            Directory.CreateDirectory(path);
        }

        // Migrate templates
        var templates = await _dbContext.Templates.ToListAsync();
        foreach (var template in templates)
        {
            await _fileSystemRepo.CreateAsync(template);
        }

        // Verify migration
        var fileCount = Directory.GetFiles(_dataPath, "*.md", SearchOption.AllDirectories).Length;
        if (fileCount != templates.Count)
            throw new MigrationException($"File count mismatch: {fileCount} files vs {templates.Count} templates");

        // Mark as migrated
        await File.WriteAllTextAsync(Path.Combine(_dataPath, ".migrated"), DateTime.UtcNow.ToString("O"));

        return MigrationResult.Success(templates.Count, backupPath);
    }
}
```

**Rollback Strategy**:
If migration fails:
- Delete `/data/templates/` directory
- Restore from backup: `prompttemplates.db.backup.{timestamp}` → `prompttemplates.db`
- Delete `.migrated` marker
- Log error and exit

**User Communication**:
- On first startup with filesystem storage: Display migration progress
- After successful migration: Show summary (X templates migrated, backup location)
- Offer option to delete backup after user verification (manual step)

---

## Research Task 7: Performance Optimization for File I/O

**Decision**: **Async I/O** with **in-memory caching** for frequently accessed templates

**Rationale**:
- Modern .NET async I/O (File.ReadAllTextAsync, FileStream.WriteAsync) prevents thread blocking
- In-memory cache (IMemoryCache) reduces disk reads for frequently used templates
- Cache invalidation on file writes ensures consistency
- FileSystemWatcher monitors external changes and invalidates cache

**Caching Strategy**:
```csharp
public class CachedFileSystemTemplateRepository : ITemplateRepository
{
    private readonly IMemoryCache _cache;
    private readonly FileSystemWatcher _watcher;

    public async Task<Template> GetByIdAsync(Guid id)
    {
        var cacheKey = $"template:{id}";

        if (_cache.TryGetValue(cacheKey, out Template cached))
            return cached;

        var template = await ReadFromDiskAsync(id);

        _cache.Set(cacheKey, template, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Size = 1  // Limit cache to ~1000 templates (configurable)
        });

        return template;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Invalidate cache when file changes externally
        var id = ExtractIdFromFileName(e.FullPath);
        _cache.Remove($"template:{id}");
    }
}
```

**FileSystemWatcher** (for external changes):
- Monitors `/data/templates/` directory recursively
- Triggers cache invalidation on file changes (Created, Changed, Deleted, Renamed)
- Debounces events (100ms delay) to avoid rapid-fire invalidations during multi-file saves

**Performance Targets Validation**:
- **SC-001**: Operations <1s for 100KB files
  - Async file read: ~5-10ms for 100KB
  - YAML parsing: ~2-5ms
  - Total: <20ms (well below 1s)
- **SC-004**: Search <2s for 10k templates
  - Lucene.NET index: ~50-100ms
  - File reads for top results: ~50ms (cached)
  - Total: <200ms (well below 2s)

---

## Summary of Decisions

| Unknown | Decision | Package/Technology |
|---------|----------|-------------------|
| Testing Framework | xUnit + FluentAssertions + Moq | xunit 2.9.0, FluentAssertions 6.12.1, Moq 4.20.72 |
| YAML Parsing | YamlDotNet | YamlDotNet 16.2.1 |
| Full-Text Search | Lucene.NET | Lucene.Net 4.8.0-beta00016 |
| File Locking | FileStream with FileShare modes | Built-in .NET |
| Filename Sanitization | Custom sanitizer with replacement map | Custom implementation |
| Migration Strategy | One-time migration utility with backup | Custom implementation |
| Performance Optimization | Async I/O + IMemoryCache + FileSystemWatcher | Built-in .NET |

**All NEEDS CLARIFICATION items from Technical Context are now resolved.**

**Next Phase**: Phase 1 - Design (data model, API contracts, quickstart scenarios)
