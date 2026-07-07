using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public sealed class ArtistVibeProfileEntity : BaseEntity
{
    private ArtistVibeProfileEntity() { } // EF Core

    public static ArtistVibeProfileEntity Create(
        string artistId,
        int visuals,
        int sound,
        int energy,
        int fanInteraction,
        int texasSpirit,
        string? aiSummary = null,
        string? aiModel = null)
    {
        var entity = new ArtistVibeProfileEntity
        {
            ArtistId = artistId,
            Visuals = visuals,
            Sound = sound,
            Energy = energy,
            FanInteraction = fanInteraction,
            TexasSpirit = texasSpirit,
            AiSummary = aiSummary,
            AiModel = aiModel
        };
        entity.SetId(Constants.IdPrefix.ArtistVibeProfile);
        return entity;
    }

    public string ArtistId { get; private set; } = default!;
    public int Visuals { get; private set; }
    public int Sound { get; private set; }
    public int Energy { get; private set; }
    public int FanInteraction { get; private set; }
    public int TexasSpirit { get; private set; }
    public string? AiSummary { get; private set; }
    public string? AiModel { get; private set; }

    public void UpdateScores(
        int visuals,
        int sound,
        int energy,
        int fanInteraction,
        int texasSpirit,
        string? aiSummary = null,
        string? aiModel = null)
    {
        Visuals = visuals;
        Sound = sound;
        Energy = energy;
        FanInteraction = fanInteraction;
        TexasSpirit = texasSpirit;
        AiSummary = aiSummary;
        AiModel = aiModel;
    }
}
