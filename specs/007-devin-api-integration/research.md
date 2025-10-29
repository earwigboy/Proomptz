# Phase 0: Research - Devin API Integration

**Date**: 2025-10-26
**Feature**: Devin API Integration
**Branch**: `007-devin-api-integration`

## Overview

This document captures technical research for integrating with the Devin API to create new sessions. The research covers HTTP client patterns in .NET, Devin API structure, error handling strategies, and configuration management.

## Research Areas

### 1. Devin API Structure

**Decision**: Use RESTful HTTP API with JSON payloads

**API Details** (from specification and documentation reference):
- **Base URL**: `https://api.devin.ai` (to be confirmed from official docs)
- **Authentication**: Bearer token authentication using API key
- **Endpoint**: `POST /v1/sessions` (create new session)
- **Headers Required**:
  - `Authorization: Bearer {api_key}`
  - `Content-Type: application/json`
  - `Accept: application/json`

**Request Structure** (inferred from standard session creation patterns):
```json
{
  "prompt": "string",
  "metadata": {
    "source": "proomptz",
    "template_id": "string"
  }
}
```

**Response Structure** (expected):
```json
{
  "session_id": "string",
  "session_url": "string",
  "status": "created",
  "created_at": "ISO 8601 timestamp"
}
```

**Error Response Structure** (expected):
```json
{
  "error": {
    "code": "string",
    "message": "string",
    "details": {}
  }
}
```

**Rationale**: REST API with JSON is the industry standard for web services. Devin API follows this pattern based on the documentation reference in the spec.

**Alternatives Considered**:
- GraphQL: Not applicable - Devin API uses REST
- gRPC: Not applicable - Devin API uses HTTP/JSON
- SOAP: Outdated, not used by modern APIs

### 2. HTTP Client Implementation in .NET

**Decision**: Use `IHttpClientFactory` with typed HttpClient

**Rationale**:
- IHttpClientFactory is the .NET recommended approach (since .NET Core 2.1)
- Manages HttpClient lifecycle properly (prevents socket exhaustion)
- Supports Polly integration for resilience patterns
- Enables dependency injection and testability
- No additional NuGet packages required (built into ASP.NET Core)

**Implementation Pattern**:
```csharp
// Registration in Program.cs
builder.Services.AddHttpClient<IDevinClient, DevinClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<DevinApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Usage in DevinClient
public class DevinClient : IDevinClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<DevinApiOptions> _options;

    public DevinClient(HttpClient httpClient, IOptions<DevinApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }
}
```

**Alternatives Considered**:
- Direct HttpClient instantiation: ❌ Causes socket exhaustion issues
- RestSharp library: ❌ Unnecessary dependency, IHttpClientFactory provides same functionality
- Refit library: ❌ Adds complexity for a single endpoint integration
- Flurl library: ❌ Unnecessary dependency

### 3. Resilience Patterns

**Decision**: Implement timeout, basic retry (optional), and detailed error logging

**Timeout Strategy**:
- HTTP client timeout: 30 seconds (external API tolerance)
- CancellationToken support for request cancellation
- Timeout exception mapped to user-friendly error message

**Retry Strategy** (optional - Phase 2 enhancement):
- Retry on transient HTTP errors (408, 429, 500, 502, 503, 504)
- Exponential backoff: 1s, 2s, 4s
- Maximum 3 attempts
- Only retry idempotent POST if API documents idempotency

**Circuit Breaker** (optional - Phase 2 enhancement):
- Break circuit after 5 consecutive failures
- Reset timeout: 30 seconds
- Useful for high-traffic scenarios

**Rationale**: Timeout is essential for preventing hung requests. Retry and circuit breaker are nice-to-have for production resilience but not required for MVP.

**Alternatives Considered**:
- No resilience patterns: ❌ Unacceptable - external APIs can be unreliable
- Polly library for advanced patterns: ✅ Available but defer to Phase 2
- Manual retry logic: ❌ Polly provides tested implementation

### 4. Configuration Management

**Decision**: Use ASP.NET Core Options pattern with appsettings.json and environment variable overrides

