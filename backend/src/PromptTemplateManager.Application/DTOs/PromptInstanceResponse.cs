namespace PromptTemplateManager.Application.DTOs;

public class PromptInstanceResponse
{
    public required string TemplateId { get; set; }
    public required string TemplateName { get; set; }
    public required string GeneratedPrompt { get; set; }
    public required Dictionary<string, string> PlaceholderValues { get; set; }
}
