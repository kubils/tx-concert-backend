using System.Text.Json.Serialization;

namespace TxConcert.Infrastructure.Services;

internal sealed class TmRoot
{
    public TmEmbedded? _embedded { get; set; }

    [JsonPropertyName("page")]
    public TmPage? Page { get; set; }
}

internal sealed class TmEmbedded
{
    [JsonPropertyName("events")]
    public List<TmEvent>? Events { get; set; }
}

internal sealed class TmEvent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("images")]
    public List<TmImage>? Images { get; set; }

    [JsonPropertyName("dates")]
    public TmDates? Dates { get; set; }

    [JsonPropertyName("priceRanges")]
    public List<TmPriceRange>? PriceRanges { get; set; }

    [JsonPropertyName("seatmap")]
    public TmSeatmap? Seatmap { get; set; }

    [JsonPropertyName("ticketLimit")]
    public TmTicketLimit? TicketLimit { get; set; }

    [JsonPropertyName("_embedded")]
    public TmEventEmbedded? _embedded { get; set; }

    [JsonPropertyName("classifications")]
    public List<TmClassification>? Classifications { get; set; }

    [JsonPropertyName("info")]
    public string? Info { get; set; }

    [JsonPropertyName("pleaseNote")]
    public string? PleaseNote { get; set; }
}

internal sealed class TmEventEmbedded
{
    [JsonPropertyName("venues")]
    public List<TmVenue>? Venues { get; set; }

    [JsonPropertyName("attractions")]
    public List<TmAttraction>? Attractions { get; set; }
}

internal sealed class TmDates
{
    [JsonPropertyName("start")]
    public TmDate? Start { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("status")]
    public TmStatus? Status { get; set; }
}

internal sealed class TmDate
{
    [JsonPropertyName("localDate")]
    public string? LocalDate { get; set; }

    [JsonPropertyName("localTime")]
    public string? LocalTime { get; set; }
}

internal sealed class TmStatus
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

internal sealed class TmVenue
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("address")]
    public TmAddress? Address { get; set; }

    [JsonPropertyName("city")]
    public TmCity? City { get; set; }

    [JsonPropertyName("state")]
    public TmState? State { get; set; }

    [JsonPropertyName("location")]
    public TmLocation? Location { get; set; }
}

internal sealed class TmAddress
{
    [JsonPropertyName("line1")]
    public string? Line1 { get; set; }
}

internal sealed class TmCity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class TmState
{
    [JsonPropertyName("stateCode")]
    public string? StateCode { get; set; }
}

internal sealed class TmLocation
{
    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }
}

internal sealed class TmPriceRange
{
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    [JsonPropertyName("max")]
    public decimal Max { get; set; }
}

internal sealed class TmAttraction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("images")]
    public List<TmImage>? Images { get; set; }

    [JsonPropertyName("externalLinks")]
    public TmExternalLinks? ExternalLinks { get; set; }

    [JsonPropertyName("classifications")]
    public List<TmClassification>? Classifications { get; set; }
}

internal sealed class TmExternalLinks
{
    [JsonPropertyName("spotify")]
    public List<TmExternalLink>? Spotify { get; set; }

    [JsonPropertyName("instagram")]
    public List<TmExternalLink>? Instagram { get; set; }

    [JsonPropertyName("homepage")]
    public List<TmExternalLink>? Homepage { get; set; }

    [JsonPropertyName("facebook")]
    public List<TmExternalLink>? Facebook { get; set; }

    [JsonPropertyName("youtube")]
    public List<TmExternalLink>? Youtube { get; set; }

    [JsonPropertyName("wiki")]
    public List<TmExternalLink>? Wiki { get; set; }
}

internal sealed class TmExternalLink
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal sealed class TmImage
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("ratio")]
    public string? Ratio { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("fallback")]
    public bool Fallback { get; set; }
}

internal sealed class TmSeatmap
{
    [JsonPropertyName("staticUrl")]
    public string? StaticUrl { get; set; }
}

internal sealed class TmTicketLimit
{
    [JsonPropertyName("info")]
    public string? Info { get; set; }
}

internal sealed class TmClassification
{
    [JsonPropertyName("segment")]
    public TmGenre? Segment { get; set; }

    [JsonPropertyName("genre")]
    public TmGenre? Genre { get; set; }

    [JsonPropertyName("subGenre")]
    public TmGenre? SubGenre { get; set; }
}

internal sealed class TmGenre
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class TmPage
{
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }
}
