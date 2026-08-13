namespace Tedas.Shuttle.Application.DTOs.Drivers;

public sealed record CreateDriverRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string LicenseNumber);

