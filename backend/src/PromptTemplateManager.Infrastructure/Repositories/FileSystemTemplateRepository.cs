using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.FileSystem;

namespace PromptTemplateManager.Infrastructure.Repositories;

/// <summary>
/// Repository for managing templates stored as markdown files with YAML frontmatter.
/// </summary>
public class FileSystemTemplateRepository : ITemplateRepository, IDisposable
{
    private readonly string _storagePath;
    private readonly ILogger<FileSystemTemplateRepository> _logger;
    private readonly IMemoryCache _cache;
    private readonly FileSystemSearchEngine _searchEngine;
    private readonly FileSystemWatcher? _fileWatcher;
    private readonly ConcurrentDictionary<Guid, string> _idToPathMap;

    private const string CacheKeyPrefix = "template:";
    private const string AllTemplatesCacheKey = "templates:all";

    public FileSystemTemplateRepository(
        string storagePath,
        ILogger<FileSystemTemplateRepository> logger,
        IMemoryCache? cache = null,
        FileSystemSearchEngine? searchEngine = null)
    {
        _storagePath = storagePath;
        _logger = logger;
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions { SizeLimit = 1000 });
        _idToPathMap = new ConcurrentDictionary<Guid, string>();

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }

        // Initialize search engine if provided
        if (searchEngine == null)
        {
            // Use a unique index path based on storage path to avoid conflicts
            var indexPath = Path.Combine(_storagePath, ".search-index");
            var searchLogger = NullLogger<FileSystemSearchEngine>.Instance;
            _searchEngine = new FileSystemSearchEngine(indexPath, searchLogger);
        }
        else
        {
            _searchEngine = searchEngine;
        }

        // Initialize ID-to-path map
        InitializeIdMap();

        // Setup file system watcher for external changes
        try
        {
            _fileWatcher = new FileSystemWatcher(_storagePath)
            {
                Filter = "*.md",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Created += OnFileCreated;
            _fileWatcher.Deleted += OnFileDeleted;
            _fileWatcher.Renamed += OnFileRenamed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize FileSystemWatcher");
        }
    }

    private void InitializeIdMap()
    {
        try
        {
            var files = Directory.GetFiles(_storagePath, "*.md", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var contents = File.ReadAllText(file);
                    var (metadata, _) = TemplateFileManager.ParseTemplateFile(contents);
                    _idToPathMap[metadata.Id] = file;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse template file: {FilePath}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ID map");
        }
    }

    public async Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default)
    {
        try
        {
            // Set timestamps if not set
            if (template.CreatedAt == default)
            {
                template.CreatedAt = DateTime.UtcNow;
            }
            if (template.UpdatedAt == default)
            {
                template.UpdatedAt = DateTime.UtcNow;
            }

            // Generate file path
            var directory = GetDirectoryForFolder(template.FolderId);
            var fileName = FileNameSanitizer.GetUniqueFileName(directory, template.Name, ".md");
            var filePath = Path.Combine(directory, fileName);

            // Create metadata and serialize
            var metadata = new TemplateMetadata
            {
                Id = template.Id,
                Created = template.CreatedAt,
                Updated = template.UpdatedAt
            };

            var fileContents = TemplateFileManager.SerializeTemplateFile(metadata, template.Content);
            await TemplateFileManager.WriteFileAsync(filePath, fileContents, cancellationToken);

            // Update ID map
            _idToPathMap[template.Id] = filePath;

            // Index in search engine
            await _searchEngine.IndexTemplateAsync(template);

            // Cache the template
            CacheTemplate(template);

            // Invalidate all templates cache
            _cache.Remove(AllTemplatesCacheKey);

            _logger.LogInformation("Created template {TemplateId}: {TemplateName}", template.Id, template.Name);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template {TemplateId}", template.Id);
            throw;
        }
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = CacheKeyPrefix + id;
        if (_cache.TryGetValue<Template>(cacheKey, out var cached))
        {
            return cached;
        }

        try
        {
            // Find file path
            if (!_idToPathMap.TryGetValue(id, out var filePath) || !File.Exists(filePath))
            {
                return null;
            }

            // Read and parse file
            var fileContents = await TemplateFileManager.ReadFileAsync(filePath, cancellationToken);
            var (metadata, content) = TemplateFileManager.ParseTemplateFile(fileContents);

            // Extract folder ID from path
            var folderPath = Path.GetDirectoryName(filePath);
            var folderId = ExtractFolderIdFromPath(folderPath);

            // Extract name from filename
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var template = new Template
            {
                Id = metadata.Id,
                Name = fileName,
                Content = content,
                FolderId = folderId,
                CreatedAt = metadata.Created,
                UpdatedAt = metadata.Updated
            };

            // Cache the template
            CacheTemplate(template);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {TemplateId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Template>> GetAllAsync(Guid? folderId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var allTemplates = await GetAllTemplatesAsync(cancellationToken);

            var filtered = allTemplates.Where(t => t.FolderId == folderId);

            return filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all templates for folder {FolderId}", folderId);
            return Enumerable.Empty<Template>();
        }
    }

    public async Task<int> GetCountAsync(Guid? folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var allTemplates = await GetAllTemplatesAsync(cancellationToken);
            return allTemplates.Count(t => t.FolderId == folderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get count for folder {FolderId}", folderId);
            return 0;
        }
    }

    public async Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get old file path
            if (!_idToPathMap.TryGetValue(template.Id, out var oldFilePath))
            {
                throw new FileNotFoundException($"Template {template.Id} not found");
            }

            // Update timestamp
            template.UpdatedAt = DateTime.UtcNow;

            // Generate new file path (name or folder might have changed)
            var directory = GetDirectoryForFolder(template.FolderId);
            var fileName = FileNameSanitizer.Sanitize(template.Name) + ".md";
            var newFilePath = Path.Combine(directory, fileName);

            // Create metadata and serialize
            var metadata = new TemplateMetadata
            {
                Id = template.Id,
                Created = template.CreatedAt,
                Updated = template.UpdatedAt
            };

            var fileContents = TemplateFileManager.SerializeTemplateFile(metadata, template.Content);

            // If path changed, delete old file
            if (oldFilePath != newFilePath && File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }

            // Write new/updated file
            await TemplateFileManager.WriteFileAsync(newFilePath, fileContents, cancellationToken);

            // Update ID map
            _idToPathMap[template.Id] = newFilePath;

            // Update search index
            await _searchEngine.IndexTemplateAsync(template);

            // Invalidate cache
            _cache.Remove(CacheKeyPrefix + template.Id);
            _cache.Remove(AllTemplatesCacheKey);

            _logger.LogInformation("Updated template {TemplateId}: {TemplateName}", template.Id, template.Name);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update template {TemplateId}", template.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_idToPathMap.TryGetValue(id, out var filePath))
            {
                return;
            }

            // Delete file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Remove from ID map
            _idToPathMap.TryRemove(id, out _);

            // Remove from search index
            await _searchEngine.RemoveTemplateAsync(id);

            // Invalidate cache
            _cache.Remove(CacheKeyPrefix + id);
            _cache.Remove(AllTemplatesCacheKey);

            _logger.LogInformation("Deleted template {TemplateId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete template {TemplateId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsWithNameInFolderAsync(string name, Guid? folderId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var allTemplates = await GetAllTemplatesAsync(cancellationToken);
            return allTemplates.Any(t =>
                t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                t.FolderId == folderId &&
                t.Id != excludeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check template existence");
            return false;
        }
    }

    public async Task<IEnumerable<Template>> SearchTemplatesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var templateIds = await _searchEngine.SearchAsync(query, pageSize * page);

            var results = new List<Template>();
            foreach (var id in templateIds.Skip((page - 1) * pageSize).Take(pageSize))
            {
                var template = await GetByIdAsync(id, cancellationToken);
                if (template != null)
                {
                    results.Add(template);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search templates with query: {Query}", query);
            return Enumerable.Empty<Template>();
        }
    }

    public async Task<int> GetSearchCountAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            var templateIds = await _searchEngine.SearchAsync(query, 10000);
            return templateIds.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get search count for query: {Query}", query);
            return 0;
        }
    }

    private async Task<List<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache
        if (_cache.TryGetValue<List<Template>>(AllTemplatesCacheKey, out var cached))
        {
            return cached;
        }

        var templates = new List<Template>();
        foreach (var kvp in _idToPathMap)
        {
            var template = await GetByIdAsync(kvp.Key, cancellationToken);
            if (template != null)
            {
                templates.Add(template);
            }
        }

        // Cache all templates
        _cache.Set(AllTemplatesCacheKey, templates, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Size = 1
        });

        return templates;
    }

    private string GetDirectoryForFolder(Guid? folderId)
    {
        if (folderId == null)
        {
            return _storagePath;
        }

        // Build folder path by traversing the folder hierarchy
        // This requires scanning the directory structure to find the folder with the given ID
        try
        {
            var folderPath = FindFolderPath(_storagePath, folderId.Value);
            if (folderPath != null)
            {
                return folderPath;
            }

            _logger.LogWarning("Folder {FolderId} not found, using root storage path", folderId);
            return _storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding folder {FolderId}, using root storage path", folderId);
            return _storagePath;
        }
    }

    /// <summary>
    /// Recursively searches for a folder by ID in the directory structure.
    /// </summary>
    private string? FindFolderPath(string currentPath, Guid folderId)
    {
        if (!Directory.Exists(currentPath))
        {
            return null;
        }

        foreach (var subDirectory in Directory.GetDirectories(currentPath))
        {
            var metaPath = Path.Combine(subDirectory, ".folder-meta");
            if (File.Exists(metaPath))
            {
                try
                {
                    var yaml = File.ReadAllText(metaPath);
                    var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                        .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                        .Build();
                    var metadata = deserializer.Deserialize<FolderMetadata>(yaml);

                    if (metadata.Id == folderId)
                    {
                        return subDirectory;
                    }

                    // Recursively search subdirectories
                    var foundPath = FindFolderPath(subDirectory, folderId);
                    if (foundPath != null)
                    {
                        return foundPath;
                    }
                }
                catch
                {
                    // Skip invalid metadata files
                }
            }
        }

        return null;
    }

    private Guid? ExtractFolderIdFromPath(string? folderPath)
    {
        // If path is null, empty, or equals the root storage path, template is in root
        if (string.IsNullOrEmpty(folderPath) || folderPath == _storagePath)
        {
            return null;
        }

        // Look for .folder-meta file in the directory
        var metaPath = Path.Combine(folderPath, ".folder-meta");
        if (!File.Exists(metaPath))
        {
            return null;
        }

        try
        {
            var yaml = File.ReadAllText(metaPath);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .Build();
            var metadata = deserializer.Deserialize<FolderMetadata>(yaml);

            return metadata.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read folder metadata from {MetaPath}", metaPath);
            return null;
        }
    }

    private void CacheTemplate(Template template)
    {
        _cache.Set(CacheKeyPrefix + template.Id, template, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Size = 1
        });
    }

    // FileSystemWatcher event handlers
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        InvalidateCacheForFile(e.FullPath);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        InvalidateCacheForFile(e.FullPath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        InvalidateCacheForFile(e.FullPath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        InvalidateCacheForFile(e.OldFullPath);
        InvalidateCacheForFile(e.FullPath);
    }

    private void InvalidateCacheForFile(string filePath)
    {
        try
        {
            var id = _idToPathMap.FirstOrDefault(kvp => kvp.Value == filePath).Key;
            if (id != Guid.Empty)
            {
                _cache.Remove(CacheKeyPrefix + id);
                _cache.Remove(AllTemplatesCacheKey);
                _logger.LogDebug("Invalidated cache for template at {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for file: {FilePath}", filePath);
        }
    }

    public void Dispose()
    {
        _fileWatcher?.Dispose();
        _searchEngine?.Dispose();
    }
}
