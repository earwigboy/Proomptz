using YamlDotNet.Serialization;

namespace PromptTemplateManager.Infrastructure.FileSystem;

/// <summary>
/// Represents folder metadata stored in .folder-meta file.
/// </summary>
public class FolderMetadata
{
    [YamlMember(Alias = "id")]
    public Guid Id { get; set; }

    [YamlMember(Alias = "created")]
    public DateTime Created { get; set; }

    [YamlMember(Alias = "updated")]
    public DateTime Updated { get; set; }
}
