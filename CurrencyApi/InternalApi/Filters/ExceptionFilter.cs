using InternalApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InternalApi.Filters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        switch (exception)
        {
            case ApiRequestLimitException apiRequestLimitException:
                _logger.LogError(context.Exception, "Too Many Requests");
                SetResponse(apiRequestLimitException.Message, StatusCodes.Status429TooManyRequests);
                break;
            case CurrencyNotFoundException currencyNotFoundException:
                SetResponse(currencyNotFoundException.Message, StatusCodes.Status404NotFound);
                break;
            default:
                SetResponse("Произошла ошибка при обработке запроса", StatusCodes.Status500InternalServerError);
                break;
        }

        context.ExceptionHandled = true;

        void SetResponse(string errorDescription, int httpStatusCode)
        {
            context.Result = new JsonResult(new ProblemDetails { Title = errorDescription, Status = httpStatusCode });

            context.HttpContext.Response.StatusCode = httpStatusCode;
        }
    }
}