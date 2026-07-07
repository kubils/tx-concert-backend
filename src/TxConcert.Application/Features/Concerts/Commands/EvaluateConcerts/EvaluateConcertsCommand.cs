using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;

public sealed record EvaluateConcertsCommand(
    IReadOnlyList<string>? ConcertIds = null,
    int? BatchSize = null) : ICommand<EvaluateConcertsResult>;

public sealed record EvaluateConcertsResult(
    int Evaluated,
    int Skipped,
    int Failed,
    int ArticlesGenerated,
    int ArticlesFailed);
