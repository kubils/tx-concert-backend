using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Prompts;

public sealed class AiPromptEntity : BaseEntity
{
    private AiPromptEntity() { } // EF Core

    public static AiPromptEntity Create(
        string key,
        string name,
        string systemPrompt,
        string userPromptInstructions,
        int versionNumber)
    {
        var entity = new AiPromptEntity
        {
            Key = key,
            Name = name,
            SystemPrompt = systemPrompt,
            UserPromptInstructions = userPromptInstructions,
            VersionNumber = versionNumber,
            IsActive = true
        };
        entity.SetId(Constants.IdPrefix.AiPrompt);
        return entity;
    }

    public string Key { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string SystemPrompt { get; private set; } = default!;
    public string UserPromptInstructions { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate() => IsActive = false;
}
