using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/geocoding")]
public sealed class GeocodingController(IGeocodingService geocodingService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await geocodingService.SearchAsync(query, limit, cancellationToken);

        return Ok(results);
    }
}

