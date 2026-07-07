using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;
using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.Concerts.Commands.EvaluateConcertById;

public sealed record EvaluateConcertByIdCommand(string ConcertId) : ICommand<EvaluateConcertByIdResult>;

public sealed record EvaluateConcertByIdResult(
    string ConcertId,
    string ConcertName,
    string Description,
    ConcertVibeScoresDto ConcertVibes,
    IReadOnlyList<ArtistVibeDto> ArtistVibes,
    bool ArticleGenerated,
    string? ArticleTitle,
    string? ArticleError);

public sealed record ConcertVibeScoresDto(
    int Energy,
    int SoundQuality,
    int Hype,
    int CrowdVibe,
    int ValueForMoney,
    string? Summary);

public sealed record ArtistVibeDto(
    string ArtistName,
    int Visuals,
    int Sound,
    int Energy,
    int FanInteraction,
    int TexasSpirit,
    string? Summary);
