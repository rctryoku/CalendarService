using Calendar.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.ErrorHandling;

internal sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (ToStatusCode(exception) is not { } statusCode)
            return false;

        httpContext.Response.StatusCode = statusCode;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = statusCode, Detail = exception.Message },
        });
    }

    private static int? ToStatusCode(Exception exception) => exception switch
    {
        NotFoundException => StatusCodes.Status404NotFound,
        _ => null,
    };
}
