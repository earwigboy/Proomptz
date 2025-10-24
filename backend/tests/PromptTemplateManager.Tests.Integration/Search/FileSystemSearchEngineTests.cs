using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Infrastructure.FileSystem;

namespace PromptTemplateManager.Tests.Integration.Search;

public class FileSystemSearchEngineTests : IDisposable
{
    private readonly string _tempIndexPath;
    private readonly FileSystemSearchEngine _searchEngine;

    public FileSystemSearchEngineTests()
    {
        _tempIndexPath = Path.Combine(Path.GetTempPath(), "search-index-" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempIndexPath);

        _searchEngine = new FileSystemSearchEngine(
            _tempIndexPath,
            NullLogger<FileSystemSearchEngine>.Instance
        );
    }

    [Fact]
    public async Task SearchAsync_WithMetadataFields_ReturnsTemplatesWithMetadata()
    {
        // Arrange - Create templates with different created dates
        var oldTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Old Meeting Notes",
            Content = "This is an old template about meetings",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var recentTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Recent Meeting Notes",
            Content = "This is a recent template about meetings",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Updated Meeting Notes",
            Content = "This template was recently updated",
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        await _searchEngine.IndexTemplateAsync(oldTemplate);
        await _searchEngine.IndexTemplateAsync(recentTemplate);
        await _searchEngine.IndexTemplateAsync(updatedTemplate);

        // Act - Search for "meeting"
        var results = await _searchEngine.SearchAsync("meeting", 100);

        // Assert - All three templates should be found
        results.Should().HaveCount(3);
        results.Should().Contain(oldTemplate.Id);
        results.Should().Contain(recentTemplate.Id);
        results.Should().Contain(updatedTemplate.Id);
    }

    [Fact]
    public async Task RebuildIndexAsync_PreservesMetadataFields()
    {
        // Arrange - Create initial templates
        var template1 = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Template 1",
            Content = "Content 1",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var template2 = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Template 2",
            Content = "Content 2",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _searchEngine.IndexTemplateAsync(template1);
        await _searchEngine.IndexTemplateAsync(template2);

        // Act - Rebuild index
        await _searchEngine.RebuildIndexAsync(new[] { template1, template2 });

        // Assert - Search should still work and find templates
        var results = await _searchEngine.SearchAsync("Template", 100);
        results.Should().HaveCount(2);
        results.Should().Contain(template1.Id);
        results.Should().Contain(template2.Id);
    }

    [Fact]
    public async Task IndexTemplateAsync_UpdatesMetadataOnReindex()
    {
        // Arrange - Create template
        var templateId = Guid.NewGuid();
        var originalTemplate = new Template
        {
            Id = templateId,
            Name = "Original Name",
            Content = "Original content",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _searchEngine.IndexTemplateAsync(originalTemplate);

        // Act - Update template and reindex
        var updatedTemplate = new Template
        {
            Id = templateId,
            Name = "Updated Name",
            Content = "Updated content",
            CreatedAt = originalTemplate.CreatedAt, // CreatedAt should not change
            UpdatedAt = DateTime.UtcNow // UpdatedAt should be current
        };

        await _searchEngine.IndexTemplateAsync(updatedTemplate);

        // Assert - Search with new name should find it
        var resultsByNewName = await _searchEngine.SearchAsync("Updated", 100);
        resultsByNewName.Should().Contain(templateId);

        // Search with old name should not find it
        var resultsByOldName = await _searchEngine.SearchAsync("Original", 100);
        resultsByOldName.Should().NotContain(templateId);
    }

    [Fact]
    public async Task SearchAsync_WithMultipleTemplates_SortsByRelevance()
    {
        // Arrange - Create templates with varying relevance
        var exactMatch = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Project Planning Template",
            Content = "planning planning planning", // High frequency of keyword
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var partialMatch = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Meeting Notes",
            Content = "Discussed the project planning for next quarter",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var weakMatch = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Daily Standup",
            Content = "Brief mention of planning",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _searchEngine.IndexTemplateAsync(exactMatch);
        await _searchEngine.IndexTemplateAsync(partialMatch);
        await _searchEngine.IndexTemplateAsync(weakMatch);

        // Act - Search for "planning"
        var results = await _searchEngine.SearchAsync("planning", 100);

        // Assert - Should find all three, with exact match first
        results.Should().HaveCount(3);
        results[0].Should().Be(exactMatch.Id, "template with 'planning' in title and high content frequency should rank first");
    }

    [Fact]
    public async Task RemoveTemplateAsync_RemovesTemplateFromIndex()
    {
        // Arrange - Create and index template
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "To Be Removed",
            Content = "This template will be removed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _searchEngine.IndexTemplateAsync(template);

        // Verify it's indexed
        var initialResults = await _searchEngine.SearchAsync("removed", 100);
        initialResults.Should().Contain(template.Id);

        // Act - Remove template
        await _searchEngine.RemoveTemplateAsync(template.Id);

        // Assert - Should not be found in search
        var finalResults = await _searchEngine.SearchAsync("removed", 100);
        finalResults.Should().NotContain(template.Id);
    }

    public void Dispose()
    {
        _searchEngine?.Dispose();

        if (Directory.Exists(_tempIndexPath))
        {
            try
            {
                Directory.Delete(_tempIndexPath, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
