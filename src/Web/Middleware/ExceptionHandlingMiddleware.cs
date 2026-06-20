using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FuelWallet.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await Write(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict");
            await Write(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (DuplicateKeyException ex)
        {
            _logger.LogWarning(ex, "Duplicate key");
            await Write(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            await Write(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain rule violation");
            await Write(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await Write(context, StatusCodes.Status400BadRequest, "Validation Failed",
                "One or more validation errors occurred.", ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await Write(context, StatusCodes.Status500InternalServerError, "Internal Server Error",
                "An unexpected error occurred.");
        }
    }

    private static async Task Write(
        HttpContext context,
        int statusCode,
        string error,
        string message,
        object? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = new ApiError(statusCode, error, message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private record ApiError(
        int StatusCode,
        string Error,
        string Message,
        object? Errors = null);
}
