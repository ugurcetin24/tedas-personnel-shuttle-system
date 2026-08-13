using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IAssignmentRepository
{
    Task<PaginatedList<PersonnelAssignment>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<PersonnelAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Personnel?> GetPersonnelByIdAsync(Guid personnelId, CancellationToken cancellationToken);

    Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken);

    Task<bool> PersonnelHasActiveAssignmentAsync(Guid personnelId, CancellationToken cancellationToken);

    Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken);

    Task AddAsync(PersonnelAssignment assignment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

