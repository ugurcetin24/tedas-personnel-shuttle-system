using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Personnel;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/personnel")]
public sealed class PersonnelController(IPersonnelService personnelService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await personnelService.SearchAsync(
            new PersonnelQuery(page, pageSize, search, department, isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var personnel = await personnelService.GetByIdAsync(id, cancellationToken);

        return personnel is null ? NotFound() : Ok(personnel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = personnel.Id }, personnel);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelService.UpdateAsync(id, request, cancellationToken);

        return personnel is null ? NotFound() : Ok(personnel);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdatePersonnelStatusRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelService.UpdateStatusAsync(id, request, cancellationToken);

        return personnel is null ? NotFound() : Ok(personnel);
    }
}
