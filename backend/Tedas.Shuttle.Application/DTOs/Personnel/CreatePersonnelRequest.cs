namespace Tedas.Shuttle.Application.DTOs.Personnel;

public sealed record CreatePersonnelRequest(
    string RegistrationNumber,
    string FirstName,
    string LastName,
    string? Department,
    string? Title,
    string? Phone,
    string? Email,
    string? Address,
    decimal? Latitude,
    decimal? Longitude);
