using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;

namespace TxConcert.Infrastructure.Services;

public sealed class TicketmasterService(
    HttpClient httpClient,
    IOptions<TicketmasterSettings> options,
    ILogger<TicketmasterService> logger) : ITicketmasterService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<IReadOnlyList<TicketmasterEventDto>> GetUpcomingEventsAsync(
        int? maxEvents = null,
        CancellationToken ct = default)
    {
        TicketmasterSettings settings = options.Value;
        var allEvents = new List<TicketmasterEventDto>();
        int page = 0;
        int totalPages = 1;
        //string startDt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        //string endDt = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        
        while (page < totalPages)
        {


            string url = $"{settings.BaseUrl}/events.json" +
                         $"?apikey={settings.ApiKey}" +
                         $"&stateCode={settings.StateCode}" +
                         $"&classificationName={settings.ClassificationName}" +
                         $"&size={settings.PageSize}" +
                         $"&page={page}" +
                         "&sort=date,asc";
                       // $"&startDateTime={startDt}" +
                       //  $"&endDateTime={endDt}" +

            logger.LogInformation("Fetching Ticketmaster events page {Page}/{TotalPages}", page, totalPages);

            HttpResponseMessage response = await httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(ct);
            TmRoot? root = JsonSerializer.Deserialize<TmRoot>(json, _jsonOptions);

            if (root?._embedded?.Events is null || root._embedded.Events.Count == 0)
                break;

            //totalPages = root.Page?.TotalPages ?? 1;  // Note: the API's pagination info is unreliable, so we keep fetching until we get an empty page

            foreach (TmEvent ev in root._embedded.Events)
            {
                TicketmasterEventDto? dto = MapEvent(ev);
                if (dto is not null)
                {
                    allEvents.Add(dto);
                }

                if (maxEvents is > 0 && allEvents.Count >= maxEvents.Value)
                {
                    logger.LogInformation("Reached Ticketmaster fetch limit of {MaxEvents}", maxEvents.Value);
                    return allEvents;
                }
            }

            page++;
        }

        logger.LogInformation("Fetched {Count} events from Ticketmaster", allEvents.Count);
        return allEvents;
    }

    private static TicketmasterEventDto? MapEvent(TmEvent ev)
    {
        if (ev.Id is null || ev.Name is null)
            return null;

        TmDate? dates = ev.Dates?.Start;
        LocalDate eventDate = default;
        LocalTime? startTime = null;

        if (dates?.LocalDate is not null &&
            _datePattern.Parse(dates.LocalDate) is { Success: true } dateResult)
        {
            eventDate = dateResult.Value;
        }
        else
        {
            return null; // skip events without a date
        }

        if (dates.LocalTime is not null &&
            _timePattern.Parse(dates.LocalTime) is { Success: true } timeResult)
        {
            startTime = timeResult.Value;
        }

        TmVenue? venue = ev._embedded?.Venues?.FirstOrDefault();

        decimal? priceMin = ev.PriceRanges?.MinBy(p => p.Min)?.Min;
        decimal? priceMax = ev.PriceRanges?.MaxBy(p => p.Max)?.Max;

        // Pick the best event-level image (largest 16:9, fallback to first available)
        string? imageUrl = PickBestImageUrl(ev.Images);

        var attractions = ev._embedded?.Attractions?
            .Where(a => a.Name is not null)
            .Select(a =>
            {
                TmClassification? ac = a.Classifications?.FirstOrDefault();
                string? aGenre = ac?.Genre?.Name;
                if (aGenre is "Undefined" or "Other") aGenre = null;
                string? aSubGenre = ac?.SubGenre?.Name;
                if (aSubGenre is "Undefined" or "Other") aSubGenre = null;

                return new TicketmasterAttractionDto
                {
                    Name = a.Name!,
                    ImageUrl = PickBestImageUrl(a.Images),
                    GenreName = aGenre,
                    SubGenreName = aSubGenre,
                    SpotifyUrl = a.ExternalLinks?.Spotify?.FirstOrDefault()?.Url,
                    InstagramUrl = a.ExternalLinks?.Instagram?.FirstOrDefault()?.Url,
                    WebsiteUrl = a.ExternalLinks?.Homepage?.FirstOrDefault()?.Url,
                    FacebookUrl = a.ExternalLinks?.Facebook?.FirstOrDefault()?.Url,
                    YoutubeUrl = a.ExternalLinks?.Youtube?.FirstOrDefault()?.Url,
                    WikiUrl = a.ExternalLinks?.Wiki?.FirstOrDefault()?.Url
                };
            })
            .ToList() ?? [];

        TmClassification? classification = ev.Classifications?.FirstOrDefault();

        string? genreName = classification?.Genre?.Name;
        if (genreName is "Undefined" or "Other")
            genreName = null;

        string? subGenreName = classification?.SubGenre?.Name;
        if (subGenreName is "Undefined" or "Other")
            subGenreName = null;

        string? segmentName = classification?.Segment?.Name;
        if (segmentName is "Undefined" or "Other")
            segmentName = null;

        return new TicketmasterEventDto
        {
            TicketmasterId = ev.Id,
            Name = ev.Name,
            Url = ev.Url ?? "",
            ImageUrl = imageUrl,
            EventDate = eventDate,
            StartTime = startTime,
            TimeZone = ev.Dates?.Timezone,
            VenueName = venue?.Name,
            VenueAddress = venue?.Address?.Line1,
            VenueCity = venue?.City?.Name,
            VenueStateCode = venue?.State?.StateCode,
            VenueZipCode = venue?.PostalCode,
            VenueLatitude = ParseDecimal(venue?.Location?.Latitude),
            VenueLongitude = ParseDecimal(venue?.Location?.Longitude),
            PriceMin = priceMin,
            PriceMax = priceMax,
            SeatmapUrl = ev.Seatmap?.StaticUrl,
            TicketLimitInfo = ev.TicketLimit?.Info,
            Attractions = attractions,
            SegmentName = segmentName,
            GenreName = genreName,
            SubGenreName = subGenreName,
            Info = ev.Info,
            PleaseNote = ev.PleaseNote,
            SalesStatus = ev.Dates?.Status?.Code
        };
    }

    /// <summary>
    /// Picks the largest non-fallback 16:9 image; falls back to fallback images, then widest of any ratio.
    /// </summary>
    private static string? PickBestImageUrl(List<TmImage>? images)
    {
        if (images is null || images.Count == 0)
            return null;

        // Prefer non-fallback images; use fallbacks only if nothing else is available
        List<TmImage> preferred = images.Where(i => !i.Fallback).ToList();
        List<TmImage> pool = preferred.Count > 0 ? preferred : images;

        TmImage? best = pool
            .Where(i => i.Ratio == "16_9")
            .MaxBy(i => i.Width);

        return (best ?? pool.MaxBy(i => i.Width))?.Url;
    }

    private static decimal? ParseDecimal(string? value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result) ? result : null;

    private static readonly NodaTime.Text.LocalDatePattern _datePattern =
        NodaTime.Text.LocalDatePattern.Iso;

    private static readonly NodaTime.Text.LocalTimePattern _timePattern =
        NodaTime.Text.LocalTimePattern.ExtendedIso;
}
