using YamlDotNet.Serialization;

namespace PromptTemplateManager.Infrastructure.FileSystem;

/// <summary>
/// Represents template metadata stored in YAML frontmatter.
/// </summary>
public class TemplateMetadata
{
    [YamlMember(Alias = "id")]
    public Guid Id { get; set; }

    [YamlMember(Alias = "created")]
    public DateTime Created { get; set; }

    [YamlMember(Alias = "updated")]
    public DateTime Updated { get; set; }
}
