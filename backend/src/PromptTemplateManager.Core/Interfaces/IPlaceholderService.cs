namespace PromptTemplateManager.Core.Interfaces;

public interface IPlaceholderService
{
    /// <summary>
    /// Extracts all placeholder names from a template's content
    /// </summary>
    List<string> ExtractPlaceholderNames(string templateContent);

    /// <summary>
    /// Generates a prompt by replacing placeholders with provided values
    /// </summary>
    string GeneratePrompt(string templateContent, Dictionary<string, string> placeholderValues);

    /// <summary>
    /// Validates that all required placeholders have values
    /// </summary>
    bool ValidatePlaceholderValues(List<string> requiredPlaceholders, Dictionary<string, string> providedValues);
}
