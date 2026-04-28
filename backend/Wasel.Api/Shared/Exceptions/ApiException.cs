namespace Wasel.Api.Shared.Exceptions;

/// <summary>
/// Custom exception for API errors with HTTP status code.
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }

    public static ApiException NotFound(string message = "Resource not found")
        => new(message, 404);

    public static ApiException BadRequest(string message = "Bad request")
        => new(message, 400);

    public static ApiException Unauthorized(string message = "Unauthorized")
        => new(message, 401);

    public static ApiException Forbidden(string message = "Forbidden")
        => new(message, 403);
}
