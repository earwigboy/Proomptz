namespace PromptTemplateManager.Application.DTOs;

public class FolderTreeNode
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Depth { get; set; }
    public int TemplateCount { get; set; }
    public List<FolderTreeNode> ChildFolders { get; set; } = new();
}
