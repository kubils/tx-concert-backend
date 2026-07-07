namespace TxConcert.Domain.Features.Concerts;

public interface ITicketmasterService
{
    Task<IReadOnlyList<TicketmasterEventDto>> GetUpcomingEventsAsync(
        int? maxEvents = null,
        CancellationToken ct = default);
}
