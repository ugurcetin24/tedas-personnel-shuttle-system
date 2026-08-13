using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Drivers;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/drivers")]
public sealed class DriversController(IDriverService driverService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await driverService.SearchAsync(
            new DriverQuery(page, pageSize, search, isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await driverService.GetByIdAsync(id, cancellationToken);

        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDriverRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateDriverRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverService.UpdateAsync(id, request, cancellationToken);

        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateDriverStatusRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverService.UpdateStatusAsync(id, request, cancellationToken);

        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPatch("{id:guid}/shift-assignment")]
    public async Task<IActionResult> UpdateShiftAssignment(
        Guid id,
        UpdateDriverShiftAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverService.UpdateShiftAssignmentAsync(id, request, cancellationToken);

        return driver is null ? NotFound() : Ok(driver);
    }
}

