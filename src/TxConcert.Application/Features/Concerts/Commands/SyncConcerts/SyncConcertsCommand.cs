using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Concerts.Commands.SyncConcerts;

public sealed record SyncConcertsCommand(int? MaxEvents = null) : ICommand<SyncConcertsResult>, ITransactional;

public sealed record SyncConcertsResult(
    int Created,
    int Updated,
    int Skipped,
    IReadOnlyList<string>? EvaluationConcertIds = null);
