using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TxConcert.Domain.Common;
using TxConcert.Domain.Prompts;
using TxConcert.Infrastructure.Persistence;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/admin/ai-prompts")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class AdminAiPromptsController(
    ApplicationDbContext db,
    IAiPromptProvider promptProvider) : ControllerBase
{
    [HttpGet("{key}")]
    public async Task<ActionResult<AiPromptResponse>> GetCurrent(string key, CancellationToken ct)
    {
        if (!IsSupportedPromptKey(key))
            return NotFound();

        AiPromptContent prompt = await promptProvider.GetConcertEvaluationPromptAsync(ct);

        AiPromptEntity? activePrompt = await db.AiPrompts
            .AsNoTracking()
            .Where(p => p.Key == key && p.IsActive)
            .OrderByDescending(p => p.VersionNumber)
            .FirstOrDefaultAsync(ct);

        return Ok(new AiPromptResponse(
            activePrompt?.Id,
            key,
            activePrompt?.Name ?? "Concert Evaluation",
            prompt.SystemPrompt,
            prompt.UserPromptInstructions,
            prompt.VersionNumber,
            prompt.Source,
            activePrompt?.CreatedAt,
            activePrompt?.UpdatedAt));
    }

    [HttpGet("{key}/versions")]
    public async Task<ActionResult<IReadOnlyList<AiPromptVersionResponse>>> GetVersions(
        string key,
        CancellationToken ct)
    {
        if (!IsSupportedPromptKey(key))
            return NotFound();

        List<AiPromptVersionResponse> versions = await db.AiPrompts
            .AsNoTracking()
            .Where(p => p.Key == key)
            .OrderByDescending(p => p.VersionNumber)
            .Select(p => new AiPromptVersionResponse(
                p.Id,
                p.Key,
                p.Name,
                p.VersionNumber,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(ct);

        return Ok(versions);
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<AiPromptResponse>> Update(
        string key,
        UpdateAiPromptRequest request,
        CancellationToken ct)
    {
        if (!IsSupportedPromptKey(key))
            return NotFound();

        string systemPrompt = request.SystemPrompt?.Trim() ?? "";
        string userPromptInstructions = request.UserPromptInstructions?.Trim() ?? "";
        string name = string.IsNullOrWhiteSpace(request.Name)
            ? "Concert Evaluation"
            : request.Name.Trim();

        if (string.IsNullOrWhiteSpace(systemPrompt))
            return BadRequest("SystemPrompt is required.");

        if (string.IsNullOrWhiteSpace(userPromptInstructions))
            return BadRequest("UserPromptInstructions is required.");

        if (systemPrompt.Length > Constants.Validation.MaxPromptLength)
            return BadRequest($"SystemPrompt must be {Constants.Validation.MaxPromptLength} characters or less.");

        if (userPromptInstructions.Length > Constants.Validation.MaxPromptLength)
            return BadRequest($"UserPromptInstructions must be {Constants.Validation.MaxPromptLength} characters or less.");

        if (name.Length > Constants.Validation.MaxNameLength)
            return BadRequest($"Name must be {Constants.Validation.MaxNameLength} characters or less.");

        List<AiPromptEntity> activePrompts = await db.AiPrompts
            .Where(p => p.Key == key && p.IsActive)
            .ToListAsync(ct);

        foreach (AiPromptEntity activePrompt in activePrompts)
        {
            activePrompt.Deactivate();
        }

        int nextVersion = await db.AiPrompts
            .Where(p => p.Key == key)
            .MaxAsync(p => (int?)p.VersionNumber, ct) ?? 0;
        nextVersion++;

        AiPromptEntity prompt = AiPromptEntity.Create(
            key,
            name,
            systemPrompt,
            userPromptInstructions,
            nextVersion);

        db.AiPrompts.Add(prompt);
        await db.SaveChangesAsync(ct);

        return Ok(new AiPromptResponse(
            prompt.Id,
            prompt.Key,
            prompt.Name,
            prompt.SystemPrompt,
            prompt.UserPromptInstructions,
            prompt.VersionNumber,
            "database",
            prompt.CreatedAt,
            prompt.UpdatedAt));
    }

    private static bool IsSupportedPromptKey(string key) =>
        key == Constants.AiPrompts.ConcertEvaluation;
}

public sealed record UpdateAiPromptRequest(
    string? Name,
    string? SystemPrompt,
    string? UserPromptInstructions);

public sealed record AiPromptResponse(
    string? Id,
    string Key,
    string Name,
    string SystemPrompt,
    string UserPromptInstructions,
    int? VersionNumber,
    string Source,
    NodaTime.Instant? CreatedAt,
    NodaTime.Instant? UpdatedAt);

public sealed record AiPromptVersionResponse(
    string Id,
    string Key,
    string Name,
    int VersionNumber,
    bool IsActive,
    NodaTime.Instant CreatedAt,
    NodaTime.Instant UpdatedAt);
