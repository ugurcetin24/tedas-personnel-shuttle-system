using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class ShuttleRepository(AppDbContext dbContext) : IShuttleRepository
{
    public async Task<PaginatedList<PhysicalShuttle>> SearchAsync(
        int page,
        int pageSize,
        string? code,
        string? plateNumber,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PhysicalShuttles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = code.Trim();
            query = query.Where(shuttle => EF.Functions.Like(shuttle.Code, $"%{normalizedCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(plateNumber))
        {
            var normalizedPlateNumber = plateNumber.Trim();
            query = query.Where(shuttle => EF.Functions.Like(shuttle.PlateNumber, $"%{normalizedPlateNumber}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(shuttle => shuttle.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(shuttle => shuttle.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PaginatedList<PhysicalShuttle>(items, page, pageSize, totalCount);
    }

    public Task<PhysicalShuttle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.PhysicalShuttles.FirstOrDefaultAsync(
            shuttle => shuttle.Id == id,
            cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedShuttleId,
        CancellationToken cancellationToken)
    {
        return dbContext.PhysicalShuttles.AnyAsync(
            shuttle =>
                shuttle.Code == code
                && (!excludedShuttleId.HasValue || shuttle.Id != excludedShuttleId.Value),
            cancellationToken);
    }

    public async Task AddAsync(PhysicalShuttle shuttle, CancellationToken cancellationToken)
    {
        await dbContext.PhysicalShuttles.AddAsync(shuttle, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
