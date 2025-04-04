using System.Net.Http.Headers;
using Audit.Core;

namespace Fuse8.BackendInternship.PublicApi.Filters;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestHeaders = FormatHeaders(request.Headers);
        var requestContentHeaders = request.Content?.Headers != null ? FormatHeaders(request.Content.Headers) : "None";
        var requestBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : "None";

        _logger.LogInformation(
            """
            HTTP REQUEST
            Headers:
            {RequestHeaders}

            Content Headers:
            {RequestContentHeaders}

            Body:
            {RequestBody}
            """,
            requestHeaders,
            requestContentHeaders,
            requestBody);

        var auditScope = await AuditScope.CreateAsync(
            new AuditScopeOptions
            {
                EventType = "HttpRequest",
                TargetGetter = () => new
                {
                    RequestHeaders = requestHeaders,
                    RequestContentHeaders = requestContentHeaders,
                    RequestBody = requestBody
                }
            },
            cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        var responseHeaders = FormatHeaders(response.Headers);
        var responseContentHeaders = response.Content?.Headers != null ? FormatHeaders(response.Content.Headers) : "None";
        var responseBody = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken) : "None";

        _logger.LogInformation(
            """
            HTTP RESPONSE
            Headers:
            {ResponseHeaders}

            Content Headers:
            {ResponseContentHeaders}

            Body:
            {ResponseBody}
            """,
            responseHeaders,
            responseContentHeaders,
            responseBody);

        auditScope.SetCustomField(
            "Response",
            new
            {
                ResponseHeaders = responseHeaders,
                ResponseContentHeaders = responseContentHeaders,
                ResponseBody = responseBody
            });

        await auditScope.DisposeAsync();

        return response;
    }

    private string FormatHeaders(HttpHeaders? headers)
    {
        return headers != null && headers.Any()
            ? string.Join("\n", headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))
            : "None";
    }
}