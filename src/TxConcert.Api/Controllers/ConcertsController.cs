using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.Concerts.Queries;
using TxConcert.Application.Features.Concerts.Queries.GetConcertDetail;
using TxConcert.Application.Features.Concerts.Queries.GetConcerts;
using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NodaTime;
using Microsoft.AspNetCore.Authorization;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/concerts")]
[EnableRateLimiting(Constants.RateLimit.Names.Public)]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class ConcertsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Get paginated list of concerts with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ConcertListResponse>>> GetAll(
        [FromQuery] int limit = Constants.Pagination.DefaultLimit,
        [FromQuery] int offset = Constants.Pagination.DefaultOffset,
        [FromQuery] string? search = null,
        [FromQuery] string? stateId = null,
        [FromQuery] string? artistId = null,
        [FromQuery] ConcertType? concertType = null,
        [FromQuery] LocalDate? fromDate = null,
        [FromQuery] LocalDate? toDate = null,
        CancellationToken ct = default)
    {
        PaginatedResponse<ConcertListResponse> result = await mediator.Send(
            new GetConcertsQuery(limit, offset, search, stateId, artistId, concertType, fromDate, toDate), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get concert details by ID, including venue, artists, AI description and vibes.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConcertDetailResponse>> GetById(string id, CancellationToken ct)
    {
        ConcertDetailResponse? result = await mediator.Send(new GetConcertDetailQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
