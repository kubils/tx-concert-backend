using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Genres;

public sealed class GenreEntity : BaseEntity
{
    private GenreEntity() { } // EF Core

    public static GenreEntity Create(string name, string slug)
    {
        var entity = new GenreEntity
        {
            Name = name,
            Slug = slug
        };
        entity.SetId(Constants.IdPrefix.Genre);
        return entity;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    public void UpdateName(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}
