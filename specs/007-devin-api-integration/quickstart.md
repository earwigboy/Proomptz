# Phase 1: Quickstart - Devin API Integration

**Date**: 2025-10-26
**Feature**: Devin API Integration
**Branch**: `007-devin-api-integration`

## Overview

This document provides quick-start integration scenarios for developers implementing and testing the Devin API integration feature. It covers local development setup, testing workflows, and common integration patterns.

---

## Prerequisites

Before starting, ensure you have:

1. ✅ .NET 9.0 SDK installed
2. ✅ Node.js 20+ installed (for frontend)
3. ✅ Devin API key obtained from [Devin AI platform](https://devin.ai)
4. ✅ Repository cloned and dependencies installed
5. ✅ Basic familiarity with the Proomptz application

---

## Scenario 1: Local Development Setup

### Goal
Configure the Devin API integration for local development and verify the connection.

### Steps

#### 1.1 Configure API Key

**Option A: Environment Variable (Recommended)**

```bash
# Linux/macOS
export DevinApi__ApiKey="your-devin-api-key-here"

# Windows PowerShell
$env:DevinApi__ApiKey="your-devin-api-key-here"

# Windows CMD
set DevinApi__ApiKey=your-devin-api-key-here
```

**Option B: appsettings.Development.json**

```bash
cd backend/src/PromptTemplateManager.Api
```

Edit `appsettings.Development.json`:

```json
{
  "DevinApi": {
    "ApiKey": "your-devin-api-key-here"
  }
}
```

⚠️ **Security Warning**: Never commit API keys to source control. Add `appsettings.Development.json` to `.gitignore` if it contains secrets.

#### 1.2 Verify Configuration

**Backend configuration check**:

```bash
cd backend/src/PromptTemplateManager.Api
dotnet run
```

Check the logs for:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5026
```

#### 1.3 Test API Endpoint

**Create a test template** (if not already exists):

```bash
curl -X POST http://localhost:5026/api/templates \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Template",
    "description": "For testing Devin integration",
    "content": "Create a {{technology}} application that does {{task}}.",
    "folderId": null
  }'
```

**Send template to Devin**:

```bash
curl -X POST http://localhost:5026/api/templates/{template-id}/send \
  -H "Content-Type: application/json" \
  -d '{
    "placeholderValues": {
      "technology": "React",
      "task": "user authentication"
    }
  }'
```

**Expected Response** (success):

```json
{
  "success": true,
  "message": "Template sent to Devin successfully",
  "devinResponseId": "devin_session_abc123",
  "sessionUrl": "https://devin.ai/sessions/devin_session_abc123"
}
```

**Expected Response** (invalid API key):

```json
{
  "success": false,
  "message": "Devin API key is invalid or missing. Please check your configuration.",
  "devinResponseId": null,
  "sessionUrl": null
}
```

#### 1.4 Verify Session URL

Open the `sessionUrl` in a browser to verify the Devin session was created correctly.

---

## Scenario 2: Frontend Integration Testing

### Goal
Test the complete user flow from frontend button click to Devin session link display.

### Steps

#### 2.1 Start Development Servers

**Terminal 1 - Backend**:
```bash
cd backend/src/PromptTemplateManager.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

**Terminal 2 - Frontend**:
```bash
cd frontend
npm run dev
```

#### 2.2 Access Application

Open browser to `http://localhost:5173`

#### 2.3 Test User Flow

1. **Create or select a template**:
   - Navigate to Templates page
   - Create a new template or select existing one
   - Ensure template has placeholders (e.g., `{{project_name}}`)

2. **Fill placeholder values**:
   - Click "Generate" or "Use Template"
   - Fill in placeholder values in the form

3. **Send to Devin**:
   - Click "Send to Devin" button
   - Observe loading state on button
   - Wait for response (should be 1-5 seconds)

4. **Verify Success**:
   - Toast notification appears with success message
   - Clickable link to Devin session is displayed
   - Click link to open Devin session in new tab

5. **Test Error Handling**:
   - Stop backend server
   - Click "Send to Devin" button
   - Verify error toast appears with network error message
   - Restart backend server

---

## Scenario 3: Docker Deployment Testing

### Goal
Verify the Devin integration works in the containerized environment.

