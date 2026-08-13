using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Personnel;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class PersonnelService(
    IPersonnelRepository personnelRepository,
    IValidator<CreatePersonnelRequest> createValidator,
    IValidator<UpdatePersonnelRequest> updateValidator)
    : IPersonnelService
{
    public async Task<PaginatedList<PersonnelListItemDto>> SearchAsync(
        PersonnelQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var personnel = await personnelRepository.SearchAsync(
            page,
            pageSize,
            NormalizeOptional(query.Search),
            NormalizeOptional(query.Department),
            query.IsActive,
            cancellationToken);

        return new PaginatedList<PersonnelListItemDto>(
            personnel.Items.Select(MapListItem).ToArray(),
            personnel.Page,
            personnel.PageSize,
            personnel.TotalCount);
    }

    public async Task<PersonnelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var personnel = await personnelRepository.GetByIdAsync(id, cancellationToken);

        return personnel is null ? null : MapDetails(personnel);
    }

    public async Task<PersonnelDto> CreateAsync(
        CreatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var registrationNumber = NormalizeRequired(request.RegistrationNumber);
        var exists = await personnelRepository.RegistrationNumberExistsAsync(
            registrationNumber,
            excludedPersonnelId: null,
            cancellationToken);

        if (exists)
        {
            throw new BusinessConflictException(
                "PERSONNEL_REGISTRATION_NUMBER_DUPLICATE",
                "Aynı sicil numarasıyla ikinci personel oluşturulamaz.");
        }

        var personnel = new Personnel(
            registrationNumber,
            NormalizeRequired(request.FirstName),
            NormalizeRequired(request.LastName),
            NormalizeOptional(request.Department),
            NormalizeOptional(request.Title),
            NormalizeOptional(request.Phone),
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Address),
            request.Latitude,
            request.Longitude,
            DateTimeOffset.UtcNow);

        await personnelRepository.AddAsync(personnel, cancellationToken);
        await personnelRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(personnel);
    }

    public async Task<PersonnelDto?> UpdateAsync(
        Guid id,
        UpdatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var personnel = await personnelRepository.GetByIdAsync(id, cancellationToken);
        if (personnel is null)
        {
            return null;
        }

        personnel.Update(
            NormalizeRequired(request.FirstName),
            NormalizeRequired(request.LastName),
            NormalizeOptional(request.Department),
            NormalizeOptional(request.Title),
            NormalizeOptional(request.Phone),
            NormalizeOptional(request.Email),
            NormalizeOptional(request.Address),
            request.Latitude,
            request.Longitude,
            DateTimeOffset.UtcNow);

        await personnelRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(personnel);
    }

    public async Task<PersonnelDto?> UpdateStatusAsync(
        Guid id,
        UpdatePersonnelStatusRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelRepository.GetByIdAsync(id, cancellationToken);
        if (personnel is null)
        {
            return null;
        }

        personnel.SetActiveStatus(request.IsActive, DateTimeOffset.UtcNow);
        await personnelRepository.SaveChangesAsync(cancellationToken);

        return MapDetails(personnel);
    }

    private static PersonnelListItemDto MapListItem(Personnel personnel)
    {
        return new PersonnelListItemDto(
            personnel.Id,
            personnel.RegistrationNumber,
            personnel.FullName,
            personnel.Department,
            personnel.Title,
            personnel.Phone,
            personnel.Email,
            personnel.IsActive);
    }

    private static PersonnelDto MapDetails(Personnel personnel)
    {
        return new PersonnelDto(
            personnel.Id,
            personnel.RegistrationNumber,
            personnel.FirstName,
            personnel.LastName,
            personnel.FullName,
            personnel.Department,
            personnel.Title,
            personnel.Phone,
            personnel.Email,
            personnel.Address,
            personnel.Latitude,
            personnel.Longitude,
            personnel.IsActive,
            personnel.CreatedAt,
            personnel.UpdatedAt,
            CurrentAssignment: null);
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
