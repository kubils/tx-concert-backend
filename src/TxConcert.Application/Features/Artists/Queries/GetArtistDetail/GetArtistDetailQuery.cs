using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Artists.Queries.GetArtistDetail;

public sealed record GetArtistDetailQuery(string Id) : IQuery<ArtistDetailResponse?>;
