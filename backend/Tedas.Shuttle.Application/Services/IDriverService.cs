using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Drivers;

namespace Tedas.Shuttle.Application.Services;

public interface IDriverService
{
    Task<PaginatedList<DriverListItemDto>> SearchAsync(
        DriverQuery query,
        CancellationToken cancellationToken);

    Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<DriverDto> CreateAsync(
        CreateDriverRequest request,
        CancellationToken cancellationToken);

    Task<DriverDto?> UpdateAsync(
        Guid id,
        UpdateDriverRequest request,
        CancellationToken cancellationToken);

    Task<DriverDto?> UpdateStatusAsync(
        Guid id,
        UpdateDriverStatusRequest request,
        CancellationToken cancellationToken);

    Task<DriverDto?> UpdateShiftAssignmentAsync(
        Guid id,
        UpdateDriverShiftAssignmentRequest request,
        CancellationToken cancellationToken);
}

