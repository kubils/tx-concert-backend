using MediatR;
using Microsoft.Extensions.Logging;

namespace TxConcert.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}", requestName);

        TResponse response;
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request {RequestName} failed", requestName);
            throw;
        }

        logger.LogInformation("Handled {RequestName}", requestName);
        return response;
    }
}
