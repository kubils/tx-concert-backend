using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Genres;

public interface IGenreRepository : IRepository<GenreEntity>
{
    Task<GenreEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<GenreEntity?> GetByNameAsync(string name, CancellationToken ct = default);
}
