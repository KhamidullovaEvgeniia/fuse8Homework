using Audit.Core;
using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fuse8.BackendInternship.PublicApi.Filters;

public class GrpcLogger : Interceptor
{
    private readonly ILogger<GrpcLogger> _logger;

    public GrpcLogger(ILogger<GrpcLogger> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        _logger.LogInformation(
            """
            gRPC REQUEST
            Request: {Request}
            """,
            request.ToString());

        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner)
    {
        try
        {
            var response = await inner;
            _logger.LogInformation(
                """
                gRPC RESPONSE
                Response: {Response}
                """,
                response?.ToString());
            
            var auditScope = await AuditScope.CreateAsync(
                new AuditScopeOptions
                {
                    EventType = "gRPC Response",
                    TargetGetter = () => new
                    {
                        Response = response?.ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            
            auditScope.SetCustomField("Response", new { Response = response?.ToString() });
            
            await auditScope.DisposeAsync();
            return await inner;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing gRPC request");
            
            var errorAuditScope = await AuditScope.CreateAsync(
                new AuditScopeOptions
                {
                    EventType = "gRPC Error",
                    TargetGetter = () => new
                    {
                        Error = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            
            errorAuditScope.SetCustomField(
                "Error",
                new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            
            await errorAuditScope.DisposeAsync();

            throw new InvalidOperationException("Custom error", ex);
        }
    }
}