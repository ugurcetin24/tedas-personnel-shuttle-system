namespace Tedas.Shuttle.Application.Services;

public sealed record ShuttleQuery(
    int Page = 1,
    int PageSize = 25,
    string? Code = null,
    string? PlateNumber = null,
    bool? IsActive = null);
