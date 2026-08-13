using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class AssignmentRepository(AppDbContext dbContext) : IAssignmentRepository
{
    public async Task<PaginatedList<PersonnelAssignment>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PersonnelAssignments
            .AsNoTracking()
            .Include(assignment => assignment.Personnel)
            .Include(assignment => assignment.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(assignment =>
                EF.Functions.Like(assignment.Personnel!.RegistrationNumber, $"%{normalizedSearch}%")
                || EF.Functions.Like(assignment.Personnel.FirstName, $"%{normalizedSearch}%")
                || EF.Functions.Like(assignment.Personnel.LastName, $"%{normalizedSearch}%")
                || EF.Functions.Like(assignment.ShuttleShift!.Name, $"%{normalizedSearch}%")
                || EF.Functions.Like(assignment.ShuttleShift.PhysicalShuttle!.Code, $"%{normalizedSearch}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(assignment => assignment.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(assignment => assignment.AssignedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PaginatedList<PersonnelAssignment>(items, page, pageSize, totalCount);
    }

    public Task<PersonnelAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.PersonnelAssignments
            .Include(assignment => assignment.Personnel)
            .Include(assignment => assignment.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .FirstOrDefaultAsync(assignment => assignment.Id == id, cancellationToken);
    }

    public Task<Personnel?> GetPersonnelByIdAsync(Guid personnelId, CancellationToken cancellationToken)
    {
        return dbContext.Personnel.FirstOrDefaultAsync(
            personnel => personnel.Id == personnelId,
            cancellationToken);
    }

    public Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public Task<bool> PersonnelHasActiveAssignmentAsync(Guid personnelId, CancellationToken cancellationToken)
    {
        return dbContext.PersonnelAssignments.AnyAsync(
            assignment => assignment.PersonnelId == personnelId && assignment.IsActive,
            cancellationToken);
    }

    public Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.PersonnelAssignments.CountAsync(
            assignment => assignment.ShuttleShiftId == shiftId && assignment.IsActive,
            cancellationToken);
    }

    public async Task AddAsync(PersonnelAssignment assignment, CancellationToken cancellationToken)
    {
        await dbContext.PersonnelAssignments.AddAsync(assignment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

