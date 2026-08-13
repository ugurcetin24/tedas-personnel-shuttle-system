using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IPersonnelRepository
{
    Task<PaginatedList<Personnel>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        string? department,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Personnel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Personnel?> GetByRegistrationNumberAsync(
        string registrationNumber,
        CancellationToken cancellationToken);

    Task<bool> RegistrationNumberExistsAsync(
        string registrationNumber,
        Guid? excludedPersonnelId,
        CancellationToken cancellationToken);

    Task AddAsync(Personnel personnel, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
