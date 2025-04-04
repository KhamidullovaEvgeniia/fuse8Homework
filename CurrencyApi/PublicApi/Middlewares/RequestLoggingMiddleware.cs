using System.Text;

namespace Fuse8.BackendInternship.PublicApi.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        _logger.LogInformation("Method {Method}, Path {Path}", request.Method, request.Path);

        await _next(context);
    }
}