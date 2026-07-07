using TxConcert.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health-check")]
    [EnableRateLimiting(Constants.RateLimit.Names.Public)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}
