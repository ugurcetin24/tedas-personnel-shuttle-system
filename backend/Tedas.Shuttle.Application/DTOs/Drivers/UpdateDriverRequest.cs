namespace Tedas.Shuttle.Application.DTOs.Drivers;

public sealed record UpdateDriverRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string LicenseNumber);

