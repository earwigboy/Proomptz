namespace PromptTemplateManager.Application.DTOs;

public class PlaceholderListResponse
{
    public required string TemplateId { get; set; }
    public required string TemplateName { get; set; }
    public required List<PlaceholderInfo> Placeholders { get; set; }
}
