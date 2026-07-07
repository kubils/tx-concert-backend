using MediatR;
using TxConcert.Application.Common.Responses;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Concerts.Queries.GetConcerts;

public sealed class GetConcertsHandler(
    IConcertRepository concertRepository,
    IVenueRepository venueRepository,
    IConcertVibeRepository concertVibeRepository)
    : IRequestHandler<GetConcertsQuery, PaginatedResponse<ConcertListResponse>>
{
    public async Task<PaginatedResponse<ConcertListResponse>> Handle(
        GetConcertsQuery request,
        CancellationToken ct)
    {
        (IReadOnlyList<ConcertEntity> items, long total) = await concertRepository.SearchAsync(
            request.Search,
            request.StateId,
            request.ArtistId,
            request.ConcertType,
            request.FromDate,
            request.ToDate,
            request.Limit,
            request.Offset,
            ct);

        var responses = new List<ConcertListResponse>(items.Count);
        foreach (ConcertEntity concert in items)
        {
            VenueEntity? venue = await venueRepository.GetByIdAsync(concert.VenueId, ct);
            ConcertVibeEntity? vibe = await concertVibeRepository.GetByConcertIdAsync(concert.Id, ct);

            responses.Add(ConcertListResponse.From(
                concert,
                venue?.Name,
                venue?.City,
                vibe));
        }

        return PaginatedResponse<ConcertListResponse>.From(responses, total, request.Limit, request.Offset);
    }
}
