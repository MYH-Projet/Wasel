using System.Net;
using System.Text.Json;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Shared.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Don't log expected business errors as critical errors, maybe as info/warn
        if (exception is ApiException)
        {
            _logger.LogWarning(exception, "Business exception occurred.");
        }
        else
        {
            _logger.LogError(exception, "An unhandled exception has occurred.");
        }

        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        if (exception is ApiException apiException)
        {
            statusCode = apiException.StatusCode;
            message = apiException.Message;
        }

        response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = statusCode,
            Message = message
        });

        return response.WriteAsync(result);
    }
}
