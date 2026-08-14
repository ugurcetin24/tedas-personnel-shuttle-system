using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Routing;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class RouteCalculationService(
    ISavedRouteRepository savedRouteRepository,
    IRoutingService routingService)
    : IRouteCalculationService
{
    public async Task<CalculatedRouteDto?> CalculateByShiftAsync(
        Guid shuttleShiftId,
        CancellationToken cancellationToken)
    {
        var routePoints = await savedRouteRepository.ListActiveRoutePointsByShiftAsync(
            shuttleShiftId,
            cancellationToken);

        if (routePoints.Count < 2)
        {
            throw new BusinessConflictException(
                "ROUTE_POINT_COUNT_INSUFFICIENT",
                "Rota hesaplamak icin en az iki aktif guzergah noktasi gereklidir.");
        }

        var coordinates = routePoints
            .Select(routePoint => new RouteCoordinateDto(routePoint.Latitude, routePoint.Longitude))
            .ToArray();

        return await routingService.CalculateAsync(coordinates, cancellationToken);
    }

    public async Task<IReadOnlyList<SavedRouteDto>?> ListSavedByShiftAsync(
        Guid shuttleShiftId,
        CancellationToken cancellationToken)
    {
        if (await savedRouteRepository.GetShiftByIdAsync(shuttleShiftId, cancellationToken) is null)
        {
            return null;
        }

        var savedRoutes = await savedRouteRepository.ListByShiftAsync(shuttleShiftId, cancellationToken);

        return savedRoutes.Select(Map).ToArray();
    }

    public async Task<SavedRouteDto?> SaveCalculatedRouteAsync(
        Guid shuttleShiftId,
        SaveRouteRequest request,
        CancellationToken cancellationToken)
    {
        if (await savedRouteRepository.GetShiftByIdAsync(shuttleShiftId, cancellationToken) is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BusinessConflictException(
                "SAVED_ROUTE_NAME_REQUIRED",
                "Kaydedilecek rota adi zorunludur.");
        }

        var calculated = await CalculateByShiftAsync(shuttleShiftId, cancellationToken);
        if (calculated is null)
        {
            throw new BusinessConflictException(
                "ROUTE_CALCULATION_FAILED",
                "Rota hesaplanamadi.");
        }

        var savedRoute = new SavedRoute(
            shuttleShiftId,
            NormalizeRequired(request.Name),
            calculated.DistanceMeters,
            calculated.DurationSeconds,
            calculated.Geometry,
            DateTimeOffset.UtcNow);

        await savedRouteRepository.AddAsync(savedRoute, cancellationToken);
        await savedRouteRepository.SaveChangesAsync(cancellationToken);

        var savedRoutes = await savedRouteRepository.ListByShiftAsync(shuttleShiftId, cancellationToken);
        return Map(savedRoutes.Single(route => route.Id == savedRoute.Id));
    }

    private static SavedRouteDto Map(SavedRoute savedRoute)
    {
        return new SavedRouteDto(
            savedRoute.Id,
            savedRoute.ShuttleShiftId,
            savedRoute.ShuttleShift?.PhysicalShuttle?.Code ?? string.Empty,
            savedRoute.ShuttleShift?.Name ?? string.Empty,
            savedRoute.Name,
            savedRoute.DistanceMeters,
            savedRoute.DurationSeconds,
            savedRoute.Geometry,
            savedRoute.CreatedAt,
            savedRoute.UpdatedAt);
    }

    private static string NormalizeRequired(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

