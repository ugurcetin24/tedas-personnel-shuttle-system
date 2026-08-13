using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class ShuttleService(
    IShuttleRepository shuttleRepository,
    IValidator<CreateShuttleRequest> createValidator,
    IValidator<UpdateShuttleRequest> updateValidator)
    : IShuttleService
{
    public async Task<PaginatedList<ShuttleListItemDto>> SearchAsync(
        ShuttleQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var shuttles = await shuttleRepository.SearchAsync(
            page,
            pageSize,
            NormalizeOptional(query.Code),
            NormalizeOptional(query.PlateNumber),
            query.IsActive,
            cancellationToken);

        return new PaginatedList<ShuttleListItemDto>(
            shuttles.Items.Select(MapListItem).ToArray(),
            shuttles.Page,
            shuttles.PageSize,
            shuttles.TotalCount);
    }

    public async Task<ShuttleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var shuttle = await shuttleRepository.GetByIdAsync(id, cancellationToken);

        return shuttle is null ? null : MapDetails(shuttle);
    }

    public async Task<ShuttleDto> CreateAsync(
        CreateShuttleRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var code = NormalizeRequired(request.Code).ToUpperInvariant();
        var exists = await shuttleRepository.CodeExistsAsync(
            code,
            excludedShuttleId: null,
            cancellationToken);

        if (exists)
        {
            throw new BusinessConflictException(
                "SHUTTLE_CODE_DUPLICATE",
                "Aynı kod ile ikinci servis oluşturulamaz.");
        }

        var shuttle = new PhysicalShuttle(
            code,
            NormalizeRequired(request.PlateNumber).ToUpperInvariant(),
            NormalizeOptional(request.Description),
            DateTimeOffset.UtcNow);

        await shuttleRepository.AddAsync(shuttle, cancellationToken);
        await shuttleRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(shuttle);
    }

    public async Task<ShuttleDto?> UpdateAsync(
        Guid id,
        UpdateShuttleRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var shuttle = await shuttleRepository.GetByIdAsync(id, cancellationToken);
        if (shuttle is null)
        {
            return null;
        }

        shuttle.Update(
            NormalizeRequired(request.PlateNumber).ToUpperInvariant(),
            NormalizeOptional(request.Description),
            DateTimeOffset.UtcNow);

        await shuttleRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(shuttle);
    }

    public async Task<ShuttleDto?> UpdateStatusAsync(
        Guid id,
        UpdateShuttleStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shuttle = await shuttleRepository.GetByIdAsync(id, cancellationToken);
        if (shuttle is null)
        {
            return null;
        }

        shuttle.SetActiveStatus(request.IsActive, DateTimeOffset.UtcNow);
        await shuttleRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(shuttle);
    }

    private static ShuttleListItemDto MapListItem(PhysicalShuttle shuttle)
    {
        return new ShuttleListItemDto(
            shuttle.Id,
            shuttle.Code,
            shuttle.PlateNumber,
            shuttle.Description,
            shuttle.IsActive);
    }

    private static ShuttleDto MapDetails(PhysicalShuttle shuttle)
    {
        return new ShuttleDto(
            shuttle.Id,
            shuttle.Code,
            shuttle.PlateNumber,
            shuttle.Description,
            shuttle.IsActive,
            shuttle.CreatedAt,
            shuttle.UpdatedAt);
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
