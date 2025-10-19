using System.Text.RegularExpressions;
using PromptTemplateManager.Application.DTOs;
using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Application.Services;

public class PlaceholderService : IPlaceholderService
{
    // Regex pattern to match {{placeholder_name}}
    private static readonly Regex PlaceholderRegex = new(@"\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}", RegexOptions.Compiled);

    public List<string> ExtractPlaceholderNames(string templateContent)
    {
        var placeholders = new HashSet<string>();
        var matches = PlaceholderRegex.Matches(templateContent);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                placeholders.Add(match.Groups[1].Value);
            }
        }

        return placeholders.OrderBy(p => p).ToList();
    }

    public string GeneratePrompt(string templateContent, Dictionary<string, string> placeholderValues)
    {
        var result = templateContent;

        foreach (var kvp in placeholderValues)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            result = result.Replace(placeholder, kvp.Value);
        }

        return result;
    }

    public bool ValidatePlaceholderValues(List<string> requiredPlaceholders, Dictionary<string, string> providedValues)
    {
        foreach (var required in requiredPlaceholders)
        {
            if (!providedValues.ContainsKey(required) || string.IsNullOrWhiteSpace(providedValues[required]))
            {
                return false;
            }
        }

        return true;
    }
}
