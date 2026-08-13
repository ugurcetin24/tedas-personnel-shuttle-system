namespace Tedas.Shuttle.Application.DTOs.Personnel;

public sealed record PersonnelListItemDto(
    Guid Id,
    string RegistrationNumber,
    string FullName,
    string? Department,
    string? Title,
    string? Phone,
    string? Email,
    bool IsActive);
