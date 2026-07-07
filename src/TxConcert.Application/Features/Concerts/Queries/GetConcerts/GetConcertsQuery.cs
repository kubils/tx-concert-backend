using TxConcert.Application.Common.Responses;
using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Cqrs;
using TxConcert.Domain.Common.Enums;
using NodaTime;

namespace TxConcert.Application.Features.Concerts.Queries.GetConcerts;

public sealed record GetConcertsQuery(
    int Limit = Constants.Pagination.DefaultLimit,
    int Offset = Constants.Pagination.DefaultOffset,
    string? Search = null,
    string? StateId = null,
    string? ArtistId = null,
    ConcertType? ConcertType = null,
    LocalDate? FromDate = null,
    LocalDate? ToDate = null) : IQuery<PaginatedResponse<ConcertListResponse>>;
