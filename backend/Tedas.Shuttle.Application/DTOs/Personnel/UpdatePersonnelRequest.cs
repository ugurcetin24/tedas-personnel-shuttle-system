namespace Tedas.Shuttle.Application.DTOs.Personnel;

public sealed record UpdatePersonnelRequest(
    string FirstName,
    string LastName,
    string? Department,
    string? Title,
    string? Phone,
    string? Email,
    string? Address,
    decimal? Latitude,
    decimal? Longitude);
