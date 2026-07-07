namespace TxConcert.Domain.Features.Concerts;

/// <summary>
/// Extra metadata synced from Ticketmaster, stored as a single jsonb column.
/// </summary>
public sealed record ConcertExternalData
{
    public string? ImageUrl { get; init; }
    public string? SeatmapUrl { get; init; }
    public string? TicketLimitInfo { get; init; }
    public string? SegmentName { get; init; }
    public string? SubGenreName { get; init; }
    public string? Info { get; init; }
    public string? PleaseNote { get; init; }
}
