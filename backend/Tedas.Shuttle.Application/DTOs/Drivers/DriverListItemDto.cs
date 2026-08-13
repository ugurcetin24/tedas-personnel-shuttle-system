namespace Tedas.Shuttle.Application.DTOs.Drivers;

public sealed record DriverListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Phone,
    string LicenseNumber,
    bool IsActive,
    DriverShiftAssignmentDto? AssignedShift);

