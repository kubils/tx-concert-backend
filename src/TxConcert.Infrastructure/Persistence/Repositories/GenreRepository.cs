using TxConcert.Domain.Features.Genres;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class GenreRepository(ApplicationDbContext context)
    : BaseRepository<GenreEntity>(context), IGenreRepository
{
    public async Task<GenreEntity?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Slug == slug, ct);
    }

    public async Task<GenreEntity?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name == name, ct);
    }
}