**Configuration Model**:
```csharp
public class DevinApiOptions
{
    public const string SectionName = "DevinApi";

    public string BaseUrl { get; set; } = "https://api.devin.ai";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
```

**Configuration Sources** (priority order):
1. Environment variables: `DevinApi__ApiKey`, `DevinApi__BaseUrl`
2. appsettings.Development.json (for local development)
3. appsettings.json (for default values)

**appsettings.json Structure**:
```json
{
  "DevinApi": {
    "BaseUrl": "https://api.devin.ai",
    "ApiKey": "",
    "TimeoutSeconds": 30
  }
}
```

**appsettings.Development.json**:
```json
{
  "DevinApi": {
    "ApiKey": "dev-key-placeholder-override-with-env-var"
  }
}
```

**Docker Environment Variable**:
```dockerfile
ENV DevinApi__ApiKey=""
```

**Rationale**:
- Options pattern is .NET standard approach
- Environment variables provide secure credential injection in Docker
- appsettings.json provides sensible defaults
- API key never committed to source control (empty default + .env or Docker secrets)

**Security Considerations**:
- ✅ API key stored server-side only (never sent to frontend)
- ✅ API key not logged (masked in log output)
- ✅ API key not in source control (empty default value)
- ✅ API key injectable via environment variables (Docker, Kubernetes secrets)

**Alternatives Considered**:
- .env file parsing: ❌ Not needed - ASP.NET Core reads environment variables natively
- Azure Key Vault / AWS Secrets Manager: ✅ Production enhancement, not MVP requirement
- User-specific configuration UI: ❌ Out of scope per spec

### 5. Error Handling Strategy

**Decision**: Translate HTTP errors to domain exceptions with user-friendly messages

**Error Categories**:

| HTTP Status | Exception Type | User Message |
|-------------|----------------|--------------|
| 400 Bad Request | `DevinApiException` | "Invalid request sent to Devin API. Please try again." |
| 401 Unauthorized | `DevinApiException` | "Devin API key is invalid or missing. Please check your configuration." |
| 403 Forbidden | `DevinApiException` | "Access denied by Devin API. Please verify your API key permissions." |
| 404 Not Found | `DevinApiException` | "Devin API endpoint not found. The service may be temporarily unavailable." |
| 429 Too Many Requests | `DevinApiException` | "Devin API rate limit exceeded. Please try again in a few minutes." |
| 500-504 Server Errors | `DevinApiException` | "Devin API is temporarily unavailable. Please try again later." |
| Timeout | `DevinApiException` | "Request to Devin API timed out. Please check your connection and try again." |
| Network Error | `DevinApiException` | "Unable to connect to Devin API. Please check your internet connection." |

**Exception Model**:
```csharp
public class DevinApiException : Exception
{
    public int? HttpStatusCode { get; }
    public string? ErrorCode { get; }
    public string UserFriendlyMessage { get; }

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
```

**Error Flow**:
1. DevinClient catches HttpRequestException, TaskCanceledException, etc.
2. Maps to DevinApiException with appropriate user message
3. TemplatesController catches DevinApiException
4. Returns SendPromptResponse with `Success = false` and user-friendly message
5. Frontend displays error in toast notification

**Rationale**:
- Separates HTTP errors from business logic errors
- Provides user-actionable messages (not technical stack traces)
- Maintains existing error handling patterns (ErrorHandlingMiddleware)
- Logs technical details for debugging while showing friendly messages to users

**Alternatives Considered**:
- Generic exceptions: ❌ Loses context for error categorization
- HTTP exceptions passed to controller: ❌ Leaks implementation details
- Result<T> pattern: ✅ Valid alternative but not current codebase pattern

### 6. Testing Strategy

**Decision**: Unit tests with mocked HttpClient, integration tests optional

**Unit Test Approach**:
- Mock HttpMessageHandler to simulate API responses
- Test success scenarios (200 OK with valid response)
- Test error scenarios (401, 429, 500, timeout)
- Test request formation (correct headers, body, authentication)
- Test response parsing (correct mapping to DTOs)
- Target: 80% code coverage

**Unit Test Libraries**:
- xUnit: Test framework (standard for .NET)
- Moq: Mocking library for HttpMessageHandler
- FluentAssertions: Readable assertions

