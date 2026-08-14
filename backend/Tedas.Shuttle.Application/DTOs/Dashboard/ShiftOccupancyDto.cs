namespace Tedas.Shuttle.Application.DTOs.Dashboard;

public sealed record ShiftOccupancyDto(
    Guid ShuttleShiftId,
    string PhysicalShuttleCode,
    string ShiftName,
    int Capacity,
    int Occupancy,
    int AvailableSeats,
    decimal UtilizationPercent,
    bool IsActive);
