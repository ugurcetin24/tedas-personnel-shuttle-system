namespace Tedas.Shuttle.Application.DTOs.Personnel;

public sealed record PersonnelDto(
    Guid Id,
    string RegistrationNumber,
    string FirstName,
    string LastName,
    string FullName,
    string? Department,
    string? Title,
    string? Phone,
    string? Email,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    PersonnelAssignmentSummaryDto? CurrentAssignment);
