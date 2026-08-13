using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Assignments;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class AssignmentService(
    IAssignmentRepository assignmentRepository,
    IValidator<CreateAssignmentRequest> createValidator)
    : IAssignmentService
{
    public async Task<PaginatedList<AssignmentListItemDto>> SearchAsync(
        AssignmentQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var assignments = await assignmentRepository.SearchAsync(
            page,
            pageSize,
            NormalizeOptional(query.Search),
            query.IsActive,
            cancellationToken);

        var items = new List<AssignmentListItemDto>(assignments.Items.Count);
        foreach (var assignment in assignments.Items)
        {
            var occupancy = await assignmentRepository.GetActiveAssignmentCountAsync(
                assignment.ShuttleShiftId,
                cancellationToken);
            items.Add(MapListItem(assignment, occupancy));
        }

        return new PaginatedList<AssignmentListItemDto>(
            items,
            assignments.Page,
            assignments.PageSize,
            assignments.TotalCount);
    }

    public async Task<AssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id, cancellationToken);
        if (assignment is null)
        {
            return null;
        }

        var occupancy = await assignmentRepository.GetActiveAssignmentCountAsync(
            assignment.ShuttleShiftId,
            cancellationToken);

        return MapDetails(assignment, occupancy);
    }

    public async Task<AssignmentDto> CreateAsync(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var personnel = await assignmentRepository.GetPersonnelByIdAsync(request.PersonnelId, cancellationToken);
        if (personnel is null)
        {
            throw new BusinessConflictException(
                "PERSONNEL_NOT_FOUND",
                "Atanacak personel bulunamadi.");
        }

        if (!personnel.IsActive)
        {
            throw new BusinessConflictException(
                "PERSONNEL_INACTIVE",
                "Pasif personel servis vardiyasina atanamaz.");
        }

        var shift = await assignmentRepository.GetShiftByIdAsync(request.ShuttleShiftId, cancellationToken);
        if (shift is null)
        {
            throw new BusinessConflictException(
                "SHIFT_NOT_FOUND",
                "Personelin atanacagi vardiya bulunamadi.");
        }

        if (!shift.IsActive)
        {
            throw new BusinessConflictException(
                "SHIFT_INACTIVE",
                "Pasif servis vardiyasina personel atanamaz.");
        }

        if (shift.PhysicalShuttle is { IsActive: false })
        {
            throw new BusinessConflictException(
                "SHUTTLE_INACTIVE",
                "Pasif servisin vardiyasina personel atanamaz.");
        }

        if (await assignmentRepository.PersonnelHasActiveAssignmentAsync(personnel.Id, cancellationToken))
        {
            throw new BusinessConflictException(
                "PERSONNEL_ASSIGNMENT_DUPLICATE",
                "Personelin zaten aktif bir servis atamasi var.");
        }

        var occupancy = await assignmentRepository.GetActiveAssignmentCountAsync(shift.Id, cancellationToken);
        if (occupancy >= shift.Capacity)
        {
            throw new BusinessConflictException(
                "SHUTTLE_CAPACITY_FULL",
                "Servis vardiyasi kapasitesi dolu.");
        }

        var assignment = new PersonnelAssignment(
            personnel.Id,
            shift.Id,
            request.BoardingRoutePointId,
            DateTimeOffset.UtcNow);

        await assignmentRepository.AddAsync(assignment, cancellationToken);
        await assignmentRepository.SaveChangesAsync(cancellationToken);

        var saved = await assignmentRepository.GetByIdAsync(assignment.Id, cancellationToken);
        return MapDetails(saved ?? assignment, occupancy + 1);
    }

    public async Task<AssignmentDto?> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id, cancellationToken);
        if (assignment is null)
        {
            return null;
        }

        if (assignment.IsActive)
        {
            assignment.Deactivate(DateTimeOffset.UtcNow);
            await assignmentRepository.SaveChangesAsync(cancellationToken);
        }

        var occupancy = await assignmentRepository.GetActiveAssignmentCountAsync(
            assignment.ShuttleShiftId,
            cancellationToken);

        return MapDetails(assignment, occupancy);
    }

    private static AssignmentListItemDto MapListItem(PersonnelAssignment assignment, int occupancy)
    {
        var shift = assignment.ShuttleShift;
        var personnel = assignment.Personnel;

        return new AssignmentListItemDto(
            assignment.Id,
            assignment.PersonnelId,
            personnel?.RegistrationNumber ?? string.Empty,
            personnel?.FullName ?? string.Empty,
            personnel?.Department,
            assignment.ShuttleShiftId,
            shift?.PhysicalShuttle?.Code ?? string.Empty,
            shift?.Name ?? string.Empty,
            shift?.Capacity ?? 0,
            occupancy,
            (shift?.Capacity ?? 0) - occupancy,
            assignment.BoardingRoutePointId,
            assignment.IsActive,
            assignment.AssignedAt);
    }

    private static AssignmentDto MapDetails(PersonnelAssignment assignment, int occupancy)
    {
        var shift = assignment.ShuttleShift;
        var personnel = assignment.Personnel;

        return new AssignmentDto(
            assignment.Id,
            assignment.PersonnelId,
            personnel?.RegistrationNumber ?? string.Empty,
            personnel?.FullName ?? string.Empty,
            personnel?.Department,
            assignment.ShuttleShiftId,
            shift?.PhysicalShuttle?.Code ?? string.Empty,
            shift?.Name ?? string.Empty,
            shift?.Capacity ?? 0,
            occupancy,
            (shift?.Capacity ?? 0) - occupancy,
            assignment.BoardingRoutePointId,
            assignment.IsActive,
            assignment.AssignedAt,
            assignment.DeactivatedAt);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

