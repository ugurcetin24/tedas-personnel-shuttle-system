namespace Tedas.Shuttle.Application.DTOs.Dashboard;

public sealed record DashboardMetricsDto(
    int TotalPersonnel,
    int ActivePersonnel,
    int TotalShuttles,
    int ActiveShuttles,
    int TotalShifts,
    int ActiveShifts,
    int AssignedPersonnel,
    int UnassignedPersonnel,
    int RoutePointCount,
    int SavedRouteCount);
