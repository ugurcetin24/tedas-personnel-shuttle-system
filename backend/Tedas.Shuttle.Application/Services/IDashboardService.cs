using Tedas.Shuttle.Application.DTOs.Dashboard;

namespace Tedas.Shuttle.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
