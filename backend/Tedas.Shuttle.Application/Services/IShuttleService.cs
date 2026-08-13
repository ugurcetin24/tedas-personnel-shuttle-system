using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Shuttles;

namespace Tedas.Shuttle.Application.Services;

public interface IShuttleService
{
    Task<PaginatedList<ShuttleListItemDto>> SearchAsync(
        ShuttleQuery query,
        CancellationToken cancellationToken);

    Task<ShuttleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ShuttleDto> CreateAsync(CreateShuttleRequest request, CancellationToken cancellationToken);

    Task<ShuttleDto?> UpdateAsync(
        Guid id,
        UpdateShuttleRequest request,
        CancellationToken cancellationToken);

    Task<ShuttleDto?> UpdateStatusAsync(
        Guid id,
        UpdateShuttleStatusRequest request,
        CancellationToken cancellationToken);
}
