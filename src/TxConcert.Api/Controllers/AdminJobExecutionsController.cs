using TxConcert.Domain.Common;
using TxConcert.Domain.Jobs;
using TxConcert.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/admin/job-executions")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class AdminJobExecutionsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<JobExecutionLogListResponse>> GetAll(
        [FromQuery] int limit = Constants.Pagination.DefaultLimit,
        [FromQuery] int offset = Constants.Pagination.DefaultOffset,
        [FromQuery] string? jobName = null,
        [FromQuery] JobExecutionStatus? status = null,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, Constants.Pagination.MaxLimit);
        offset = Math.Max(offset, 0);

        IQueryable<JobExecutionLogEntity> query = db.JobExecutionLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(jobName))
            query = query.Where(log => log.JobName == jobName);

        if (status.HasValue)
            query = query.Where(log => log.Status == status.Value);

        long totalCount = await query.LongCountAsync(ct);
        List<JobExecutionLogResponse> items = await query
            .OrderByDescending(log => log.StartedAt)
            .Skip(offset)
            .Take(limit)
            .Select(log => new JobExecutionLogResponse(
                log.Id,
                log.JobName,
                log.JobGroup,
                log.TriggerName,
                log.TriggerGroup,
                log.FireInstanceId,
                log.Status,
                log.StartedAt,
                log.CompletedAt,
                log.DurationMs,
                log.ErrorMessage))
            .ToListAsync(ct);

        return Ok(new JobExecutionLogListResponse(items, totalCount, limit, offset));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobExecutionLogResponse>> GetById(string id, CancellationToken ct)
    {
        JobExecutionLogResponse? result = await db.JobExecutionLogs
            .AsNoTracking()
            .Where(log => log.Id == id)
            .Select(log => new JobExecutionLogResponse(
                log.Id,
                log.JobName,
                log.JobGroup,
                log.TriggerName,
                log.TriggerGroup,
                log.FireInstanceId,
                log.Status,
                log.StartedAt,
                log.CompletedAt,
                log.DurationMs,
                log.ErrorMessage))
            .FirstOrDefaultAsync(ct);

        return result is null ? NotFound() : Ok(result);
    }
}

public sealed record JobExecutionLogListResponse(
    IReadOnlyList<JobExecutionLogResponse> Items,
    long TotalCount,
    int Limit,
    int Offset);

public sealed record JobExecutionLogResponse(
    string Id,
    string JobName,
    string JobGroup,
    string TriggerName,
    string TriggerGroup,
    string FireInstanceId,
    JobExecutionStatus Status,
    Instant StartedAt,
    Instant? CompletedAt,
    long? DurationMs,
    string? ErrorMessage);
