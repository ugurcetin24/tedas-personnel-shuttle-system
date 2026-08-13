using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.RoutePoints;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class RoutePointService(
    IRoutePointRepository routePointRepository,
    IValidator<CreateRoutePointRequest> createValidator,
    IValidator<UpdateRoutePointRequest> updateValidator)
    : IRoutePointService
{
    public async Task<IReadOnlyList<RoutePointListItemDto>?> ListByShiftAsync(
        Guid shuttleShiftId,
        CancellationToken cancellationToken)
    {
        if (await routePointRepository.GetShiftByIdAsync(shuttleShiftId, cancellationToken) is null)
        {
            return null;
        }

        var routePoints = await routePointRepository.ListByShiftAsync(shuttleShiftId, cancellationToken);

        return routePoints.Select(MapListItem).ToArray();
    }

    public async Task<RoutePointDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var routePoint = await routePointRepository.GetByIdAsync(id, cancellationToken);

        return routePoint is null ? null : MapDetails(routePoint);
    }

    public async Task<RoutePointDto?> CreateAsync(
        Guid shuttleShiftId,
        CreateRoutePointRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await routePointRepository.GetShiftByIdAsync(shuttleShiftId, cancellationToken) is null)
        {
            return null;
        }

        var routePoint = new RoutePoint(
            shuttleShiftId,
            await routePointRepository.GetNextOrderAsync(shuttleShiftId, cancellationToken),
            NormalizeRequired(request.Name),
            NormalizeOptional(request.Address),
            request.Latitude,
            request.Longitude,
            DateTimeOffset.UtcNow);

        await routePointRepository.AddAsync(routePoint, cancellationToken);
        await routePointRepository.SaveChangesAsync(cancellationToken);

        var saved = await routePointRepository.GetByIdAsync(routePoint.Id, cancellationToken);
        return MapDetails(saved ?? routePoint);
    }

    public async Task<RoutePointDto?> UpdateAsync(
        Guid id,
        UpdateRoutePointRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var routePoint = await routePointRepository.GetByIdAsync(id, cancellationToken);
        if (routePoint is null)
        {
            return null;
        }

        routePoint.Update(
            NormalizeRequired(request.Name),
            NormalizeOptional(request.Address),
            request.Latitude,
            request.Longitude,
            DateTimeOffset.UtcNow);

        await routePointRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(routePoint);
    }

    public async Task<RoutePointDto?> UpdateStatusAsync(
        Guid id,
        UpdateRoutePointStatusRequest request,
        CancellationToken cancellationToken)
    {
        var routePoint = await routePointRepository.GetByIdAsync(id, cancellationToken);
        if (routePoint is null)
        {
            return null;
        }

        routePoint.SetActiveStatus(request.IsActive, DateTimeOffset.UtcNow);
        await routePointRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(routePoint);
    }

    public async Task<IReadOnlyList<RoutePointListItemDto>?> ReorderAsync(
        Guid shuttleShiftId,
        ReorderRoutePointsRequest request,
        CancellationToken cancellationToken)
    {
        var routePoints = await routePointRepository.ListByShiftAsync(shuttleShiftId, cancellationToken);
        if (routePoints.Count == 0 && await routePointRepository.GetShiftByIdAsync(shuttleShiftId, cancellationToken) is null)
        {
            return null;
        }

        if (request.RoutePointIds.Count != routePoints.Count
            || request.RoutePointIds.Distinct().Count() != routePoints.Count
            || routePoints.Any(routePoint => !request.RoutePointIds.Contains(routePoint.Id)))
        {
            throw new BusinessConflictException(
                "ROUTE_POINT_ORDER_INVALID",
                "Siralamada vardiyaya ait tum guzergah noktalari eksiksiz ve tekrarsiz verilmelidir.");
        }

        var now = DateTimeOffset.UtcNow;
        var routePointById = routePoints.ToDictionary(routePoint => routePoint.Id);
        foreach (var routePoint in routePoints)
        {
            routePoint.SetOrder(-routePoint.Order, now);
        }

        await routePointRepository.SaveChangesAsync(cancellationToken);

        now = DateTimeOffset.UtcNow;
        for (var index = 0; index < request.RoutePointIds.Count; index++)
        {
            routePointById[request.RoutePointIds[index]].SetOrder(index + 1, now);
        }

        await routePointRepository.SaveChangesAsync(cancellationToken);

        return (await routePointRepository.ListByShiftAsync(shuttleShiftId, cancellationToken))
            .Select(MapListItem)
            .ToArray();
    }

    private static RoutePointListItemDto MapListItem(RoutePoint routePoint)
    {
        return new RoutePointListItemDto(
            routePoint.Id,
            routePoint.ShuttleShiftId,
            routePoint.ShuttleShift?.PhysicalShuttle?.Code ?? string.Empty,
            routePoint.ShuttleShift?.Name ?? string.Empty,
            routePoint.Order,
            routePoint.Name,
            routePoint.Address,
            routePoint.Latitude,
            routePoint.Longitude,
            routePoint.IsActive);
    }

    private static RoutePointDto MapDetails(RoutePoint routePoint)
    {
        return new RoutePointDto(
            routePoint.Id,
            routePoint.ShuttleShiftId,
            routePoint.ShuttleShift?.PhysicalShuttle?.Code ?? string.Empty,
            routePoint.ShuttleShift?.Name ?? string.Empty,
            routePoint.Order,
            routePoint.Name,
            routePoint.Address,
            routePoint.Latitude,
            routePoint.Longitude,
            routePoint.IsActive,
            routePoint.CreatedAt,
            routePoint.UpdatedAt);
    }

    private static string NormalizeRequired(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
