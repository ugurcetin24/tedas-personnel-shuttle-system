using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
public sealed class ShiftsController(IShiftService shiftService) : ControllerBase
{
    [HttpGet("api/shuttles/{shuttleId:guid}/shifts")]
    public async Task<IActionResult> ListByShuttle(
        Guid shuttleId,
        CancellationToken cancellationToken)
    {
        var shifts = await shiftService.ListByShuttleAsync(shuttleId, cancellationToken);

        return shifts is null ? NotFound() : Ok(shifts);
    }

    [HttpPost("api/shuttles/{shuttleId:guid}/shifts")]
    public async Task<IActionResult> Create(
        Guid shuttleId,
        CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await shiftService.CreateAsync(shuttleId, request, cancellationToken);

        return shift is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift);
    }

    [HttpGet("api/shifts/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var shift = await shiftService.GetByIdAsync(id, cancellationToken);

        return shift is null ? NotFound() : Ok(shift);
    }

    [HttpPut("api/shifts/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await shiftService.UpdateAsync(id, request, cancellationToken);

        return shift is null ? NotFound() : Ok(shift);
    }

    [HttpPatch("api/shifts/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateShiftStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await shiftService.UpdateStatusAsync(id, request, cancellationToken);

        return shift is null ? NotFound() : Ok(shift);
    }
}
