using TxConcert.Application.Common.Responses;
using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Artists.Queries.GetArtists;

public sealed record GetArtistsQuery(
    int Limit = Constants.Pagination.DefaultLimit,
    int Offset = Constants.Pagination.DefaultOffset,
    string? Search = null,
    string? GenreId = null) : IQuery<PaginatedResponse<ArtistListResponse>>;
