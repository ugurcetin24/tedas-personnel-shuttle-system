using Tedas.Shuttle.Application.DTOs.Shifts;

namespace Tedas.Shuttle.Application.Services;

public interface IShiftService
{
    Task<IReadOnlyList<ShiftListItemDto>?> ListByShuttleAsync(
        Guid physicalShuttleId,
        CancellationToken cancellationToken);

    Task<ShiftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ShiftDto?> CreateAsync(
        Guid physicalShuttleId,
        CreateShiftRequest request,
        CancellationToken cancellationToken);

    Task<ShiftDto?> UpdateAsync(
        Guid id,
        UpdateShiftRequest request,
        CancellationToken cancellationToken);

    Task<ShiftDto?> UpdateStatusAsync(
        Guid id,
        UpdateShiftStatusRequest request,
        CancellationToken cancellationToken);
}
