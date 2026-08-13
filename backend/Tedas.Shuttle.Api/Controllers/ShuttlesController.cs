using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/shuttles")]
public sealed class ShuttlesController(IShuttleService shuttleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? code = null,
        [FromQuery] string? plateNumber = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await shuttleService.SearchAsync(
            new ShuttleQuery(page, pageSize, code, plateNumber, isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var shuttle = await shuttleService.GetByIdAsync(id, cancellationToken);

        return shuttle is null ? NotFound() : Ok(shuttle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateShuttleRequest request,
        CancellationToken cancellationToken)
    {
        var shuttle = await shuttleService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = shuttle.Id }, shuttle);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateShuttleRequest request,
        CancellationToken cancellationToken)
    {
        var shuttle = await shuttleService.UpdateAsync(id, request, cancellationToken);

        return shuttle is null ? NotFound() : Ok(shuttle);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateShuttleStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shuttle = await shuttleService.UpdateStatusAsync(id, request, cancellationToken);

        return shuttle is null ? NotFound() : Ok(shuttle);
    }
}
