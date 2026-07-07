using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Concerts.Queries.GetConcertDetail;

public sealed record GetConcertDetailQuery(string Id) : IQuery<ConcertDetailResponse?>;
