namespace PromptTemplateManager.Application.DTOs;

public class SendPromptResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public string? DevinResponseId { get; set; }
}
