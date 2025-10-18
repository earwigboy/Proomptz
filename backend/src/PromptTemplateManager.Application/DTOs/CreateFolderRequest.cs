namespace PromptTemplateManager.Application.DTOs;

public class CreateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
}
