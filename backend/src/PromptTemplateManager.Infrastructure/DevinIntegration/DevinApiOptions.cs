namespace PromptTemplateManager.Infrastructure.DevinIntegration;

/// <summary>
/// Configuration options for Devin API integration
/// </summary>
public class DevinApiOptions
{
    public const string SectionName = "DevinApi";

    /// <summary>
    /// Base URL for the Devin API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.devin.ai";

    /// <summary>
    /// API key for authentication with Devin API
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
