using System.Net;
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
        SetResponse("Произошла ошибка при обработке запроса", StatusCodes.Status500InternalServerError);

        context.ExceptionHandled = true;

        void SetResponse(string errorDescription, int httpStatusCode)
        {
            context.Result = new JsonResult(new ProblemDetails { Title = errorDescription, Status = httpStatusCode });

            context.HttpContext.Response.StatusCode = httpStatusCode;
        }
    }
}