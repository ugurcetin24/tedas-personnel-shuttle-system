namespace Tedas.Shuttle.Application.DTOs.Assignments;

public sealed record AssignmentDto(
    Guid Id,
    Guid PersonnelId,
    string RegistrationNumber,
    string PersonnelFullName,
    string? Department,
    Guid ShuttleShiftId,
    string PhysicalShuttleCode,
    string ShiftName,
    int Capacity,
    int Occupancy,
    int AvailableSeats,
    Guid? BoardingRoutePointId,
    bool IsActive,
    DateTimeOffset AssignedAt,
    DateTimeOffset? DeactivatedAt);

