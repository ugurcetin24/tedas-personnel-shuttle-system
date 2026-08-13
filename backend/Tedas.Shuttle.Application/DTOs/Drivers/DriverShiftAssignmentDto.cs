using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.DTOs.Drivers;

public sealed record DriverShiftAssignmentDto(
    Guid ShuttleShiftId,
    Guid PhysicalShuttleId,
    string PhysicalShuttleCode,
    string ShiftName,
    ShiftType ShiftType);

