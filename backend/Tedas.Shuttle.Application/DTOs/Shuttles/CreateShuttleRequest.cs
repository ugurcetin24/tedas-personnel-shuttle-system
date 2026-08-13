namespace Tedas.Shuttle.Application.DTOs.Shuttles;

public sealed record CreateShuttleRequest(
    string Code,
    string PlateNumber,
    string? Description);
