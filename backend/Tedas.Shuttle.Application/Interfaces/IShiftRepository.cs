using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IShiftRepository
{
    Task<IReadOnlyList<ShuttleShift>> ListByShuttleAsync(
        Guid physicalShuttleId,
        CancellationToken cancellationToken);

    Task<ShuttleShift?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ShuttleExistsAsync(Guid physicalShuttleId, CancellationToken cancellationToken);

    Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken);

    Task AddAsync(ShuttleShift shift, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
