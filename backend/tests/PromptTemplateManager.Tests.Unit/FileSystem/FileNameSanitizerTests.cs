using FluentAssertions;
using PromptTemplateManager.Infrastructure.FileSystem;

namespace PromptTemplateManager.Tests.Unit.FileSystem;

public class FileNameSanitizerTests
{
    [Fact]
    public void Sanitize_WithIllegalCharacters_ReplacesWithSafeAlternatives()
    {
        // Arrange
        var input = "My Template: Draft #2";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("My Template - Draft #2");
    }

    [Fact]
    public void Sanitize_WithForwardSlash_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Before/After";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Before_After");
    }

    [Fact]
    public void Sanitize_WithBackslash_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Path\\To\\Template";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Path_To_Template");
    }

    [Fact]
    public void Sanitize_WithAngleBrackets_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "<Template>";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("_Template_");
    }

    [Fact]
    public void Sanitize_WithQuotes_ReplacesWithSingleQuote()
    {
        // Arrange
        var input = "\"Quoted Template\"";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("'Quoted Template'");
    }

    [Fact]
    public void Sanitize_WithPipeCharacter_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Template|Version2";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Template_Version2");
    }

    [Fact]
    public void Sanitize_WithQuestionMark_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Is this valid?";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Is this valid_");
    }

    [Fact]
    public void Sanitize_WithAsterisk_ReplacesWithUnderscore()
    {
        // Arrange
        var input = "Template*";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Template_");
    }

    [Fact]
    public void Sanitize_WithControlCharacters_RemovesControlCharacters()
    {
        // Arrange
        var input = "Template\nWith\tControl\rCharacters";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("TemplateWithControlCharacters");
    }

    [Fact]
    public void Sanitize_WithLeadingAndTrailingSpaces_TrimsSpaces()
    {
        // Arrange
        var input = "  Template Name  ";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Template Name");
    }

    [Fact]
    public void Sanitize_WithLeadingAndTrailingDots_TrimsDots()
    {
        // Arrange
        var input = "...Template Name...";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Template Name");
    }

    [Fact]
    public void Sanitize_WithEmptyString_ReturnsUntitled()
    {
        // Arrange
        var input = "";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Untitled");
    }

    [Fact]
    public void Sanitize_WithWhitespaceOnly_ReturnsUntitled()
    {
        // Arrange
        var input = "   ";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Untitled");
    }

    [Fact]
    public void Sanitize_WithWindowsReservedName_PrependsUnderscore()
    {
        // Arrange
        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };

        // Act & Assert
        foreach (var name in reservedNames)
        {
            var result = FileNameSanitizer.Sanitize(name);
            result.Should().Be("_" + name);
        }
    }

    [Fact]
    public void Sanitize_WithVeryLongName_TruncatesTo200Characters()
    {
        // Arrange
        var input = new string('a', 250);

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().HaveLength(200);
    }

    [Fact]
    public void Sanitize_WithUnicodeCharacters_PreservesUnicode()
    {
        // Arrange
        var input = "Template 日本語 🎉";

        // Act
        var result = FileNameSanitizer.Sanitize(input);

        // Assert
        result.Should().Be("Template 日本語 🎉");
    }

    [Fact]
    public void GetUniqueFileName_WhenFileDoesNotExist_ReturnsOriginalName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = FileNameSanitizer.GetUniqueFileName(tempDir, "Test Template", ".md");

            // Assert
            result.Should().Be("Test Template.md");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetUniqueFileName_WhenFileExists_AppendsNumber()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Test Template.md"), "");

            // Act
            var result = FileNameSanitizer.GetUniqueFileName(tempDir, "Test Template", ".md");

            // Assert
            result.Should().Be("Test Template (2).md");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetUniqueFileName_WithMultipleExistingFiles_AppendsCorrectNumber()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Test Template.md"), "");
            File.WriteAllText(Path.Combine(tempDir, "Test Template (2).md"), "");
            File.WriteAllText(Path.Combine(tempDir, "Test Template (3).md"), "");

            // Act
            var result = FileNameSanitizer.GetUniqueFileName(tempDir, "Test Template", ".md");

            // Assert
            result.Should().Be("Test Template (4).md");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ContainsIllegalCharacters_WithIllegalCharacters_ReturnsTrue()
    {
        // Arrange
        var input = "Template<Name>";

        // Act
        var result = FileNameSanitizer.ContainsIllegalCharacters(input);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsIllegalCharacters_WithValidCharacters_ReturnsFalse()
    {
        // Arrange
        var input = "Valid Template Name";

        // Act
        var result = FileNameSanitizer.ContainsIllegalCharacters(input);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsIllegalCharacters_WithControlCharacters_ReturnsTrue()
    {
        // Arrange
        var input = "Template\nName";

        // Act
        var result = FileNameSanitizer.ContainsIllegalCharacters(input);

        // Assert
        result.Should().BeTrue();
    }
}
