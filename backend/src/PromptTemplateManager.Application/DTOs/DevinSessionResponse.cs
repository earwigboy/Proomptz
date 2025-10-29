namespace PromptTemplateManager.Application.DTOs;

/// <summary>
/// Application DTO for Devin session response
/// </summary>
public class DevinSessionResponse
{
    public required string SessionId { get; set; }
    public required string Url { get; set; }
    public bool? IsNewSession { get; set; }
}
