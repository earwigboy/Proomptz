using System.Text.Json.Serialization;

namespace PromptTemplateManager.Infrastructure.DevinIntegration;

/// <summary>
/// Request model for creating a Devin session
/// </summary>
public class DevinSessionRequest
{
    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    [JsonPropertyName("idempotent")]
    public bool Idempotent { get; set; } = true;
}

/// <summary>
/// Response model from Devin API session creation
/// </summary>
public class DevinSessionResponse
{
    [JsonPropertyName("session_id")]
    public required string SessionId { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("is_new_session")]
    public bool? IsNewSession { get; set; }
}

/// <summary>
/// Error response model from Devin API
/// </summary>
public class DevinErrorResponse
{
    [JsonPropertyName("detail")]
    public required string Detail { get; set; }
}
