using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.Artists.Queries;
using TxConcert.Application.Features.Artists.Queries.GetArtistDetail;
using TxConcert.Application.Features.Artists.Queries.GetArtists;
using TxConcert.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/artists")]
[EnableRateLimiting(Constants.RateLimit.Names.Public)]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class ArtistsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Get paginated list of artists with optional search and genre filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ArtistListResponse>>> GetAll(
        [FromQuery] int limit = Constants.Pagination.DefaultLimit,
        [FromQuery] int offset = Constants.Pagination.DefaultOffset,
        [FromQuery] string? search = null,
        [FromQuery] string? genreId = null,
        CancellationToken ct = default)
    {
        PaginatedResponse<ArtistListResponse> result = await mediator.Send(
            new GetArtistsQuery(limit, offset, search, genreId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get artist details by ID, including vibes, genres, and upcoming concerts.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ArtistDetailResponse>> GetById(string id, CancellationToken ct)
    {
        ArtistDetailResponse? result = await mediator.Send(new GetArtistDetailQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
