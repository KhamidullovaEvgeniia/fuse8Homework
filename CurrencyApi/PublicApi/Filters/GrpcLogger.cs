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

            // Создаем событие аудита с помощью Audit.Net
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

            // Устанавливаем дополнительные поля для аудита
            auditScope.SetCustomField("Response", new { Response = response?.ToString() });

            // Закрываем область аудита
            await auditScope.DisposeAsync();
            return await inner;
        }
        catch (Exception ex)
        {
            // Логируем ошибку с помощью ILogger
            _logger.LogError(ex, "Error occurred while processing gRPC request");

            // Создаем событие аудита для ошибки
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

            // Устанавливаем дополнительные поля для аудита ошибок
            errorAuditScope.SetCustomField(
                "Error",
                new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });

            // Закрываем область аудита для ошибки
            await errorAuditScope.DisposeAsync();

            throw new InvalidOperationException("Custom error", ex);
        }
    }
}