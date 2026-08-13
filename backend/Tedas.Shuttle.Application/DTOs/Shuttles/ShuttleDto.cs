namespace Tedas.Shuttle.Application.DTOs.Shuttles;

public sealed record ShuttleDto(
    Guid Id,
    string Code,
    string PlateNumber,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