### Steps

#### 3.1 Build Docker Image

```bash
docker build -t proomptz:devin-integration .
```

#### 3.2 Run Container with API Key

```bash
docker run -d \
  --name proomptz-test \
  -p 5026:5026 \
  -e DevinApi__ApiKey="your-devin-api-key-here" \
  -e PORT=5026 \
  -v $(pwd)/data:/app/data \
  proomptz:devin-integration
```

#### 3.3 Test API

```bash
# Health check
curl http://localhost:5026/health

# Send template to Devin (replace {template-id})
curl -X POST http://localhost:5026/api/templates/{template-id}/send \
  -H "Content-Type: application/json" \
  -d '{
    "placeholderValues": {
      "key": "value"
    }
  }'
```

#### 3.4 View Logs

```bash
docker logs proomptz-test
```

Look for:
```
info: Sending prompt to Devin API [PromptLength=256]
info: Devin API request succeeded [SessionId=xyz, Duration=1234ms]
```

#### 3.5 Cleanup

```bash
docker stop proomptz-test
docker rm proomptz-test
```

---

## Scenario 4: Error Handling Testing

### Goal
Verify all error scenarios are handled gracefully with user-friendly messages.

### Steps

#### 4.1 Test Invalid API Key

**Update configuration** with invalid key:

```bash
export DevinApi__ApiKey="invalid-key-12345"
```

**Restart backend** and send request.

**Expected**:
- Status 200 (not 500)
- `success: false`
- User-friendly message about API key

#### 4.2 Test Missing API Key

**Remove API key** from configuration:

```bash
unset DevinApi__ApiKey
```

**Restart backend** and send request.

**Expected**:
- User-friendly message about missing configuration

#### 4.3 Test Network Timeout

**Mock slow network** by setting very short timeout:

Edit `appsettings.Development.json`:

```json
{
  "DevinApi": {
    "TimeoutSeconds": 1
  }
}
```

**Expected**:
- Timeout error after 1 second
- User-friendly message about connection timeout

#### 4.4 Test Rate Limit

**Make multiple rapid requests**:

```bash
for i in {1..10}; do
  curl -X POST http://localhost:5026/api/templates/{template-id}/send \
    -H "Content-Type: application/json" \
    -d '{"placeholderValues": {"key": "value"}}'
done
```

**If rate limited**:
- User-friendly message about rate limit
- Suggestion to try again later

---

## Scenario 5: Unit Testing the DevinClient

### Goal
Run unit tests for the DevinClient implementation.

### Steps

#### 5.1 Run Unit Tests

```bash
cd backend/tests/PromptTemplateManager.Tests.Unit
dotnet test
```

#### 5.2 Verify Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

**Expected**: 80%+ coverage for DevinClient

#### 5.3 Run Specific Test Category

```bash
# Run only DevinClient tests
dotnet test --filter "FullyQualifiedName~DevinClientTests"
```

---

## Scenario 6: Integration Testing with Test API Key

### Goal
Run integration tests against the real Devin API (if test environment available).

### Steps

#### 6.1 Configure Test API Key

```bash
export DevinApi__ApiKey="test-api-key-here"
export DevinApi__BaseUrl="https://api.devin.ai"  # or test URL
```

#### 6.2 Run Integration Tests

```bash
cd backend/tests/PromptTemplateManager.Tests.Integration
dotnet test
```

#### 6.3 Verify Real Session Creation

```bash
# Integration tests should output session URLs
# Manually verify a few session URLs open correctly
```

---

## Common Integration Patterns

### Pattern 1: HttpClient with IHttpClientFactory

```csharp
// Program.cs registration
builder.Services.AddHttpClient<IDevinClient, DevinClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DevinApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// DevinClient usage
public class DevinClient : IDevinClient
{
    private readonly HttpClient _httpClient;
    private readonly DevinApiOptions _options;

    public DevinClient(HttpClient httpClient, IOptions<DevinApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<(bool Success, string Message, string? ResponseId)> SendPromptAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

### Pattern 2: Options Configuration

```csharp
// Program.cs
builder.Services.Configure<DevinApiOptions>(
    builder.Configuration.GetSection(DevinApiOptions.SectionName)
);

