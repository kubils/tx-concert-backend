using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Concerts;

public sealed class ConcertVibeEntity : BaseEntity
{
    private ConcertVibeEntity() { } // EF Core

    public static ConcertVibeEntity Create(
        string concertId,
        int energy,
        int soundQuality,
        int hype,
        int crowdVibe,
        int valueForMoney,
        string? aiSummary = null,
        string? aiModel = null)
    {
        var entity = new ConcertVibeEntity
        {
            ConcertId = concertId,
            Energy = energy,
            SoundQuality = soundQuality,
            Hype = hype,
            CrowdVibe = crowdVibe,
            ValueForMoney = valueForMoney,
            AiSummary = aiSummary,
            AiModel = aiModel
        };
        entity.SetId(Constants.IdPrefix.ConcertVibe);
        return entity;
    }

    public string ConcertId { get; private set; } = default!;
    public int Energy { get; private set; }
    public int SoundQuality { get; private set; }
    public int Hype { get; private set; }
    public int CrowdVibe { get; private set; }
    public int ValueForMoney { get; private set; }
    public string? AiSummary { get; private set; }
    public string? AiModel { get; private set; }

    public void UpdateScores(
        int energy,
        int soundQuality,
        int hype,
        int crowdVibe,
        int valueForMoney,
        string? aiSummary = null,
        string? aiModel = null)
    {
        Energy = energy;
        SoundQuality = soundQuality;
        Hype = hype;
        CrowdVibe = crowdVibe;
        ValueForMoney = valueForMoney;
        AiSummary = aiSummary;
        AiModel = aiModel;
    }
}
