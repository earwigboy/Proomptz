using System.Net;
using System.Text.Json;
using PromptTemplateManager.Core.Exceptions;

namespace PromptTemplateManager.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, error, message, details) = exception switch
        {
            NotFoundException e => (HttpStatusCode.NotFound, "NotFound", e.Message, Array.Empty<string>()),
            ConflictException e => (HttpStatusCode.Conflict, "Conflict", e.Message, Array.Empty<string>()),
            ValidationException e => (HttpStatusCode.BadRequest, "ValidationError", e.Message, e.Errors),
            _ => (HttpStatusCode.InternalServerError, "InternalError", "An unexpected error occurred", Array.Empty<string>())
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error,
            message,
            details
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
