namespace Tedas.Shuttle.Application.DTOs.RoutePoints;

public sealed record RoutePointDto(
    Guid Id,
    Guid ShuttleShiftId,
    string PhysicalShuttleCode,
    string ShiftName,
    int Order,
    string Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

