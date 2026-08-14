using Tedas.Shuttle.Application.DTOs.Routing;

namespace Tedas.Shuttle.Application.Services;

public interface IRouteCalculationService
{
    Task<CalculatedRouteDto?> CalculateByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SavedRouteDto>?> ListSavedByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<SavedRouteDto?> SaveCalculatedRouteAsync(Guid shuttleShiftId, SaveRouteRequest request, CancellationToken cancellationToken);
}

