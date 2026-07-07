using MediatR;
using TxConcert.Application.Common.Responses;
using TxConcert.Domain.Features.Artists;

namespace TxConcert.Application.Features.Artists.Queries.GetArtists;

public sealed class GetArtistsHandler(
    IArtistRepository artistRepository,
    IArtistVibeProfileRepository artistVibeProfileRepository)
    : IRequestHandler<GetArtistsQuery, PaginatedResponse<ArtistListResponse>>
{
    public async Task<PaginatedResponse<ArtistListResponse>> Handle(
        GetArtistsQuery request,
        CancellationToken ct)
    {
        (IReadOnlyList<ArtistEntity> items, long total) = await artistRepository.SearchAsync(
            request.Search, request.GenreId, request.Limit, request.Offset, ct);

        var responses = new List<ArtistListResponse>(items.Count);
        foreach (ArtistEntity artist in items)
        {
            ArtistVibeProfileEntity? vibe =
                await artistVibeProfileRepository.GetByArtistIdAsync(artist.Id, ct);
            responses.Add(ArtistListResponse.From(artist, vibe));
        }

        return PaginatedResponse<ArtistListResponse>.From(responses, total, request.Limit, request.Offset);
    }
}
