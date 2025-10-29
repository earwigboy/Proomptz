namespace PromptTemplateManager.Core.Exceptions;

/// <summary>
/// Exception thrown when Devin API operations fail
/// </summary>
public class DevinApiException : Exception
{
    /// <summary>
    /// User-friendly error message suitable for display
    /// </summary>
    public string UserFriendlyMessage { get; }

    /// <summary>
    /// HTTP status code from the API response (if applicable)
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// Error code from Devin API (if applicable)
    /// </summary>
    public string? ErrorCode { get; }

    public DevinApiException(
        string message,
        string userFriendlyMessage,
        Exception? innerException = null,
        int? httpStatusCode = null,
        string? errorCode = null)
        : base(message, innerException)
    {
        UserFriendlyMessage = userFriendlyMessage;
        HttpStatusCode = httpStatusCode;
        ErrorCode = errorCode;
    }
}
