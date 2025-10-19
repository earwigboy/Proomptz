namespace PromptTemplateManager.Core.Interfaces;

public interface IDevinClient
{
    /// <summary>
    /// Sends a prompt to Devin LLM
    /// Returns tuple of (success, message, responseId)
    /// </summary>
    Task<(bool Success, string Message, string? ResponseId)> SendPromptAsync(string prompt, CancellationToken cancellationToken = default);
}
