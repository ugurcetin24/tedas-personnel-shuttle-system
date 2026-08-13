using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Assignments;

namespace Tedas.Shuttle.Application.Services;

public interface IAssignmentService
{
    Task<PaginatedList<AssignmentListItemDto>> SearchAsync(
        AssignmentQuery query,
        CancellationToken cancellationToken);

    Task<AssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AssignmentDto> CreateAsync(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken);

    Task<AssignmentDto?> DeactivateAsync(Guid id, CancellationToken cancellationToken);
}

