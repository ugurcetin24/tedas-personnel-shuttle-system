using Tedas.Shuttle.Application.DTOs.Dashboard;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Application.Services;

public sealed class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
{
    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        return dashboardRepository.GetSummaryAsync(cancellationToken);
    }
}
