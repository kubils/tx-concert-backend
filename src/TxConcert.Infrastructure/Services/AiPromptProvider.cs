using Microsoft.EntityFrameworkCore;
using TxConcert.Domain.Common;
using TxConcert.Domain.Prompts;
using TxConcert.Infrastructure.Persistence;
using TxConcert.Infrastructure.Services.Prompts;

namespace TxConcert.Infrastructure.Services;

public sealed class AiPromptProvider(ApplicationDbContext db) : IAiPromptProvider
{
    public async Task<AiPromptContent> GetConcertEvaluationPromptAsync(CancellationToken ct = default)
    {
        AiPromptEntity? activePrompt = await db.AiPrompts
            .AsNoTracking()
            .Where(prompt => prompt.Key == Constants.AiPrompts.ConcertEvaluation && prompt.IsActive)
            .OrderByDescending(prompt => prompt.VersionNumber)
            .FirstOrDefaultAsync(ct);

        return activePrompt is not null
            ? new AiPromptContent(
                activePrompt.SystemPrompt,
                activePrompt.UserPromptInstructions,
                "database",
                activePrompt.VersionNumber)
            : new AiPromptContent(
                ConcertEvaluationPrompts.DefaultSystemPrompt,
                ConcertEvaluationPrompts.DefaultUserPromptInstructions,
                "default",
                null);
    }
}