// Usage in services
public SomeService(IOptions<DevinApiOptions> options)
{
    var apiKey = options.Value.ApiKey;
}
```

### Pattern 3: Error Handling

```csharp
try
{
    var response = await _httpClient.PostAsJsonAsync("/v1/sessions", request, ct);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<DevinSessionResponse>(ct);
    return (true, "Success", result.SessionId);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    throw new DevinApiException(
        "Unauthorized",
        "Devin API key is invalid or missing. Please check your configuration.",
        ex,
        401,
        "invalid_api_key"
    );
}
catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
{
    throw new DevinApiException(
        "Request timeout",
        "Request to Devin API timed out. Please check your connection and try again.",
        httpStatusCode: 408
    );
}
```

### Pattern 4: Logging with Sensitive Data Masking

```csharp
// Good - logs length only
_logger.LogInformation(
    "Sending prompt to Devin API [PromptLength={Length}]",
    prompt.Length
);

// Bad - logs sensitive data
// _logger.LogInformation("Sending prompt: {Prompt}", prompt);

// Good - masks API key
_logger.LogDebug(
    "Devin API configured [BaseUrl={BaseUrl}, ApiKeyLength={KeyLength}]",
    _options.BaseUrl,
    _options.ApiKey?.Length ?? 0
);

// Bad - logs API key
// _logger.LogDebug("Devin API key: {ApiKey}", _options.ApiKey);
```

---

## Troubleshooting Guide

### Problem: "Devin API key is invalid"

**Possible Causes**:
- API key not configured
- API key incorrect or revoked
- Environment variable not loaded

**Solutions**:
1. Verify API key in Devin AI dashboard
2. Check environment variable: `echo $DevinApi__ApiKey`
3. Restart backend after setting environment variable
4. Check appsettings.Development.json (if using file configuration)

---

### Problem: "Request to Devin API timed out"

**Possible Causes**:
- Network connectivity issues
- Devin API is slow or down
- Timeout setting too low

**Solutions**:
1. Check internet connection
2. Try request from command line: `curl https://api.devin.ai`
3. Increase timeout in appsettings.json
4. Check Devin API status page

---

### Problem: Frontend shows "Network Error"

**Possible Causes**:
- Backend not running
- CORS misconfiguration
- Wrong backend URL in frontend

**Solutions**:
1. Verify backend is running: `curl http://localhost:5026/health`
2. Check frontend API base URL configuration
3. Check browser console for CORS errors
4. Verify CORS policy in Program.cs includes frontend origin

---

### Problem: "Rate limit exceeded"

**Possible Causes**:
- Too many requests in short period
- Devin API rate limit reached

**Solutions**:
1. Wait before retrying (check Retry-After header)
2. Implement exponential backoff
3. Consider request queuing for high-volume scenarios
4. Contact Devin support for rate limit increase

---

## Performance Benchmarks

Expected performance metrics for integration:

| Metric | Target | Notes |
|--------|--------|-------|
| API request latency (p50) | 1-3s | Depends on Devin API performance |
| API request latency (p95) | 3-5s | Network and external API dependent |
| API request timeout | 30s | Configurable, prevents hung requests |
| Frontend loading state | <100ms | Button shows loading immediately |
| Error display latency | <500ms | Toast notification appears quickly |

---

## Security Checklist

Before deploying to production, verify:

- [ ] API key not committed to source control
- [ ] API key not logged in application logs
- [ ] API key not sent to frontend/browser
- [ ] API key injectable via environment variables
- [ ] HTTPS used for all Devin API requests
- [ ] Request/response payloads don't contain PII (unless necessary)
- [ ] Error messages don't leak sensitive information
- [ ] Rate limiting implemented or planned
- [ ] API key rotation procedure documented

---

## Next Steps

After completing quickstart scenarios:

1. **Implement the feature** using `/speckit.implement`
2. **Run tests** to verify 80% coverage
3. **Test all error scenarios** manually
4. **Verify Docker deployment** works correctly
5. **Document any API deviations** from expected contract
6. **Update README** with configuration instructions
7. **Create PR** for code review

---

## Phase 1 Quickstart Complete

All integration scenarios and patterns have been documented. Ready to proceed to agent context update and implementation.
