using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class PersonnelRepository(AppDbContext dbContext) : IPersonnelRepository
{
    public async Task<PaginatedList<Personnel>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        string? department,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Personnel.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(personnel =>
                EF.Functions.Like(personnel.RegistrationNumber, $"%{normalizedSearch}%")
                || EF.Functions.Like(personnel.FirstName, $"%{normalizedSearch}%")
                || EF.Functions.Like(personnel.LastName, $"%{normalizedSearch}%"));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var normalizedDepartment = department.Trim();
            query = query.Where(personnel => personnel.Department == normalizedDepartment);
        }

        if (isActive.HasValue)
        {
            query = query.Where(personnel => personnel.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(personnel => personnel.LastName)
            .ThenBy(personnel => personnel.FirstName)
            .ThenBy(personnel => personnel.RegistrationNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PaginatedList<Personnel>(items, page, pageSize, totalCount);
    }

    public Task<Personnel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Personnel.FirstOrDefaultAsync(
            personnel => personnel.Id == id,
            cancellationToken);
    }

    public Task<Personnel?> GetByRegistrationNumberAsync(
        string registrationNumber,
        CancellationToken cancellationToken)
    {
        return dbContext.Personnel.FirstOrDefaultAsync(
            personnel => personnel.RegistrationNumber == registrationNumber,
            cancellationToken);
    }

    public Task<bool> RegistrationNumberExistsAsync(
        string registrationNumber,
        Guid? excludedPersonnelId,
        CancellationToken cancellationToken)
    {
        return dbContext.Personnel.AnyAsync(
            personnel =>
                personnel.RegistrationNumber == registrationNumber
                && (!excludedPersonnelId.HasValue || personnel.Id != excludedPersonnelId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Personnel personnel, CancellationToken cancellationToken)
    {
        await dbContext.Personnel.AddAsync(personnel, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
