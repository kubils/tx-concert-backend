using TxConcert.Application.Common.Exceptions;
using TxConcert.Domain.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace TxConcert.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        (int statusCode, object body) = exception switch
        {
            ValidationException validation => (422, new
            {
                statusCode = 422,
                message = validation.Message,
                errors = validation.Errors
            }),
            TooManyRequestsException tooMany => HandleTooManyRequests(httpContext, tooMany),
            DomainException domain => (domain.StatusCode, new
            {
                statusCode = domain.StatusCode,
                message = domain.Message,
                i18nKey = domain.I18nKey
            }),
            _ => (500, (object)new
            {
                statusCode = 500,
                message = "An unexpected error occurred."
            })
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception ({StatusCode}): {Message}", statusCode, exception.Message);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(body, ct);
        return true;
    }

    private static (int, object) HandleTooManyRequests(HttpContext ctx, TooManyRequestsException ex)
    {
        if (ex.RetryAfterSeconds > 0)
            ctx.Response.Headers.RetryAfter = ex.RetryAfterSeconds.ToString();

        return (429, new
        {
            statusCode = 429,
            message = ex.Message,
            retryAfterSeconds = ex.RetryAfterSeconds
        });
    }
}
