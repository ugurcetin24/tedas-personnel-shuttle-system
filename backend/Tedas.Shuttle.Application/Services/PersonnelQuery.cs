namespace Tedas.Shuttle.Application.Services;

public sealed record PersonnelQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Department = null,
    bool? IsActive = null);
