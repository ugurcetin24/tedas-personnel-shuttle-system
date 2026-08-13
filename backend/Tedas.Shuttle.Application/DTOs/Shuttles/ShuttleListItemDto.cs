namespace Tedas.Shuttle.Application.DTOs.Shuttles;

public sealed record ShuttleListItemDto(
    Guid Id,
    string Code,
    string PlateNumber,
    string? Description,
    bool IsActive);
