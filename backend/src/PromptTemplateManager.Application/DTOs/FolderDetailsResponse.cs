namespace PromptTemplateManager.Application.DTOs;

public class FolderDetailsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public string? ParentFolderName { get; set; }
    public int TemplateCount { get; set; }
    public int SubfolderCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
