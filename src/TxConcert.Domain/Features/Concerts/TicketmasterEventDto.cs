using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

/// <summary>
/// Flat DTO representing a single event parsed from the Ticketmaster Discovery API.
/// </summary>
public sealed record TicketmasterEventDto
{
    public required string TicketmasterId { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public LocalDate EventDate { get; init; }
    public LocalTime? StartTime { get; init; }

    // Image
    public string? ImageUrl { get; init; }

    // Timezone
    public string? TimeZone { get; init; }

    // Venue
    public string? VenueName { get; init; }
    public string? VenueAddress { get; init; }
    public string? VenueCity { get; init; }
    public string? VenueStateCode { get; init; }
    public string? VenueZipCode { get; init; }
    public decimal? VenueLatitude { get; init; }
    public decimal? VenueLongitude { get; init; }

    // Price
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }

    // Seatmap
    public string? SeatmapUrl { get; init; }

    // Ticket limit
    public string? TicketLimitInfo { get; init; }

    // Artists / Attractions
    public IReadOnlyList<TicketmasterAttractionDto> Attractions { get; init; } = [];

    // Genre / SubGenre (event-level classification)
    public string? SegmentName { get; init; }
    public string? GenreName { get; init; }
    public string? SubGenreName { get; init; }

    // Event info
    public string? Info { get; init; }
    public string? PleaseNote { get; init; }

    // Status
    public string? SalesStatus { get; init; }
}

public sealed record TicketmasterAttractionDto
{
    public required string Name { get; init; }
    public string? ImageUrl { get; init; }

    // Per-attraction classification
    public string? GenreName { get; init; }
    public string? SubGenreName { get; init; }

    // External links
    public string? SpotifyUrl { get; init; }
    public string? InstagramUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? FacebookUrl { get; init; }
    public string? YoutubeUrl { get; init; }
    public string? WikiUrl { get; init; }
}
