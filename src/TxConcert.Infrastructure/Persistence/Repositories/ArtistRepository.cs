using TxConcert.Domain.Features.Artists;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ArtistRepository(ApplicationDbContext context)
    : BaseRepository<ArtistEntity>(context), IArtistRepository
{
    public async Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Slug == slug, ct);
    }

    public async Task<(IReadOnlyList<ArtistEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? genreId,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<ArtistEntity> query = DbSet.Where(e => e.IsActive);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{searchTerm}%"));

        if (!string.IsNullOrEmpty(genreId))
        {
            IQueryable<string> artistIdsWithGenre = Context.ArtistGenres
                .Where(ag => ag.GenreId == genreId)
                .Select(ag => ag.ArtistId);
            query = query.Where(e => artistIdsWithGenre.Contains(e.Id));
        }

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ArtistEntity> items = await query
            .OrderByDescending(e => e.PopularityScore)
            .ThenBy(e => e.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<ArtistEntity>> GetTopByPopularityAsync(
        int count,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.PopularityScore)
            .Take(count)
            .ToListAsync(ct);
    }
}
