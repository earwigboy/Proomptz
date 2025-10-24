using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Infrastructure.Repositories;

namespace PromptTemplateManager.Tests.Integration.Repositories;

public class FileSystemTemplateRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileSystemTemplateRepository _repository;

    public FileSystemTemplateRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _repository = new FileSystemTemplateRepository(
            _tempDirectory,
            NullLogger<FileSystemTemplateRepository>.Instance
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidTemplate_CreatesFileWithFrontmatter()
    {
        // Arrange
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Content = "This is test content with {{placeholder}}",
            FolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(template);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(template.Id);
        result.Name.Should().Be(template.Name);

        // Verify file exists
        var expectedPath = Path.Combine(_tempDirectory, "Test Template.md");
        File.Exists(expectedPath).Should().BeTrue();

        // Verify file contains frontmatter and content
        var fileContents = await File.ReadAllTextAsync(expectedPath);
        fileContents.Should().Contain("---");
        fileContents.Should().Contain(template.Id.ToString());
        fileContents.Should().Contain(template.Content);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTemplate_ReturnsTemplate()
    {
        // Arrange
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Existing Template",
            Content = "Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(template);

        // Act
        var result = await _repository.GetByIdAsync(template.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(template.Id);
        result.Name.Should().Be(template.Name);
        result.Content.Should().Be(template.Content);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingTemplate_UpdatesFile()
    {
        // Arrange
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Content = "Original Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(template);

        // Act
        template.Name = "Updated Name";
        template.Content = "Updated Content";
        var result = await _repository.UpdateAsync(template);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Content.Should().Be("Updated Content");

        // Verify old file doesn't exist
        var oldPath = Path.Combine(_tempDirectory, "Original Name.md");
        File.Exists(oldPath).Should().BeFalse();

        // Verify new file exists
        var newPath = Path.Combine(_tempDirectory, "Updated Name.md");
        File.Exists(newPath).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingTemplate_RemovesFile()
    {
        // Arrange
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Content = "Content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(template);
        var filePath = Path.Combine(_tempDirectory, "To Delete.md");
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _repository.DeleteAsync(template.Id);

        // Assert
        File.Exists(filePath).Should().BeFalse();
        var retrieved = await _repository.GetByIdAsync(template.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleTemplates_ReturnsAll()
    {
        // Arrange
        var template1 = new Template { Id = Guid.NewGuid(), Name = "Template 1", Content = "Content 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var template2 = new Template { Id = Guid.NewGuid(), Name = "Template 2", Content = "Content 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _repository.CreateAsync(template1);
        await _repository.CreateAsync(template2);

        // Act
        var result = await _repository.GetAllAsync(null, 1, 100);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Template 1");
        result.Should().Contain(t => t.Name == "Template 2");
    }

    [Fact]
    public async Task MetadataPersistence_AcrossApplicationRestarts_PreservesIdAndTimestamps()
    {
        // Arrange - Create template with specific metadata
        var originalId = Guid.NewGuid();
        var beforeCreate = DateTime.UtcNow;

        var template = new Template
        {
            Id = originalId,
            Name = "Persistent Template",
            Content = "Content that persists across restarts",
            CreatedAt = beforeCreate,
            UpdatedAt = beforeCreate
        };

        await _repository.CreateAsync(template);
        var afterCreate = DateTime.UtcNow;

        // Simulate application restart by creating a new repository instance pointing to same directory
        var newRepository = new FileSystemTemplateRepository(
            _tempDirectory,
            NullLogger<FileSystemTemplateRepository>.Instance
        );

        // Act - Retrieve template using new repository instance
        var retrievedById = await newRepository.GetByIdAsync(originalId);
        var allTemplates = await newRepository.GetAllAsync(null, 1, 100);

        // Assert - Verify metadata is preserved
        retrievedById.Should().NotBeNull();
        retrievedById!.Id.Should().Be(originalId, "ID should persist across restarts");
        retrievedById.Name.Should().Be("Persistent Template");
        retrievedById.Content.Should().Be("Content that persists across restarts");

        // Timestamps should be preserved - CreatedAt and UpdatedAt should be within the creation window
        retrievedById.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        retrievedById.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);

        // Verify template appears in GetAllAsync with correct metadata
        allTemplates.Should().HaveCount(1);
        var templateFromList = allTemplates.First();
        templateFromList.Id.Should().Be(originalId, "ID should be consistent in list");
        templateFromList.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        templateFromList.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
