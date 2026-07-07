using TxConcert.Domain.Common;

namespace TxConcert.Api.Middleware;

/// <summary>
/// Ensures every request has a correlation ID for distributed tracing.
/// NestJS equivalent: CorrelationIdMiddleware from src/api/middleware/.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(Constants.Headers.CorrelationId))
        {
            context.Request.Headers.Append(Constants.Headers.CorrelationId, Guid.NewGuid().ToString());
        }

        string correlationId = context.Request.Headers[Constants.Headers.CorrelationId].ToString();
        context.Response.Headers.Append(Constants.Headers.CorrelationId, correlationId);

        using IDisposable? scope = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CorrelationIdMiddleware>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        await next(context);
    }
}
