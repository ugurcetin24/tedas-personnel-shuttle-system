using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.DTOs.Shifts;

public sealed record ShiftListItemDto(
    Guid Id,
    Guid PhysicalShuttleId,
    string PhysicalShuttleCode,
    string Name,
    ShiftType ShiftType,
    int Capacity,
    int Occupancy,
    int AvailableSeats,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);
