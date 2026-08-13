using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class ShiftService(
    IShiftRepository shiftRepository,
    IValidator<CreateShiftRequest> createValidator,
    IValidator<UpdateShiftRequest> updateValidator)
    : IShiftService
{
    public async Task<IReadOnlyList<ShiftListItemDto>> ListAsync(
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var shifts = await shiftRepository.ListAsync(isActive, cancellationToken);
        var items = new List<ShiftListItemDto>(shifts.Count);

        foreach (var shift in shifts)
        {
            var occupancy = await shiftRepository.GetActiveAssignmentCountAsync(shift.Id, cancellationToken);
            items.Add(MapListItem(shift, occupancy));
        }

        return items;
    }

    public async Task<IReadOnlyList<ShiftListItemDto>?> ListByShuttleAsync(
        Guid physicalShuttleId,
        CancellationToken cancellationToken)
    {
        if (!await shiftRepository.ShuttleExistsAsync(physicalShuttleId, cancellationToken))
        {
            return null;
        }

        var shifts = await shiftRepository.ListByShuttleAsync(physicalShuttleId, cancellationToken);
        var items = new List<ShiftListItemDto>(shifts.Count);

        foreach (var shift in shifts)
        {
            var occupancy = await shiftRepository.GetActiveAssignmentCountAsync(shift.Id, cancellationToken);
            items.Add(MapListItem(shift, occupancy));
        }

        return items;
    }

    public async Task<ShiftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var shift = await shiftRepository.GetByIdAsync(id, cancellationToken);
        if (shift is null)
        {
            return null;
        }

        var occupancy = await shiftRepository.GetActiveAssignmentCountAsync(id, cancellationToken);
        return MapDetails(shift, occupancy);
    }

    public async Task<ShiftDto?> CreateAsync(
        Guid physicalShuttleId,
        CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await shiftRepository.ShuttleExistsAsync(physicalShuttleId, cancellationToken))
        {
            return null;
        }

        var shift = new ShuttleShift(
            physicalShuttleId,
            NormalizeRequired(request.Name),
            request.ShiftType,
            request.Capacity,
            request.StartTime,
            request.EndTime,
            DateTimeOffset.UtcNow);

        await shiftRepository.AddAsync(shift, cancellationToken);
        await shiftRepository.SaveChangesAsync(cancellationToken);

        var saved = await shiftRepository.GetByIdAsync(shift.Id, cancellationToken);
        return saved is null ? MapDetails(shift, occupancy: 0) : MapDetails(saved, occupancy: 0);
    }

    public async Task<ShiftDto?> UpdateAsync(
        Guid id,
        UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var shift = await shiftRepository.GetByIdAsync(id, cancellationToken);
        if (shift is null)
        {
            return null;
        }

        var occupancy = await shiftRepository.GetActiveAssignmentCountAsync(id, cancellationToken);
        if (request.Capacity < occupancy)
        {
            throw new BusinessConflictException(
                "SHIFT_CAPACITY_BELOW_OCCUPANCY",
                "Servis vardiyası kapasitesi mevcut aktif atama sayısından düşük olamaz.");
        }

        shift.Update(
            NormalizeRequired(request.Name),
            request.ShiftType,
            request.Capacity,
            request.StartTime,
            request.EndTime,
            DateTimeOffset.UtcNow);

        await shiftRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(shift, occupancy);
    }

    public async Task<ShiftDto?> UpdateStatusAsync(
        Guid id,
        UpdateShiftStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shift = await shiftRepository.GetByIdAsync(id, cancellationToken);
        if (shift is null)
        {
            return null;
        }

        var occupancy = await shiftRepository.GetActiveAssignmentCountAsync(id, cancellationToken);
        shift.SetActiveStatus(request.IsActive, DateTimeOffset.UtcNow);
        await shiftRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(shift, occupancy);
    }

    private static ShiftListItemDto MapListItem(ShuttleShift shift, int occupancy)
    {
        return new ShiftListItemDto(
            shift.Id,
            shift.PhysicalShuttleId,
            shift.PhysicalShuttle?.Code ?? string.Empty,
            shift.Name,
            shift.ShiftType,
            shift.Capacity,
            occupancy,
            shift.Capacity - occupancy,
            shift.StartTime,
            shift.EndTime,
            shift.IsActive);
    }

    private static ShiftDto MapDetails(ShuttleShift shift, int occupancy)
    {
        return new ShiftDto(
            shift.Id,
            shift.PhysicalShuttleId,
            shift.PhysicalShuttle?.Code ?? string.Empty,
            shift.Name,
            shift.ShiftType,
            shift.Capacity,
            occupancy,
            shift.Capacity - occupancy,
            shift.StartTime,
            shift.EndTime,
            shift.IsActive,
            shift.CreatedAt,
            shift.UpdatedAt);
    }

    private static string NormalizeRequired(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
