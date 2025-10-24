using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PromptTemplateManager.Application.Services;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Infrastructure.Repositories;

namespace PromptTemplateManager.Tests.Integration.Controllers;

/// <summary>
/// Contract tests for Search API to verify metadata fields are properly returned in search results.
/// </summary>
public class SearchControllerTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileSystemTemplateRepository _repository;
    private readonly TemplateService _templateService;

    public SearchControllerTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _repository = new FileSystemTemplateRepository(
            _tempDirectory,
            NullLogger<FileSystemTemplateRepository>.Instance
        );

        _templateService = new TemplateService(_repository);
    }

    [Fact]
    public async Task SearchTemplatesAsync_ReturnsTemplatesWithMetadata()
    {
        // Arrange - Create templates with metadata
        var beforeFirstCreate = DateTime.UtcNow;
        var oldTemplate = await _templateService.CreateAsync(
            "Old Meeting Template",
            "This template is about meetings and planning",
            null
        );
        var afterFirstCreate = DateTime.UtcNow;

        await Task.Delay(100); // Small delay to ensure different timestamps

        var beforeSecondCreate = DateTime.UtcNow;
        var recentTemplate = await _templateService.CreateAsync(
            "Recent Meeting Notes",
            "Current meeting template with planning items",
            null
        );
        var afterSecondCreate = DateTime.UtcNow;

        // Act - Search for templates
        var (items, totalCount) = await _templateService.SearchTemplatesAsync("meeting", 1, 10);

        // Assert - Verify metadata is included in results
        var resultList = items.ToList();
        resultList.Should().HaveCount(2);
        totalCount.Should().Be(2);

        // Verify all templates have metadata populated
        foreach (var template in resultList)
        {
            template.Id.Should().NotBeEmpty("Id should be populated");
            template.Name.Should().NotBeNullOrEmpty("Name should be populated");
            template.Content.Should().NotBeNullOrEmpty("Content should be populated");
            template.CreatedAt.Should().NotBe(default(DateTime), "CreatedAt should be populated");
            template.UpdatedAt.Should().NotBe(default(DateTime), "UpdatedAt should be populated");
        }

        // Verify specific template metadata - timestamps should be within creation windows
        var oldResult = resultList.FirstOrDefault(t => t.Name == "Old Meeting Template");
        oldResult.Should().NotBeNull();
        oldResult!.CreatedAt.Should().BeOnOrAfter(beforeFirstCreate).And.BeOnOrBefore(afterFirstCreate);
        oldResult.UpdatedAt.Should().BeOnOrAfter(beforeFirstCreate).And.BeOnOrBefore(afterFirstCreate);

        var recentResult = resultList.FirstOrDefault(t => t.Name == "Recent Meeting Notes");
        recentResult.Should().NotBeNull();
        recentResult!.CreatedAt.Should().BeOnOrAfter(beforeSecondCreate).And.BeOnOrBefore(afterSecondCreate);
        recentResult.UpdatedAt.Should().BeOnOrAfter(beforeSecondCreate).And.BeOnOrBefore(afterSecondCreate);
    }

    [Fact]
    public async Task SearchTemplatesAsync_WithPagination_PreservesMetadata()
    {
        // Arrange - Create multiple templates
        for (int i = 1; i <= 5; i++)
        {
            var template = await _templateService.CreateAsync(
                $"Template {i}",
                $"Content about project planning item {i}",
                null
            );

            // Set different timestamps for each template
            template.CreatedAt = DateTime.UtcNow.AddDays(-i);
            template.UpdatedAt = DateTime.UtcNow.AddDays(-i);
            await _repository.UpdateAsync(template);
        }

        // Act - Search with pagination
        var (page1Items, page1Total) = await _templateService.SearchTemplatesAsync("project", 1, 2);
        var (page2Items, page2Total) = await _templateService.SearchTemplatesAsync("project", 2, 2);

        // Assert
        var page1List = page1Items.ToList();
        var page2List = page2Items.ToList();

        page1List.Should().HaveCount(2);
        page2List.Should().HaveCount(2);
        page1Total.Should().Be(5);
        page2Total.Should().Be(5);

        // Verify all pages have metadata
        foreach (var template in page1List.Concat(page2List))
        {
            template.Id.Should().NotBeEmpty();
            template.CreatedAt.Should().NotBe(default(DateTime));
            template.UpdatedAt.Should().NotBe(default(DateTime));
        }
    }

    [Fact]
    public async Task SearchTemplatesAsync_WithNoResults_ReturnsEmptyWithValidMetadataStructure()
    {
        // Arrange - Create template that won't match search
        await _templateService.CreateAsync(
            "Unrelated Template",
            "This has nothing to do with the search",
            null
        );

        // Act - Search for non-existent term
        var (items, totalCount) = await _templateService.SearchTemplatesAsync("nonexistent", 1, 10);

        // Assert
        items.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchTemplatesAsync_PreservesIdAcrossMultipleSearches()
    {
        // Arrange - Create template
        var createdTemplate = await _templateService.CreateAsync(
            "Persistent Search Template",
            "Content for search persistence test",
            null
        );

        // Act - Perform same search twice
        var (firstSearchItems, _) = await _templateService.SearchTemplatesAsync("persistence", 1, 10);
        var (secondSearchItems, _) = await _templateService.SearchTemplatesAsync("persistence", 1, 10);

        // Assert - Same template ID returned in both searches
        var firstResult = firstSearchItems.First();
        var secondResult = secondSearchItems.First();

        firstResult.Id.Should().Be(createdTemplate.Id);
        secondResult.Id.Should().Be(createdTemplate.Id);
        firstResult.Id.Should().Be(secondResult.Id, "Template ID should be consistent across searches");
    }

    [Fact]
    public async Task SearchTemplatesAsync_AfterUpdate_ReflectsNewMetadata()
    {
        // Arrange - Create and search for template
        var template = await _templateService.CreateAsync(
            "Original Search Name",
            "Original content for searching",
            null
        );

        var originalCreatedAt = template.CreatedAt;
        await Task.Delay(100); // Small delay to ensure UpdatedAt differs

        // Act - Update template and search again
        var updatedTemplate = await _templateService.UpdateAsync(
            template.Id,
            "Updated Search Name",
            "Updated content for searching",
            null
        );

        var (searchResults, _) = await _templateService.SearchTemplatesAsync("Updated", 1, 10);

        // Assert - Metadata reflects update
        var result = searchResults.First();
        result.Name.Should().Be("Updated Search Name");
        result.Content.Should().Be("Updated content for searching");
        result.CreatedAt.Should().BeCloseTo(originalCreatedAt, TimeSpan.FromSeconds(1), "CreatedAt should not change");
        result.UpdatedAt.Should().BeAfter(originalCreatedAt, "UpdatedAt should be newer than CreatedAt");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
