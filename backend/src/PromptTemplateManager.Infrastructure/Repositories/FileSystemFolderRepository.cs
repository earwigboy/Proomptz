using Microsoft.Extensions.Logging;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.FileSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptTemplateManager.Infrastructure.Repositories;

/// <summary>
/// Filesystem-based implementation of IFolderRepository.
/// Folders are represented as directories with .folder-meta YAML files.
/// </summary>
public class FileSystemFolderRepository : IFolderRepository
{
    private readonly string _rootPath;
    private readonly ILogger<FileSystemFolderRepository> _logger;
    private readonly ISerializer _yamlSerializer;
    private readonly IDeserializer _yamlDeserializer;
    private const string FolderMetaFileName = ".folder-meta";

    public FileSystemFolderRepository(string rootPath, ILogger<FileSystemFolderRepository> logger)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Ensure root directory exists
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            return allFolders.FirstOrDefault(f => f.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by ID {FolderId}", id);
            throw;
        }
    }

    public async Task<List<Folder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAllFoldersWithMetadataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all folders");
            throw;
        }
    }

    public async Task<List<Folder>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);

            // Build parent-child relationships
            var folderDict = allFolders.ToDictionary(f => f.Id);

            foreach (var folder in allFolders)
            {
                folder.ChildFolders = new List<Folder>();
                folder.Templates = new List<Template>();
            }

            foreach (var folder in allFolders)
            {
                if (folder.ParentFolderId.HasValue && folderDict.ContainsKey(folder.ParentFolderId.Value))
                {
                    var parent = folderDict[folder.ParentFolderId.Value];
                    parent.ChildFolders.Add(folder);
                }
            }

            // Return only root-level folders
            return allFolders.Where(f => !f.ParentFolderId.HasValue).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder tree");
            throw;
        }
    }

    public async Task<List<Folder>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            return allFolders.Where(f => f.ParentFolderId == parentId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting children for parent {ParentId}", parentId);
            throw;
        }
    }

    public async Task<Folder?> GetByNameAndParentAsync(string name, Guid? parentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            return allFolders.FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                f.ParentFolderId == parentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by name {Name} and parent {ParentId}", name, parentId);
            throw;
        }
    }

    public async Task<bool> HasCircularReferenceAsync(Guid folderId, Guid? newParentId, CancellationToken cancellationToken = default)
    {
        if (!newParentId.HasValue)
        {
            return false; // Moving to root level, no circular reference possible
        }

        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            var folderDict = allFolders.ToDictionary(f => f.Id);

            // Check if newParentId is a descendant of folderId
            var currentId = newParentId;
            while (currentId.HasValue)
            {
                if (currentId.Value == folderId)
                {
                    return true; // Circular reference detected
                }

                if (!folderDict.ContainsKey(currentId.Value))
                {
                    break;
                }

                currentId = folderDict[currentId.Value].ParentFolderId;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking circular reference for folder {FolderId} with new parent {NewParentId}", folderId, newParentId);
            throw;
        }
    }

    public async Task<int> GetDepthAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            var folderDict = allFolders.ToDictionary(f => f.Id);

            if (!folderDict.ContainsKey(folderId))
            {
                throw new InvalidOperationException($"Folder {folderId} not found");
            }

            int depth = 0;
            var currentId = folderDict[folderId].ParentFolderId;

            while (currentId.HasValue)
            {
                depth++;

                if (!folderDict.ContainsKey(currentId.Value))
                {
                    break;
                }

                currentId = folderDict[currentId.Value].ParentFolderId;
            }

            return depth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting depth for folder {FolderId}", folderId);
            throw;
        }
    }

    public async Task<bool> IsEmptyAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await GetByIdAsync(folderId, cancellationToken);
            if (folder == null)
            {
                throw new InvalidOperationException($"Folder {folderId} not found");
            }

            var folderPath = GetFolderPath(folder);

            if (!Directory.Exists(folderPath))
            {
                return true;
            }

            // Check for subdirectories (excluding hidden folders like .folder-meta)
            var subDirectories = Directory.GetDirectories(folderPath);
            if (subDirectories.Length > 0)
            {
                return false;
            }

            // Check for .md files (templates)
            var mdFiles = Directory.GetFiles(folderPath, "*.md");
            if (mdFiles.Length > 0)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if folder {FolderId} is empty", folderId);
            throw;
        }
    }

    public async Task<Folder> CreateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        try
        {
            var folderPath = GetFolderPath(folder);

            // Create directory
            Directory.CreateDirectory(folderPath);
            _logger.LogInformation("Created directory at {Path}", folderPath);

            // Create .folder-meta file
            var metadata = new FolderMetadata
            {
                Id = folder.Id,
                Created = folder.CreatedAt,
                Updated = folder.UpdatedAt
            };

            var metaPath = Path.Combine(folderPath, FolderMetaFileName);
            var yaml = _yamlSerializer.Serialize(metadata);
            await File.WriteAllTextAsync(metaPath, yaml, cancellationToken);
            _logger.LogInformation("Created metadata file at {Path}", metaPath);

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder {FolderName}", folder.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFolders = await GetAllFoldersWithMetadataAsync(cancellationToken);
            var existingFolder = allFolders.FirstOrDefault(f => f.Id == folder.Id);

            if (existingFolder == null)
            {
                throw new InvalidOperationException($"Folder {folder.Id} not found");
            }

            var oldPath = GetFolderPath(existingFolder);
            var newPath = GetFolderPath(folder);

            // Move/rename directory if path changed
            if (oldPath != newPath)
            {
                Directory.Move(oldPath, newPath);
                _logger.LogInformation("Moved folder from {OldPath} to {NewPath}", oldPath, newPath);
            }

            // Update .folder-meta file
            var metadata = new FolderMetadata
            {
                Id = folder.Id,
                Created = folder.CreatedAt,
                Updated = folder.UpdatedAt
            };

            var metaPath = Path.Combine(newPath, FolderMetaFileName);
            var yaml = _yamlSerializer.Serialize(metadata);
            await File.WriteAllTextAsync(metaPath, yaml, cancellationToken);
            _logger.LogInformation("Updated metadata file at {Path}", metaPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderId}", folder.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await GetByIdAsync(id, cancellationToken);
            if (folder == null)
            {
                throw new InvalidOperationException($"Folder {id} not found");
            }

            var folderPath = GetFolderPath(folder);

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                _logger.LogInformation("Deleted folder at {Path}", folderPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", id);
            throw;
        }
    }

    /// <summary>
    /// Scans the filesystem and returns all folders with their metadata.
    /// </summary>
    private async Task<List<Folder>> GetAllFoldersWithMetadataAsync(CancellationToken cancellationToken = default)
    {
        var folders = new List<Folder>();
        await ScanDirectoryAsync(_rootPath, null, folders, cancellationToken);
        return folders;
    }

    /// <summary>
    /// Recursively scans a directory for folders with .folder-meta files.
    /// </summary>
    private async Task ScanDirectoryAsync(string directoryPath, Guid? parentId, List<Folder> folders, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        foreach (var subDirectory in Directory.GetDirectories(directoryPath))
        {
            var metaPath = Path.Combine(subDirectory, FolderMetaFileName);

            if (File.Exists(metaPath))
            {
                try
                {
                    var yaml = await File.ReadAllTextAsync(metaPath, cancellationToken);
                    var metadata = _yamlDeserializer.Deserialize<FolderMetadata>(yaml);

                    var folderName = Path.GetFileName(subDirectory);

                    // Count template files (.md) in this folder (not recursive)
                    var templateCount = Directory.GetFiles(subDirectory, "*.md", SearchOption.TopDirectoryOnly).Length;

                    var folder = new Folder
                    {
                        Id = metadata.Id,
                        Name = folderName,
                        ParentFolderId = parentId,
                        CreatedAt = metadata.Created,
                        UpdatedAt = metadata.Updated,
                        ChildFolders = new List<Folder>(),
                        Templates = CreatePlaceholderTemplates(templateCount)
                    };

                    folders.Add(folder);

                    // Recursively scan subdirectories
                    await ScanDirectoryAsync(subDirectory, folder.Id, folders, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse metadata file at {Path}", metaPath);
                }
            }
        }
    }

    /// <summary>
    /// Gets the filesystem path for a folder based on its hierarchy.
    /// </summary>
    private string GetFolderPath(Folder folder)
    {
        var sanitizedName = FileNameSanitizer.Sanitize(folder.Name);

        if (!folder.ParentFolderId.HasValue)
        {
            return Path.Combine(_rootPath, sanitizedName);
        }

        // Build path by traversing parent hierarchy
        // Note: This is simplified - in practice, we'd need to look up parent paths
        // For now, assume single-level nesting or that folder.ParentFolder is populated
        var allFolders = GetAllFoldersWithMetadataAsync().GetAwaiter().GetResult();
        var parent = allFolders.FirstOrDefault(f => f.Id == folder.ParentFolderId.Value);

        if (parent == null)
        {
            return Path.Combine(_rootPath, sanitizedName);
        }

        var parentPath = GetFolderPath(parent);
        return Path.Combine(parentPath, sanitizedName);
    }

    /// <summary>
    /// Creates a list of placeholder templates to represent the count.
    /// Used for efficiently populating the Templates collection for counting purposes.
    /// </summary>
    private List<Template> CreatePlaceholderTemplates(int count)
    {
        var templates = new List<Template>(count);
        for (int i = 0; i < count; i++)
        {
            templates.Add(new Template());
        }
        return templates;
    }
}
