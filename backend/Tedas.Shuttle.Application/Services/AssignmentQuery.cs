namespace Tedas.Shuttle.Application.Services;

public sealed record AssignmentQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    bool? IsActive = true);

