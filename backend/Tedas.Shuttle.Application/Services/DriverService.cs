using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Drivers;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class DriverService(
    IDriverRepository driverRepository,
    IValidator<CreateDriverRequest> createValidator,
    IValidator<UpdateDriverRequest> updateValidator)
    : IDriverService
{
    public async Task<PaginatedList<DriverListItemDto>> SearchAsync(
        DriverQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var drivers = await driverRepository.SearchAsync(
            page,
            pageSize,
            NormalizeOptional(query.Search),
            query.IsActive,
            cancellationToken);

        return new PaginatedList<DriverListItemDto>(
            drivers.Items.Select(MapListItem).ToArray(),
            drivers.Page,
            drivers.PageSize,
            drivers.TotalCount);
    }

    public async Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);

        return driver is null ? null : MapDetails(driver);
    }

    public async Task<DriverDto> CreateAsync(
        CreateDriverRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var licenseNumber = NormalizeRequired(request.LicenseNumber).ToUpperInvariant();
        if (await driverRepository.LicenseNumberExistsAsync(licenseNumber, null, cancellationToken))
        {
            throw new BusinessConflictException(
                "DRIVER_LICENSE_DUPLICATE",
                "Ayni ehliyet numarasi ile ikinci sofor olusturulamaz.");
        }

        var driver = new Driver(
            NormalizeRequired(request.FirstName),
            NormalizeRequired(request.LastName),
            NormalizeOptional(request.Phone),
            licenseNumber,
            DateTimeOffset.UtcNow);

        await driverRepository.AddAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(driver);
    }

    public async Task<DriverDto?> UpdateAsync(
        Guid id,
        UpdateDriverRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        if (driver is null)
        {
            return null;
        }

        var licenseNumber = NormalizeRequired(request.LicenseNumber).ToUpperInvariant();
        if (await driverRepository.LicenseNumberExistsAsync(licenseNumber, id, cancellationToken))
        {
            throw new BusinessConflictException(
                "DRIVER_LICENSE_DUPLICATE",
                "Ayni ehliyet numarasi ile ikinci sofor olusturulamaz.");
        }

        driver.Update(
            NormalizeRequired(request.FirstName),
            NormalizeRequired(request.LastName),
            NormalizeOptional(request.Phone),
            licenseNumber,
            DateTimeOffset.UtcNow);

        await driverRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(driver);
    }

    public async Task<DriverDto?> UpdateStatusAsync(
        Guid id,
        UpdateDriverStatusRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        if (driver is null)
        {
            return null;
        }

        driver.SetActiveStatus(request.IsActive, DateTimeOffset.UtcNow);
        await driverRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(driver);
    }

    public async Task<DriverDto?> UpdateShiftAssignmentAsync(
        Guid id,
        UpdateDriverShiftAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        if (driver is null)
        {
            return null;
        }

        if (!request.ShuttleShiftId.HasValue)
        {
            driver.ClearShiftAssignment(DateTimeOffset.UtcNow);
            await driverRepository.SaveChangesAsync(cancellationToken);
            return MapDetails(driver);
        }

        if (!driver.IsActive)
        {
            throw new BusinessConflictException(
                "DRIVER_INACTIVE",
                "Pasif sofor vardiyaya atanamaz.");
        }

        var shift = await driverRepository.GetShiftByIdAsync(request.ShuttleShiftId.Value, cancellationToken);
        if (shift is null)
        {
            throw new BusinessConflictException(
                "SHIFT_NOT_FOUND",
                "Soforun atanacagi vardiya bulunamadi.");
        }

        if (!shift.IsActive)
        {
            throw new BusinessConflictException(
                "SHIFT_INACTIVE",
                "Pasif vardiyaya sofor atanamaz.");
        }

        if (shift.PhysicalShuttle is { IsActive: false })
        {
            throw new BusinessConflictException(
                "SHUTTLE_INACTIVE",
                "Pasif servisin vardiyasina sofor atanamaz.");
        }

        if (await driverRepository.ShiftHasAssignedDriverAsync(shift.Id, driver.Id, cancellationToken))
        {
            throw new BusinessConflictException(
                "SHIFT_DRIVER_ALREADY_ASSIGNED",
                "Bu vardiyaya zaten bir sofor atanmis.");
        }

        driver.AssignToShift(shift.Id, DateTimeOffset.UtcNow);
        await driverRepository.SaveChangesAsync(cancellationToken);

        driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        return driver is null ? null : MapDetails(driver);
    }

    private static DriverListItemDto MapListItem(Driver driver)
    {
        return new DriverListItemDto(
            driver.Id,
            driver.FirstName,
            driver.LastName,
            $"{driver.FirstName} {driver.LastName}",
            driver.Phone,
            driver.LicenseNumber,
            driver.IsActive,
            MapAssignment(driver.ShuttleShift));
    }

    private static DriverDto MapDetails(Driver driver)
    {
        return new DriverDto(
            driver.Id,
            driver.FirstName,
            driver.LastName,
            $"{driver.FirstName} {driver.LastName}",
            driver.Phone,
            driver.LicenseNumber,
            driver.IsActive,
            MapAssignment(driver.ShuttleShift),
            driver.CreatedAt,
            driver.UpdatedAt);
    }

    private static DriverShiftAssignmentDto? MapAssignment(ShuttleShift? shift)
    {
        if (shift is null)
        {
            return null;
        }

        return new DriverShiftAssignmentDto(
            shift.Id,
            shift.PhysicalShuttleId,
            shift.PhysicalShuttle?.Code ?? string.Empty,
            shift.Name,
            shift.ShiftType);
    }

    private static string NormalizeRequired(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

