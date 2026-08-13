namespace Tedas.Shuttle.Application.DTOs.Drivers;

public sealed record DriverDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Phone,
    string LicenseNumber,
    bool IsActive,
    DriverShiftAssignmentDto? AssignedShift,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

