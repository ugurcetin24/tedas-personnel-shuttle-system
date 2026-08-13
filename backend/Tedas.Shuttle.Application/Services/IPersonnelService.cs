using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Personnel;

namespace Tedas.Shuttle.Application.Services;

public interface IPersonnelService
{
    Task<PaginatedList<PersonnelListItemDto>> SearchAsync(
        PersonnelQuery query,
        CancellationToken cancellationToken);

    Task<PersonnelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PersonnelDto> CreateAsync(
        CreatePersonnelRequest request,
        CancellationToken cancellationToken);

    Task<PersonnelDto?> UpdateAsync(
        Guid id,
        UpdatePersonnelRequest request,
        CancellationToken cancellationToken);

    Task<PersonnelDto?> UpdateStatusAsync(
        Guid id,
        UpdatePersonnelStatusRequest request,
        CancellationToken cancellationToken);
}
