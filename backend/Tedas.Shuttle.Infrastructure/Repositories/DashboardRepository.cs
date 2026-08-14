using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.DTOs.Dashboard;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class DashboardRepository(AppDbContext dbContext) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var totalPersonnel = await dbContext.Personnel.AsNoTracking().CountAsync(cancellationToken);
        var activePersonnel = await dbContext.Personnel
            .AsNoTracking()
            .CountAsync(personnel => personnel.IsActive, cancellationToken);
        var assignedPersonnel = await dbContext.PersonnelAssignments
            .AsNoTracking()
            .Where(assignment => assignment.IsActive && assignment.Personnel!.IsActive)
            .Select(assignment => assignment.PersonnelId)
            .Distinct()
            .CountAsync(cancellationToken);
        var unassignedPersonnel = Math.Max(activePersonnel - assignedPersonnel, 0);
        var totalShuttles = await dbContext.PhysicalShuttles.AsNoTracking().CountAsync(cancellationToken);
        var activeShuttles = await dbContext.PhysicalShuttles
            .AsNoTracking()
            .CountAsync(shuttle => shuttle.IsActive, cancellationToken);
        var totalShifts = await dbContext.ShuttleShifts.AsNoTracking().CountAsync(cancellationToken);
        var activeShifts = await dbContext.ShuttleShifts
            .AsNoTracking()
            .CountAsync(shift => shift.IsActive, cancellationToken);
        var routePointCount = await dbContext.RoutePoints.AsNoTracking().CountAsync(cancellationToken);
        var savedRouteCount = await dbContext.SavedRoutes.AsNoTracking().CountAsync(cancellationToken);

        var activeAssignmentCounts = await dbContext.PersonnelAssignments
            .AsNoTracking()
            .Where(assignment => assignment.IsActive)
            .GroupBy(assignment => assignment.ShuttleShiftId)
            .Select(group => new { ShiftId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ShiftId, item => item.Count, cancellationToken);
        var shifts = await dbContext.ShuttleShifts
            .AsNoTracking()
            .Include(shift => shift.PhysicalShuttle)
            .Where(shift => shift.IsActive && shift.PhysicalShuttle!.IsActive)
            .OrderBy(shift => shift.PhysicalShuttle!.Code)
            .ThenBy(shift => shift.StartTime)
            .ThenBy(shift => shift.Name)
            .ToArrayAsync(cancellationToken);
        var shiftOccupancies = shifts
            .Select(shift =>
            {
                var occupancy = activeAssignmentCounts.GetValueOrDefault(shift.Id);
                var utilization = shift.Capacity == 0
                    ? 0m
                    : Math.Round(occupancy * 100m / shift.Capacity, 1);

                return new ShiftOccupancyDto(
                    shift.Id,
                    shift.PhysicalShuttle?.Code ?? string.Empty,
                    shift.Name,
                    shift.Capacity,
                    occupancy,
                    shift.Capacity - occupancy,
                    utilization,
                    shift.IsActive);
            })
            .ToArray();

        return new DashboardSummaryDto(
            new DashboardMetricsDto(
                totalPersonnel,
                activePersonnel,
                totalShuttles,
                activeShuttles,
                totalShifts,
                activeShifts,
                assignedPersonnel,
                unassignedPersonnel,
                routePointCount,
                savedRouteCount),
            shiftOccupancies);
    }
}
