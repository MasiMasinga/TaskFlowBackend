using System.Security.Claims;
using TaskFlow.Extensions;

namespace TaskFlow.Middleware;

public sealed class AuthForbiddenLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthForbiddenLoggingMiddleware> _logger;

    public AuthForbiddenLoggingMiddleware(RequestDelegate next, ILogger<AuthForbiddenLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode != StatusCodes.Status403Forbidden)
            return;

        var user = context.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user.FindFirstValue(ClaimTypes.Name)
                     ?? user.FindFirstValue("sub");
        var email = user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue(ClaimTypes.Name);

        _logger.LogWarning(
            "Forbidden {Method} {Path} for {UserId} {Email} from {ClientIp}",
            context.Request.Method,
            context.Request.Path.Value,
            userId,
            email,
            context.GetClientIpAddress());
    }
}
