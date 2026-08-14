using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.DTOs.Routing;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
public sealed class RoutesController(IRouteCalculationService routeCalculationService) : ControllerBase
{
    [HttpPost("api/shifts/{shiftId:guid}/routes/calculate")]
    public async Task<IActionResult> Calculate(Guid shiftId, CancellationToken cancellationToken)
    {
        var route = await routeCalculationService.CalculateByShiftAsync(shiftId, cancellationToken);

        return route is null ? StatusCode(StatusCodes.Status502BadGateway) : Ok(route);
    }

    [HttpGet("api/shifts/{shiftId:guid}/routes")]
    public async Task<IActionResult> ListSaved(Guid shiftId, CancellationToken cancellationToken)
    {
        var routes = await routeCalculationService.ListSavedByShiftAsync(shiftId, cancellationToken);

        return routes is null ? NotFound() : Ok(routes);
    }

    [HttpPost("api/shifts/{shiftId:guid}/routes")]
    public async Task<IActionResult> Save(
        Guid shiftId,
        SaveRouteRequest request,
        CancellationToken cancellationToken)
    {
        var savedRoute = await routeCalculationService.SaveCalculatedRouteAsync(
            shiftId,
            request,
            cancellationToken);

        return savedRoute is null ? NotFound() : Ok(savedRoute);
    }
}

