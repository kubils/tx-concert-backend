using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.ExternalAudits.Commands.CreateRequestAudit;
using TxConcert.Application.Features.ExternalAudits.Commands.RetryAudit;
using TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;
using TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudits;
using TxConcert.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/external-audits")]
[Authorize]
[EnableRateLimiting(Constants.RateLimit.Names.Default)]
public sealed class ExternalAuditsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ExternalAuditResponse>>> GetAll(
        [FromQuery] int limit = Constants.Pagination.DefaultLimit,
        [FromQuery] int offset = Constants.Pagination.DefaultOffset,
        [FromQuery] string? type = null,
        [FromQuery] string? targetId = null,
        CancellationToken ct = default)
    {
        PaginatedResponse<ExternalAuditResponse> result =
            await mediator.Send(new GetExternalAuditsQuery(limit, offset, type, targetId), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExternalAuditResponse>> GetById(string id, CancellationToken ct)
    {
        ExternalAuditResponse? result = await mediator.Send(new GetExternalAuditQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> Create(
        [FromBody] CreateRequestAuditCommand command,
        CancellationToken ct)
    {
        string id = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<string>.Ok(id));
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> Retry(string id, CancellationToken ct)
    {
        await mediator.Send(new RetryAuditCommand(id), ct);
        return NoContent();
    }
}
