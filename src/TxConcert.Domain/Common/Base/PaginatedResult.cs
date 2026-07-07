namespace TxConcert.Domain.Common.Base;

/// <summary>Generic paginated result returned by repositories.</summary>
public sealed record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int Limit,
    int Offset
);
