namespace PromptTemplateManager.Application.DTOs;

public class TemplateListResponse
{
    public IEnumerable<TemplateSummary> Items { get; set; } = new List<TemplateSummary>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}
