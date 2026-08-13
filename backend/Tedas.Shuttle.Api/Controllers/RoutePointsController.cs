using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.RoutePoints;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
public sealed class RoutePointsController(IRoutePointService routePointService) : ControllerBase
{
    [HttpGet("api/shifts/{shiftId:guid}/route-points")]
    public async Task<IActionResult> ListByShift(Guid shiftId, CancellationToken cancellationToken)
    {
        var routePoints = await routePointService.ListByShiftAsync(shiftId, cancellationToken);

        return routePoints is null ? NotFound() : Ok(routePoints);
    }

    [HttpPost("api/shifts/{shiftId:guid}/route-points")]
    public async Task<IActionResult> Create(
        Guid shiftId,
        CreateRoutePointRequest request,
        CancellationToken cancellationToken)
    {
        var routePoint = await routePointService.CreateAsync(shiftId, request, cancellationToken);

        return routePoint is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = routePoint.Id }, routePoint);
    }

    [HttpPatch("api/shifts/{shiftId:guid}/route-points/order")]
    public async Task<IActionResult> Reorder(
        Guid shiftId,
        ReorderRoutePointsRequest request,
        CancellationToken cancellationToken)
    {
        var routePoints = await routePointService.ReorderAsync(shiftId, request, cancellationToken);

        return routePoints is null ? NotFound() : Ok(routePoints);
    }

    [HttpGet("api/route-points/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var routePoint = await routePointService.GetByIdAsync(id, cancellationToken);

        return routePoint is null ? NotFound() : Ok(routePoint);
    }

    [HttpPut("api/route-points/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateRoutePointRequest request,
        CancellationToken cancellationToken)
    {
        var routePoint = await routePointService.UpdateAsync(id, request, cancellationToken);

        return routePoint is null ? NotFound() : Ok(routePoint);
    }

    [HttpPatch("api/route-points/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateRoutePointStatusRequest request,
        CancellationToken cancellationToken)
    {
        var routePoint = await routePointService.UpdateStatusAsync(id, request, cancellationToken);

        return routePoint is null ? NotFound() : Ok(routePoint);
    }
}

