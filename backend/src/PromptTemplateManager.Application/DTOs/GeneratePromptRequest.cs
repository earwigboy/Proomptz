namespace PromptTemplateManager.Application.DTOs;

public class GeneratePromptRequest
{
    public required Dictionary<string, string> PlaceholderValues { get; set; }
}
