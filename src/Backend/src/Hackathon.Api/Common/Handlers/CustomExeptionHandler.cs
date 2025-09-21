using Microsoft.AspNetCore.Diagnostics;
using Hackathon.Api.Common.Models;
using Hackathon.Application.Exceptions;

namespace Hackathon.Api.Common.Handlers;

public class CustomExeptionHandler : IExceptionHandler
{
    public ILogger<CustomExeptionHandler> _logger;
    public CustomExeptionHandler(ILogger<CustomExeptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, $"Error handeled: {exception.Message}");

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR", validationException.Errors),
            ArgumentNullException => (StatusCodes.Status400BadRequest, "ARGUMENT_NULL_ERROR", null),
            ArgumentException => (StatusCodes.Status400BadRequest, "ARGUMENT_ERROR", null),
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", null),
            _ => (StatusCodes.Status500InternalServerError, "SERVER_ERROR", null)
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Title = title,
            Errors = errors,
            Message = exception.Message,
            StatusCode = statusCode,
            TraceId = httpContext.TraceIdentifier
        });

        return true;
    }
}