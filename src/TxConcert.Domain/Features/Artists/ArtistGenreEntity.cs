using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public sealed class ArtistGenreEntity : BaseEntity
{
    private ArtistGenreEntity() { } // EF Core

    public static ArtistGenreEntity Create(string artistId, string genreId, bool isPrimary = false)
    {
        var entity = new ArtistGenreEntity
        {
            ArtistId = artistId,
            GenreId = genreId,
            IsPrimary = isPrimary
        };
        entity.SetId(Constants.IdPrefix.ArtistGenre);
        return entity;
    }

    public string ArtistId { get; private set; } = default!;
    public string GenreId { get; private set; } = default!;
    public bool IsPrimary { get; private set; }

    public void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
