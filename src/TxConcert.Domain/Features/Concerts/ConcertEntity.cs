using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Enums;
using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

public sealed class ConcertEntity : BaseEntity
{
    private ConcertEntity() { } // EF Core

    public static ConcertEntity Create(
        string name,
        string slug,
        string venueId,
        LocalDate eventDate,
        ConcertType concertType,
        string timeZone = "America/Chicago",
        LocalTime? doorsOpen = null,
        LocalTime? startTime = null)
    {
        var entity = new ConcertEntity
        {
            Name = name,
            Slug = slug,
            VenueId = venueId,
            EventDate = eventDate,
            DoorsOpen = doorsOpen,
            StartTime = startTime,
            TimeZone = timeZone,
            ConcertType = concertType,
            Status = ConcertStatus.Scheduled,
            IsSoldOut = false,
            IsFeatured = false
        };
        entity.SetId(Constants.IdPrefix.Concert);
        return entity;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string VenueId { get; private set; } = default!;
    public LocalDate EventDate { get; private set; }
    public LocalTime? DoorsOpen { get; private set; }
    public LocalTime? StartTime { get; private set; }
    public string TimeZone { get; private set; } = "America/Chicago";
    public ConcertType ConcertType { get; private set; }
    public ConcertStatus Status { get; private set; }
    public string? TicketUrl { get; private set; }
    public decimal? PriceMin { get; private set; }
    public decimal? PriceMax { get; private set; }
    public bool IsSoldOut { get; private set; }
    public bool IsFeatured { get; private set; }
    public string? RescheduledFromId { get; private set; }
    public string? TicketmasterId { get; private set; }
    public ConcertExternalData? ExternalData { get; private set; }
    public string? DataHash { get; private set; }

    public void SetTicketInfo(string? ticketUrl, decimal? priceMin, decimal? priceMax)
    {
        TicketUrl = ticketUrl;
        PriceMin = priceMin;
        PriceMax = priceMax;
    }

    public void MarkSoldOut() => IsSoldOut = true;
    public void ClearSoldOut() => IsSoldOut = false;
    public void SetFeatured(bool featured) => IsFeatured = featured;

    public void UpdateStatus(ConcertStatus status) => Status = status;

    public void Reschedule(LocalDate newDate, LocalTime? newStartTime = null)
    {
        EventDate = newDate;
        StartTime = newStartTime ?? StartTime;
        Status = ConcertStatus.Rescheduled;
    }

    public void SetRescheduledFrom(string originalConcertId) =>
        RescheduledFromId = originalConcertId;

    public void SetTicketmasterId(string? ticketmasterId) =>
        TicketmasterId = ticketmasterId;

    public void SetExternalData(ConcertExternalData externalData) =>
        ExternalData = externalData;

    /// <summary>
    /// Updates the data hash and returns true if the data has changed.
    /// </summary>
    public bool UpdateDataHash(string newHash)
    {
        if (string.Equals(DataHash, newHash, StringComparison.Ordinal))
            return false;

        DataHash = newHash;
        return true;
    }

    public void UpdateSchedule(
        LocalDate eventDate,
        LocalTime? doorsOpen,
        LocalTime? startTime,
        string timeZone)
    {
        EventDate = eventDate;
        DoorsOpen = doorsOpen;
        StartTime = startTime;
        TimeZone = timeZone;
    }
}
