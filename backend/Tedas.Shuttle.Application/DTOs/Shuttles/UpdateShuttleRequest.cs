namespace Tedas.Shuttle.Application.DTOs.Shuttles;

public sealed record UpdateShuttleRequest(
    string PlateNumber,
    string? Description);
