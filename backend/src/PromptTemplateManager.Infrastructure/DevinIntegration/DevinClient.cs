using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Infrastructure.DevinIntegration;

/// <summary>
/// Stub implementation of Devin LLM client
/// Returns success immediately without actually calling an external service
/// </summary>
public class DevinClient : IDevinClient
{
    public Task<(bool Success, string Message, string? ResponseId)> SendPromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Stub implementation - always returns success
        var result = (
            Success: true,
            Message: "Prompt sent successfully (stub implementation)",
            ResponseId: (string?)Guid.NewGuid().ToString()
        );

        return Task.FromResult(result);
    }
}
