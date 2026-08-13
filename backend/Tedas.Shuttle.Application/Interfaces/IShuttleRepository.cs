using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IShuttleRepository
{
    Task<PaginatedList<PhysicalShuttle>> SearchAsync(
        int page,
        int pageSize,
        string? code,
        string? plateNumber,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<PhysicalShuttle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedShuttleId,
        CancellationToken cancellationToken);

    Task AddAsync(PhysicalShuttle shuttle, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
