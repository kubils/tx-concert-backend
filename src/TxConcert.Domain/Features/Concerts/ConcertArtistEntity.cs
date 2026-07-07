using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

public sealed class ConcertArtistEntity : BaseEntity
{
    private ConcertArtistEntity() { } // EF Core

    public static ConcertArtistEntity Create(
        string concertId,
        string artistId,
        bool isHeadliner = false,
        int billingOrder = 0,
        LocalTime? setTime = null)
    {
        var entity = new ConcertArtistEntity
        {
            ConcertId = concertId,
            ArtistId = artistId,
            IsHeadliner = isHeadliner,
            BillingOrder = billingOrder,
            SetTime = setTime
        };
        entity.SetId(Constants.IdPrefix.ConcertArtist);
        return entity;
    }

    public string ConcertId { get; private set; } = default!;
    public string ArtistId { get; private set; } = default!;
    public bool IsHeadliner { get; private set; }
    public int BillingOrder { get; private set; }
    public LocalTime? SetTime { get; private set; }

    public void SetHeadliner(bool isHeadliner) => IsHeadliner = isHeadliner;
    public void SetBillingOrder(int order) => BillingOrder = order;
    public void SetSetTime(LocalTime? time) => SetTime = time;
}
