using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

public sealed class ConcertDescriptionEntity : BaseEntity
{
    private ConcertDescriptionEntity() { } // EF Core

    public static ConcertDescriptionEntity Create(
        string concertId,
        string content,
        string aiModel,
        string promptUsed,
        int versionNumber,
        Instant generatedAt,
        int? tokensUsed = null,
        long? generationTimeMs = null)
    {
        var entity = new ConcertDescriptionEntity
        {
            ConcertId = concertId,
            Content = content,
            AiModel = aiModel,
            PromptUsed = promptUsed,
            VersionNumber = versionNumber,
            IsActive = false,
            GeneratedAt = generatedAt,
            TokensUsed = tokensUsed,
            GenerationTimeMs = generationTimeMs
        };
        entity.SetId(Constants.IdPrefix.ConcertDescription);
        return entity;
    }

    public string ConcertId { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public string AiModel { get; private set; } = default!;
    public string PromptUsed { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public bool IsActive { get; private set; }
    public Instant GeneratedAt { get; private set; }
    public int? TokensUsed { get; private set; }
    public long? GenerationTimeMs { get; private set; }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
