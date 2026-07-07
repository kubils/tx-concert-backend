namespace TxConcert.Application.Common.Responses;

/// <summary>
/// A paginated response wrapper for list queries.
/// </summary>

public sealed record PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public long Total { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public bool HasNext => Offset + Items.Count < Total;

    public static PaginatedResponse<T> From(IReadOnlyList<T> items, long total, int limit, int offset) =>
        new() { Items = items, Total = total, Limit = limit, Offset = offset };
}
