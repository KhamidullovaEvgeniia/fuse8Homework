using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Framework.Middlewares;

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
        _logger.LogInformation("Get request for Method {Method}, Path {Path}", request.Method, request.Path);

        await _next(context);

        var response = context.Response;
        _logger.LogInformation(
            "Response for Method {Method}, Path {Path}, completed with status code {Code}",
            request.Method,
            request.Path,
            response.StatusCode);
    }
}