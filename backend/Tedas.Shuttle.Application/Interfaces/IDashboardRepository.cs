using Tedas.Shuttle.Application.DTOs.Dashboard;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
