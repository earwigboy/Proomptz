namespace PromptTemplateManager.Application.DTOs;

public class UpdateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
}
