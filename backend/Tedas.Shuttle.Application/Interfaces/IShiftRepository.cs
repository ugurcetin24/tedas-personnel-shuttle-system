using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IShiftRepository
{
    Task<IReadOnlyList<ShuttleShift>> ListAsync(
        bool? isActive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ShuttleShift>> ListByShuttleAsync(
        Guid physicalShuttleId,
        CancellationToken cancellationToken);

    Task<ShuttleShift?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ShuttleShift>> ListByShuttleCodesAsync(
        IReadOnlyCollection<string> shuttleCodes,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, PhysicalShuttle>> GetShuttlesByCodesAsync(
        IReadOnlyCollection<string> shuttleCodes,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetActiveAssignmentCountsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken);

    Task<bool> ShuttleExistsAsync(Guid physicalShuttleId, CancellationToken cancellationToken);

    Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken);

    Task AddAsync(ShuttleShift shift, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<ShuttleShift> shifts, CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
