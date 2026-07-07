using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ConcertRepository(ApplicationDbContext context, IClock clock)
    : BaseRepository<ConcertEntity>(context), IConcertRepository
{
    public async Task<ConcertEntity?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Slug == slug, ct);
    }

    public async Task<ConcertEntity?> GetByTicketmasterIdAsync(string ticketmasterId, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.TicketmasterId == ticketmasterId, ct);
    }

    public async Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetUpcomingAsync(
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        LocalDate today = clock.GetCurrentInstant().InUtc().Date;

        IQueryable<ConcertEntity> query = DbSet
            .Where(e => e.EventDate >= today
                && e.Status != ConcertStatus.Cancelled
                && e.Status != ConcertStatus.Completed);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ConcertEntity> items = await query
            .OrderBy(e => e.EventDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByStateAsync(
        string stateId,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<string> venueIdsInState = Context.Venues
            .Where(v => v.StateId == stateId)
            .Select(v => v.Id);

        IQueryable<ConcertEntity> query = DbSet
            .Where(e => venueIdsInState.Contains(e.VenueId));

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ConcertEntity> items = await query
            .OrderBy(e => e.EventDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByArtistAsync(
        string artistId,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<string> concertIdsForArtist = Context.ConcertArtists
            .Where(ca => ca.ArtistId == artistId)
            .Select(ca => ca.ConcertId);

        IQueryable<ConcertEntity> query = DbSet
            .Where(e => concertIdsForArtist.Contains(e.Id));

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ConcertEntity> items = await query
            .OrderBy(e => e.EventDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByDateRangeAsync(
        LocalDate from,
        LocalDate to,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<ConcertEntity> query = DbSet
            .Where(e => e.EventDate >= from && e.EventDate <= to);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ConcertEntity> items = await query
            .OrderBy(e => e.EventDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ConcertEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? stateId,
        string? artistId,
        ConcertType? concertType,
        LocalDate? fromDate,
        LocalDate? toDate,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<ConcertEntity> query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{searchTerm}%"));

        if (!string.IsNullOrEmpty(stateId))
        {
            IQueryable<string> venueIdsInState = Context.Venues
                .Where(v => v.StateId == stateId)
                .Select(v => v.Id);
            query = query.Where(e => venueIdsInState.Contains(e.VenueId));
        }

        if (!string.IsNullOrEmpty(artistId))
        {
            IQueryable<string> concertIdsForArtist = Context.ConcertArtists
                .Where(ca => ca.ArtistId == artistId)
                .Select(ca => ca.ConcertId);
            query = query.Where(e => concertIdsForArtist.Contains(e.Id));
        }

        if (concertType.HasValue)
            query = query.Where(e => e.ConcertType == concertType.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.EventDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.EventDate <= toDate.Value);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ConcertEntity> items = await query
            .OrderBy(e => e.EventDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<ConcertEntity>> GetFeaturedAsync(
        int count,
        CancellationToken ct = default)
    {
        LocalDate today = clock.GetCurrentInstant().InUtc().Date;

        return await DbSet
            .Where(e => e.IsFeatured
                && e.EventDate >= today
                && e.Status != ConcertStatus.Cancelled)
            .OrderBy(e => e.EventDate)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConcertEntity>> GetUnevaluatedAsync(
        int limit,
        CancellationToken ct = default)
    {
        LocalDate today = clock.GetCurrentInstant().InUtc().Date;

        // Concerts that have no active description yet
        IQueryable<string> evaluatedConcertIds = Context.ConcertDescriptions
            .Where(d => d.IsActive)
            .Select(d => d.ConcertId);

        return await DbSet
            .Where(e => e.EventDate >= today
                && e.Status != ConcertStatus.Cancelled
                && !evaluatedConcertIds.Contains(e.Id))
            .OrderBy(e => e.EventDate)
            .Take(limit)
            .ToListAsync(ct);
    }
}
