using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Infrastructure.Repositories;

namespace PromptTemplateManager.Tests.Integration.Repositories;

public class FileSystemFolderRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileSystemFolderRepository _repository;

    public FileSystemFolderRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _repository = new FileSystemFolderRepository(
            _tempDirectory,
            NullLogger<FileSystemFolderRepository>.Instance
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidFolder_CreatesDirectoryWithMetadata()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Test Folder",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(folder);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(folder.Id);
        result.Name.Should().Be(folder.Name);

        // Verify directory exists
        var expectedPath = Path.Combine(_tempDirectory, "Test Folder");
        Directory.Exists(expectedPath).Should().BeTrue();

        // Verify .folder-meta exists
        var metaPath = Path.Combine(expectedPath, ".folder-meta");
        File.Exists(metaPath).Should().BeTrue();

        // Verify metadata contains ID and timestamps
        var metaContents = await File.ReadAllTextAsync(metaPath);
        metaContents.Should().Contain(folder.Id.ToString());
    }

    [Fact]
    public async Task CreateAsync_WithNestedFolder_CreatesInParentDirectory()
    {
        // Arrange
        var parentFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Parent",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(parentFolder);

        var childFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Child",
            ParentFolderId = parentFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(childFolder);

        // Assert
        result.Should().NotBeNull();
        var expectedPath = Path.Combine(_tempDirectory, "Parent", "Child");
        Directory.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingFolder_ReturnsFolder()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Existing Folder",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(folder);

        // Act
        var result = await _repository.GetByIdAsync(folder.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(folder.Id);
        result.Name.Should().Be(folder.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentFolder_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleFolders_ReturnsAll()
    {
        // Arrange
        var folder1 = new Folder { Id = Guid.NewGuid(), Name = "Folder 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var folder2 = new Folder { Id = Guid.NewGuid(), Name = "Folder 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(folder1);
        await _repository.CreateAsync(folder2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name == "Folder 1");
        result.Should().Contain(f => f.Name == "Folder 2");
    }

    [Fact]
    public async Task GetTreeAsync_WithNestedFolders_ReturnsHierarchicalStructure()
    {
        // Arrange
        var parent = new Folder { Id = Guid.NewGuid(), Name = "Parent", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(parent);

        var child = new Folder { Id = Guid.NewGuid(), Name = "Child", ParentFolderId = parent.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(child);

        // Act
        var result = await _repository.GetTreeAsync();

        // Assert
        result.Should().HaveCount(1); // Only root-level folders
        result[0].Name.Should().Be("Parent");
        result[0].ChildFolders.Should().HaveCount(1);
        result[0].ChildFolders.First().Name.Should().Be("Child");
    }

    [Fact]
    public async Task UpdateAsync_WithRename_RenamesDirectory()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(folder);

        // Act
        folder.Name = "Updated Name";
        await _repository.UpdateAsync(folder);

        // Assert
        var oldPath = Path.Combine(_tempDirectory, "Original Name");
        Directory.Exists(oldPath).Should().BeFalse();

        var newPath = Path.Combine(_tempDirectory, "Updated Name");
        Directory.Exists(newPath).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithMove_MovesDirectoryToNewParent()
    {
        // Arrange
        var parent1 = new Folder { Id = Guid.NewGuid(), Name = "Parent1", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(parent1);

        var parent2 = new Folder { Id = Guid.NewGuid(), Name = "Parent2", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(parent2);

        var child = new Folder { Id = Guid.NewGuid(), Name = "Child", ParentFolderId = parent1.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(child);

        // Act
        child.ParentFolderId = parent2.Id;
        await _repository.UpdateAsync(child);

        // Assert
        var oldPath = Path.Combine(_tempDirectory, "Parent1", "Child");
        Directory.Exists(oldPath).Should().BeFalse();

        var newPath = Path.Combine(_tempDirectory, "Parent2", "Child");
        Directory.Exists(newPath).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyFolder_RemovesDirectoryAndMetadata()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(folder);
        var folderPath = Path.Combine(_tempDirectory, "To Delete");
        Directory.Exists(folderPath).Should().BeTrue();

        // Act
        await _repository.DeleteAsync(folder.Id);

        // Assert
        Directory.Exists(folderPath).Should().BeFalse();
        var retrieved = await _repository.GetByIdAsync(folder.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task HasCircularReferenceAsync_WithCircularReference_ReturnsTrue()
    {
        // Arrange
        var grandparent = new Folder { Id = Guid.NewGuid(), Name = "Grandparent", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(grandparent);

        var parent = new Folder { Id = Guid.NewGuid(), Name = "Parent", ParentFolderId = grandparent.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(parent);

        var child = new Folder { Id = Guid.NewGuid(), Name = "Child", ParentFolderId = parent.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(child);

        // Act - Try to move parent to be child of child (circular)
        var hasCircular = await _repository.HasCircularReferenceAsync(parent.Id, child.Id);

        // Assert
        hasCircular.Should().BeTrue();
    }

    [Fact]
    public async Task HasCircularReferenceAsync_WithoutCircularReference_ReturnsFalse()
    {
        // Arrange
        var folder1 = new Folder { Id = Guid.NewGuid(), Name = "Folder1", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(folder1);

        var folder2 = new Folder { Id = Guid.NewGuid(), Name = "Folder2", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(folder2);

        // Act - Move folder1 to folder2 (no circular reference)
        var hasCircular = await _repository.HasCircularReferenceAsync(folder1.Id, folder2.Id);

        // Assert
        hasCircular.Should().BeFalse();
    }

    [Fact]
    public async Task GetDepthAsync_WithNestedFolders_ReturnsCorrectDepth()
    {
        // Arrange
        var level0 = new Folder { Id = Guid.NewGuid(), Name = "Level0", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(level0);

        var level1 = new Folder { Id = Guid.NewGuid(), Name = "Level1", ParentFolderId = level0.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(level1);

        var level2 = new Folder { Id = Guid.NewGuid(), Name = "Level2", ParentFolderId = level1.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(level2);

        // Act
        var depth0 = await _repository.GetDepthAsync(level0.Id);
        var depth1 = await _repository.GetDepthAsync(level1.Id);
        var depth2 = await _repository.GetDepthAsync(level2.Id);

        // Assert
        depth0.Should().Be(0); // Root level
        depth1.Should().Be(1);
        depth2.Should().Be(2);
    }

    [Fact]
    public async Task IsEmptyAsync_WithEmptyFolder_ReturnsTrue()
    {
        // Arrange
        var folder = new Folder { Id = Guid.NewGuid(), Name = "Empty", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(folder);

        // Act
        var isEmpty = await _repository.IsEmptyAsync(folder.Id);

        // Assert
        isEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmptyAsync_WithSubfolder_ReturnsFalse()
    {
        // Arrange
        var parent = new Folder { Id = Guid.NewGuid(), Name = "Parent", ParentFolderId = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(parent);

        var child = new Folder { Id = Guid.NewGuid(), Name = "Child", ParentFolderId = parent.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(child);

        // Act
        var isEmpty = await _repository.IsEmptyAsync(parent.Id);

        // Assert
        isEmpty.Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
