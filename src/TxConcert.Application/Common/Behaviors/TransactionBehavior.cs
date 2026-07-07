using TxConcert.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TxConcert.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    IDbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ITransactional)
            return await next();

        string requestName = typeof(TRequest).Name;

        // Cast to DbContext to access Database.BeginTransactionAsync
        if (dbContext is not DbContext context)
            return await next();

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx =
            await context.Database.BeginTransactionAsync(ct);

        try
        {
            TResponse response = await next();
            await tx.CommitAsync(ct);
            logger.LogDebug("Transaction committed for {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            logger.LogWarning(ex, "Transaction rolled back for {RequestName}", requestName);
            throw;
        }
    }
}
