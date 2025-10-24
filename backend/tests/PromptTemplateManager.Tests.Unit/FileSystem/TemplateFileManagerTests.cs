using FluentAssertions;
using PromptTemplateManager.Infrastructure.FileSystem;

namespace PromptTemplateManager.Tests.Unit.FileSystem;

public class TemplateFileManagerTests
{
    [Fact]
    public void ParseTemplateFile_WithValidFrontmatter_ParsesCorrectly()
    {
        // Arrange
        var fileContents = @"---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:00:00.0000000Z
updated: 2025-10-22T15:00:00.0000000Z
---
This is the template content with {{placeholder}} syntax.";

        // Act
        var result = TemplateFileManager.ParseTemplateFile(fileContents);

        // Assert
        result.Metadata.Id.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        result.Metadata.Created.Should().Be(DateTime.Parse("2025-10-22T10:00:00.0000000Z").ToUniversalTime());
        result.Metadata.Updated.Should().Be(DateTime.Parse("2025-10-22T15:00:00.0000000Z").ToUniversalTime());
        result.Content.Should().Be("This is the template content with {{placeholder}} syntax.");
    }

    [Fact]
    public void ParseTemplateFile_WithMultilineContent_ParsesCorrectly()
    {
        // Arrange
        var fileContents = @"---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:00:00.0000000Z
updated: 2025-10-22T15:00:00.0000000Z
---
Line 1
Line 2
Line 3";

        // Act
        var result = TemplateFileManager.ParseTemplateFile(fileContents);

        // Assert
        result.Content.Should().Be("Line 1\nLine 2\nLine 3");
    }

    [Fact]
    public void ParseTemplateFile_WithMissingFrontmatter_ThrowsException()
    {
        // Arrange
        var fileContents = "This is just content without frontmatter";

        // Act
        Action act = () => TemplateFileManager.ParseTemplateFile(fileContents);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing or invalid YAML frontmatter");
    }

    [Fact]
    public void ParseTemplateFile_WithInvalidYaml_ThrowsException()
    {
        // Arrange
        var fileContents = @"---
id: not-a-guid
created: invalid-date
updated: invalid-date
---
Content";

        // Act
        Action act = () => TemplateFileManager.ParseTemplateFile(fileContents);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to parse YAML frontmatter*");
    }

    [Fact]
    public void ParseTemplateFile_WithEmptyContent_ParsesCorrectly()
    {
        // Arrange
        var fileContents = @"---
id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:00:00.0000000Z
updated: 2025-10-22T15:00:00.0000000Z
---
";

        // Act
        var result = TemplateFileManager.ParseTemplateFile(fileContents);

        // Assert
        result.Content.Should().Be("");
    }

    [Fact]
    public void SerializeTemplateFile_WithMetadataAndContent_GeneratesCorrectFormat()
    {
        // Arrange
        var metadata = new TemplateMetadata
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Created = DateTime.Parse("2025-10-22T10:00:00.0000000Z").ToUniversalTime(),
            Updated = DateTime.Parse("2025-10-22T15:00:00.0000000Z").ToUniversalTime()
        };
        var content = "This is the template content";

        // Act
        var result = TemplateFileManager.SerializeTemplateFile(metadata, content);

        // Assert
        result.Should().Contain("---");
        result.Should().Contain("id: 3fa85f64-5717-4562-b3fc-2c963f66afa6");
        result.Should().Contain("created:");
        result.Should().Contain("updated:");
        result.Should().Contain("This is the template content");
    }

    [Fact]
    public void SerializeTemplateFile_RoundTrip_PreservesData()
    {
        // Arrange
        var originalMetadata = new TemplateMetadata
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };
        var originalContent = "Test content with {{placeholder}}";

        // Act
        var serialized = TemplateFileManager.SerializeTemplateFile(originalMetadata, originalContent);
        var (parsedMetadata, parsedContent) = TemplateFileManager.ParseTemplateFile(serialized);

        // Assert
        parsedMetadata.Id.Should().Be(originalMetadata.Id);
        parsedMetadata.Created.Should().BeCloseTo(originalMetadata.Created, TimeSpan.FromSeconds(1));
        parsedMetadata.Updated.Should().BeCloseTo(originalMetadata.Updated, TimeSpan.FromSeconds(1));
        parsedContent.Should().Be(originalContent);
    }

    [Fact]
    public async Task ReadFileAsync_WithExistingFile_ReadsContentCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var expectedContent = "Test file content";
        await File.WriteAllTextAsync(tempFile, expectedContent);

        try
        {
            // Act
            var result = await TemplateFileManager.ReadFileAsync(tempFile);

            // Assert
            result.Should().Be(expectedContent);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFileAsync_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        // Act
        Func<Task> act = async () => await TemplateFileManager.ReadFileAsync(nonExistentFile);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task WriteFileAsync_WithValidContent_WritesFileCorrectly()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        var content = "Test content to write";

        try
        {
            // Act
            await TemplateFileManager.WriteFileAsync(tempFile, content);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var readContent = await File.ReadAllTextAsync(tempFile);
            readContent.Should().Be(content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task WriteFileAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempFile = Path.Combine(tempDir, "test.txt");
        var content = "Test content";

        try
        {
            // Act
            await TemplateFileManager.WriteFileAsync(tempFile, content);

            // Assert
            Directory.Exists(tempDir).Should().BeTrue();
            File.Exists(tempFile).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ReadFolderMetadataAsync_WithValidYaml_ParsesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var yaml = @"id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
created: 2025-10-22T10:00:00.0000000Z
updated: 2025-10-22T15:00:00.0000000Z";
        await File.WriteAllTextAsync(tempFile, yaml);

        try
        {
            // Act
            var result = await TemplateFileManager.ReadFolderMetadataAsync(tempFile);

            // Assert
            result.Id.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
            result.Created.Should().Be(DateTime.Parse("2025-10-22T10:00:00.0000000Z").ToUniversalTime());
            result.Updated.Should().Be(DateTime.Parse("2025-10-22T15:00:00.0000000Z").ToUniversalTime());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task WriteFolderMetadataAsync_WithValidMetadata_WritesCorrectly()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml");
        var metadata = new FolderMetadata
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Created = DateTime.Parse("2025-10-22T10:00:00.0000000Z").ToUniversalTime(),
            Updated = DateTime.Parse("2025-10-22T15:00:00.0000000Z").ToUniversalTime()
        };

        try
        {
            // Act
            await TemplateFileManager.WriteFolderMetadataAsync(tempFile, metadata);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Contain("id: 3fa85f64-5717-4562-b3fc-2c963f66afa6");
            content.Should().Contain("created:");
            content.Should().Contain("updated:");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task FolderMetadata_RoundTrip_PreservesData()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".yaml");
        var originalMetadata = new FolderMetadata
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        try
        {
            // Act
            await TemplateFileManager.WriteFolderMetadataAsync(tempFile, originalMetadata);
            var parsedMetadata = await TemplateFileManager.ReadFolderMetadataAsync(tempFile);

            // Assert
            parsedMetadata.Id.Should().Be(originalMetadata.Id);
            parsedMetadata.Created.Should().BeCloseTo(originalMetadata.Created, TimeSpan.FromSeconds(1));
            parsedMetadata.Updated.Should().BeCloseTo(originalMetadata.Updated, TimeSpan.FromSeconds(1));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