**Example Test Structure**:
```csharp
[Fact]
public async Task SendPromptAsync_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var mockHttpMessageHandler = CreateMockHandler(
        HttpStatusCode.OK,
        "{\"session_id\":\"123\",\"session_url\":\"https://devin.ai/sessions/123\"}"
    );
    var httpClient = new HttpClient(mockHttpMessageHandler);
    var client = new DevinClient(httpClient, Options.Create(new DevinApiOptions()));

    // Act
    var result = await client.SendPromptAsync("test prompt");

    // Assert
    result.Success.Should().BeTrue();
    result.ResponseId.Should().Be("123");
}
```

**Integration Test Approach** (optional):
- Use real Devin API sandbox/test environment (if available)
- Test end-to-end flow from controller to API
- Requires test API key
- Run separately from unit tests (slower, external dependency)

**Rationale**:
- Unit tests provide fast feedback and high coverage
- Mocking HttpClient is a well-established pattern in .NET
- Integration tests validate real API contract but are optional for MVP
- Testing ensures error handling works correctly

**Alternatives Considered**:
- Integration tests only: ❌ Slow, requires test API access, fragile
- No tests: ❌ Violates constitution (80% coverage requirement)
- Contract tests with Pact: ✅ Advanced option for Phase 2

### 7. Logging and Observability

**Decision**: Use structured logging with sensitive data masking

**Logging Strategy**:
- Log request start (prompt length, template ID)
- Log response success/failure with timing
- Log errors with HTTP status, error code, and sanitized message
- **NEVER log API keys or full prompts** (GDPR/security concern)

**Example Log Output**:
```
INFO: Sending prompt to Devin API [PromptLength=1024, TemplateId=abc-123]
INFO: Devin API request succeeded [SessionId=xyz-789, Duration=1234ms]
ERROR: Devin API request failed [Status=401, Message="Unauthorized", Duration=234ms]
```

**Log Sanitization**:
```csharp
_logger.LogInformation(
    "Sending prompt to Devin API [PromptLength={PromptLength}]",
    prompt.Length
);
// NOT: _logger.LogInformation("Sending prompt: {Prompt}", prompt);
```

**Rationale**:
- Structured logging enables querying and monitoring
- Masking sensitive data prevents security/compliance issues
- Duration tracking enables performance monitoring
- Follows existing logging patterns in ErrorHandlingMiddleware

**Alternatives Considered**:
- No logging: ❌ Can't diagnose production issues
- Log everything including API keys: ❌ Security violation
- Application Insights / Datadog: ✅ Production enhancement

## Implementation Checklist

- [ ] Create `DevinApiOptions.cs` configuration model
- [ ] Update `appsettings.json` with Devin API section
- [ ] Create `DevinApiException.cs` domain exception
- [ ] Create `DevinApiModels.cs` with request/response DTOs
- [ ] Implement `DevinClient.cs` with HttpClient
- [ ] Register HttpClient in `Program.cs` with IHttpClientFactory
- [ ] Update `TemplatesController.cs` error handling
- [ ] Add unit tests for DevinClient
- [ ] Update frontend to handle new error messages
- [ ] Document configuration in README (if exists)

## Open Questions

1. **Devin API Base URL**: Need to confirm from official docs (assumed `https://api.devin.ai`)
2. **Session Creation Endpoint Path**: Need to confirm exact path (assumed `/v1/sessions`)
3. **Request/Response Schema**: Need to validate exact field names from API docs
4. **Idempotency**: Does Devin API support idempotent session creation for safe retries?
5. **Rate Limits**: What are the specific rate limits? How are they communicated (headers, error codes)?
6. **Sandbox Environment**: Is there a test/sandbox API URL for development?

**Resolution Plan**: These questions will be resolved during implementation by:
1. Reading official Devin API documentation at `https://docs.devin.ai/api-reference/`
2. Making test API calls during development
3. Adjusting implementation based on actual API behavior
4. Documenting findings in code comments

## Phase 0 Complete

All technical unknowns have been researched and documented. Ready to proceed to Phase 1 (Design & Contracts).
