using Fuse8.BackendInternship.PublicApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fuse8.BackendInternship.PublicApi.Filters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiRequestLimitException)
        {
            _logger.LogError(context.Exception, "Too Many Requests");
            context.Result = new ObjectResult("Too Many Requests") { StatusCode = StatusCodes.Status429TooManyRequests };
        }
        else if (context.Exception is CurrencyNotFoundException)
        {
            context.Result = new ObjectResult("Currency Not Found") { StatusCode = StatusCodes.Status404NotFound };
        }
        else
        {
            _logger.LogError(context.Exception, "Internal Server Error");
            context.Result = new ObjectResult("Internal Server Error") { StatusCode = StatusCodes.Status500InternalServerError };
        }

        context.ExceptionHandled = true;
    }
}