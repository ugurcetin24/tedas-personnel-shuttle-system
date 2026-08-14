namespace Tedas.Shuttle.Application.DTOs.Dashboard;

public sealed record DashboardSummaryDto(
    DashboardMetricsDto Metrics,
    IReadOnlyList<ShiftOccupancyDto> ShiftOccupancies);
