using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Exceptions;

namespace TaskFlow.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        var (status, title, detail) = exception switch
        {
            NotFoundException nf => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                nf.Message),

            ConflictException ce => (
                StatusCodes.Status409Conflict,
                "Conflict",
                ce.Message),

            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "You are not authorized to perform this action."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                _env.IsDevelopment() ? exception.Message : "Please try again later.")
        };

        // Log at Error for unexpected; Warning for expected client errors
        if (status >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception during {Method} {Path}. CorrelationId={CorrelationId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                correlationId);
        }
        else
        {
            _logger.LogWarning(
                "Handled {ExceptionType} during {Method} {Path}: {Message}. CorrelationId={CorrelationId}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message,
                correlationId);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (!string.IsNullOrEmpty(correlationId))
            problem.Extensions["traceId"] = correlationId;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }
}