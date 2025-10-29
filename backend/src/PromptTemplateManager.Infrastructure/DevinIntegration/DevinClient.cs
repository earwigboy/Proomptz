using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptTemplateManager.Core.Exceptions;
using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Infrastructure.DevinIntegration;

/// <summary>
/// Production implementation of Devin API client with HTTP integration
/// </summary>
public class DevinClient : IDevinClient
{
    private readonly HttpClient _httpClient;
    private readonly DevinApiOptions _options;
    private readonly ILogger<DevinClient> _logger;

    public DevinClient(
        HttpClient httpClient,
        IOptions<DevinApiOptions> options,
        ILogger<DevinClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration on initialization
        ValidateConfiguration();
    }

    public async Task<(bool Success, string Message, string? ResponseId, string? SessionUrl)> SendPromptAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        try
        {
            _logger.LogInformation(
                "Sending prompt to Devin API [PromptLength={PromptLength}, BaseUrl={BaseUrl}]",
                prompt.Length,
                _options.BaseUrl);

            var startTime = DateTime.UtcNow;

            // Prepare request
            var request = new DevinSessionRequest
            {
                Prompt = prompt,
                Idempotent = true
            };

            // Set authorization header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Send request to Devin API
            var response = await _httpClient.PostAsJsonAsync("/v1/sessions", request, cancellationToken);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Handle success response
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DevinSessionResponse>(cancellationToken);

                if (result == null)
                {
                    _logger.LogError("Devin API returned null response body [Duration={Duration}ms]", duration);
                    throw new DevinApiException(
                        "Devin API returned empty response",
                        "Devin API returned an invalid response. Please try again.",
                        httpStatusCode: (int)response.StatusCode);
                }

                _logger.LogInformation(
                    "Devin API request succeeded [SessionId={SessionId}, Duration={Duration}ms]",
                    MaskSensitiveData(result.SessionId),
                    duration);

                return (true, "Template sent to Devin successfully", result.SessionId, result.Url);
            }

            // Handle error responses
            return await HandleErrorResponse(response, duration, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout (not user-initiated cancellation)
            _logger.LogWarning(ex, "Devin API request timed out [Timeout={Timeout}s]", _options.TimeoutSeconds);
            throw new DevinApiException(
                $"Request timed out after {_options.TimeoutSeconds} seconds",
                "Request to Devin API timed out. Please check your connection and try again.",
                ex,
                httpStatusCode: 408);
        }
        catch (HttpRequestException ex)
        {
            // Network errors
            _logger.LogError(ex, "Network error connecting to Devin API [BaseUrl={BaseUrl}]", _options.BaseUrl);
            throw new DevinApiException(
                "Network error connecting to Devin API",
                "Unable to connect to Devin API. Please check your internet connection.",
                ex);
        }
        catch (DevinApiException)
        {
            // Re-throw DevinApiException as-is
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected errors
            _logger.LogError(ex, "Unexpected error sending prompt to Devin API");
            throw new DevinApiException(
                "Unexpected error occurred",
                "An unexpected error occurred while sending to Devin. Please try again.",
                ex);
        }
    }

    private async Task<(bool Success, string Message, string? ResponseId, string? SessionUrl)> HandleErrorResponse(
        HttpResponseMessage response,
        double duration,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        string errorDetail;

        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<DevinErrorResponse>(cancellationToken);
            errorDetail = errorResponse?.Detail ?? "Unknown error";
        }
        catch
        {
            errorDetail = await response.Content.ReadAsStringAsync(cancellationToken);
        }

        _logger.LogWarning(
            "Devin API request failed [Status={StatusCode}, Detail={Detail}, Duration={Duration}ms]",
            statusCode,
            errorDetail,
            duration);

        var userFriendlyMessage = MapHttpStatusToUserMessage(response.StatusCode);

        throw new DevinApiException(
            $"HTTP {statusCode}: {errorDetail}",
            userFriendlyMessage,
            httpStatusCode: statusCode,
            errorCode: errorDetail);
    }

    private string MapHttpStatusToUserMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Invalid request sent to Devin API. Please try again.",

            HttpStatusCode.Unauthorized =>
                "Devin API key is invalid or missing. Please check your configuration.",

            HttpStatusCode.Forbidden =>
                "Access denied by Devin API. Please verify your API key permissions.",

            HttpStatusCode.NotFound =>
                "Devin API endpoint not found. The service may be temporarily unavailable.",

            HttpStatusCode.TooManyRequests =>
                "Devin API rate limit exceeded. Please try again in a few minutes.",

            HttpStatusCode.InternalServerError =>
                "Devin API is temporarily unavailable. Please try again later.",

            HttpStatusCode.BadGateway =>
                "Devin API is temporarily unavailable. Please try again later.",

            HttpStatusCode.ServiceUnavailable =>
                "Devin API is temporarily unavailable. Please try again later.",

            HttpStatusCode.GatewayTimeout =>
                "Devin API is temporarily unavailable. Please try again later.",

            _ =>
                $"Devin API returned an error (HTTP {(int)statusCode}). Please try again."
        };
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Devin API key is not configured");
        }

        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException($"Invalid BaseUrl in DevinApi configuration: {_options.BaseUrl}");
        }

        if (_options.TimeoutSeconds < 1 || _options.TimeoutSeconds > 300)
        {
            throw new InvalidOperationException($"TimeoutSeconds must be between 1 and 300, got: {_options.TimeoutSeconds}");
        }

        _logger.LogDebug(
            "Devin API configured [BaseUrl={BaseUrl}, Timeout={Timeout}s, ApiKeyConfigured={ApiKeyConfigured}]",
            _options.BaseUrl,
            _options.TimeoutSeconds,
            !string.IsNullOrWhiteSpace(_options.ApiKey));
    }

    private string MaskSensitiveData(string data)
    {
        if (string.IsNullOrWhiteSpace(data) || data.Length <= 8)
        {
            return "***";
        }

        return $"{data[..4]}***{data[^4..]}";
    }
}
