using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Imports;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Application.Services;

public sealed class ExcelImportPreviewService(
    IExcelWorkbookReader workbookReader,
    IPersonnelRepository personnelRepository,
    IShiftRepository shiftRepository,
    IRoutePointRepository routePointRepository)
    : IExcelImportPreviewService
{
    private static readonly IReadOnlySet<string> RequiredPersonnelFields =
        new HashSet<string>(["RegistrationNumber", "FirstName", "LastName"], StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> RequiredCapacityFields =
        new HashSet<string>(["PhysicalShuttleCode", "ShiftName", "Capacity"], StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> RequiredRouteFields =
        new HashSet<string>(
            ["PhysicalShuttleCode", "ShiftName", "Order", "Name", "Latitude", "Longitude"],
            StringComparer.Ordinal);

    public async Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        return await PreviewPersonnelAsync(
            stream,
            fileName,
            sheetName,
            new PersonnelImportPreviewOptions(IncludeConflictDetection: false),
            cancellationToken);
    }

    public async Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        PersonnelImportPreviewOptions options,
        CancellationToken cancellationToken)
    {
        var workbook = await workbookReader.ReadAsync(stream, fileName, cancellationToken);
        var sheet = SelectSheet(workbook, sheetName);
        var suggestions = ExcelColumnMappingSuggester.Suggest(sheet.Headers, ExcelImportProfiles.Personnel);
        var mapping = suggestions
            .GroupBy(suggestion => suggestion.TargetField)
            .ToDictionary(group => group.Key, group => group.First().SourceHeader, StringComparer.Ordinal);
        var missingFields = RequiredPersonnelFields
            .Where(field => !mapping.ContainsKey(field))
            .ToArray();

        var rows = sheet.Rows
            .Select(row => MapPersonnelRow(row, mapping, missingFields))
            .ToArray();

        if (options.IncludeConflictDetection)
        {
            rows = await AddPersonnelConflictDetectionAsync(rows, cancellationToken);
        }

        return new ExcelImportPreviewDto(
            workbook.FileName,
            sheet.Name,
            sheet.Headers,
            suggestions,
            rows);
    }

    public async Task<PersonnelImportCommitResultDto> CommitPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewPersonnelAsync(
            stream,
            fileName,
            sheetName,
            new PersonnelImportPreviewOptions(IncludeConflictDetection: true),
            cancellationToken);
        var errorRows = preview.Rows
            .Where(row => row.Status == "Error")
            .ToArray();

        if (errorRows.Length > 0)
        {
            throw new BusinessConflictException(
                "PERSONNEL_IMPORT_PREVIEW_HAS_ERRORS",
                "Personel import commit icin onizlemede hata bulunmamali.");
        }

        var importRows = preview.Rows
            .Where(row => row.Action is "Create" or "Update")
            .ToArray();
        var registrationNumbers = importRows
            .Select(row => row.NormalizedData["RegistrationNumber"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var existingPersonnel = await personnelRepository.GetByRegistrationNumbersAsync(
            registrationNumbers,
            cancellationToken);
        var createdCount = 0;
        var updatedCount = 0;

        await personnelRepository.ExecuteInTransactionAsync(
            async token =>
            {
                var newPersonnel = new List<Personnel>();

                foreach (var row in importRows)
                {
                    var registrationNumber = row.NormalizedData["RegistrationNumber"]!;
                    if (existingPersonnel.TryGetValue(registrationNumber, out var personnel))
                    {
                        UpdatePersonnel(personnel, row);
                        updatedCount++;
                        continue;
                    }

                    newPersonnel.Add(CreatePersonnel(row));
                    createdCount++;
                }

                if (newPersonnel.Count > 0)
                {
                    await personnelRepository.AddRangeAsync(newPersonnel, token);
                }

                await personnelRepository.SaveChangesAsync(token);
            },
            cancellationToken);

        var skippedCount = preview.Rows.Count - createdCount - updatedCount;

        return new PersonnelImportCommitResultDto(createdCount, updatedCount, skippedCount, preview.Rows);
    }

    public async Task<ExcelImportPreviewDto> PreviewCapacityAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        var workbook = await workbookReader.ReadAsync(stream, fileName, cancellationToken);
        var sheet = SelectSheet(workbook, sheetName);
        var suggestions = ExcelColumnMappingSuggester.Suggest(sheet.Headers, ExcelImportProfiles.ShuttleCapacity);
        var mapping = suggestions
            .GroupBy(suggestion => suggestion.TargetField)
            .ToDictionary(group => group.Key, group => group.First().SourceHeader, StringComparer.Ordinal);
        var missingFields = RequiredCapacityFields
            .Where(field => !mapping.ContainsKey(field))
            .ToArray();
        var rows = sheet.Rows
            .Select(row => MapCapacityRow(row, mapping, missingFields))
            .ToArray();
        rows = await AddCapacityConflictDetectionAsync(rows, cancellationToken);

        return new ExcelImportPreviewDto(
            workbook.FileName,
            sheet.Name,
            sheet.Headers,
            suggestions,
            rows);
    }

    public async Task<CapacityImportCommitResultDto> CommitCapacityAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewCapacityAsync(stream, fileName, sheetName, cancellationToken);
        if (preview.Rows.Any(row => row.Status == "Error"))
        {
            throw new BusinessConflictException(
                "CAPACITY_IMPORT_PREVIEW_HAS_ERRORS",
                "Kapasite import commit icin onizlemede hata bulunmamali.");
        }

        var importRows = preview.Rows
            .Where(row => row.Action is "Create" or "Update")
            .ToArray();
        var shuttleCodes = importRows
            .Select(row => row.NormalizedData["PhysicalShuttleCode"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var existingShifts = await shiftRepository.ListByShuttleCodesAsync(shuttleCodes, cancellationToken);
        var shiftsByKey = existingShifts.ToDictionary(
            shift => CapacityKey(shift.PhysicalShuttle?.Code ?? string.Empty, shift.Name),
            StringComparer.OrdinalIgnoreCase);
        var shuttlesByCode = await shiftRepository.GetShuttlesByCodesAsync(shuttleCodes, cancellationToken);
        var createdCount = 0;
        var updatedCount = 0;

        await shiftRepository.ExecuteInTransactionAsync(
            async token =>
            {
                var newShifts = new List<ShuttleShift>();

                foreach (var row in importRows)
                {
                    var shuttleCode = row.NormalizedData["PhysicalShuttleCode"]!;
                    var shiftName = row.NormalizedData["ShiftName"]!;
                    var capacity = int.Parse(row.NormalizedData["Capacity"]!, System.Globalization.CultureInfo.InvariantCulture);
                    var key = CapacityKey(shuttleCode, shiftName);

                    if (shiftsByKey.TryGetValue(key, out var existingShift))
                    {
                        existingShift.Update(
                            existingShift.Name,
                            existingShift.ShiftType,
                            capacity,
                            existingShift.StartTime,
                            existingShift.EndTime,
                            DateTimeOffset.UtcNow);
                        updatedCount++;
                        continue;
                    }

                    var shuttle = shuttlesByCode[shuttleCode];
                    newShifts.Add(new ShuttleShift(
                        shuttle.Id,
                        shiftName,
                        ParseShiftType(row.NormalizedData["ShiftType"])!.Value,
                        capacity,
                        ParseTime(row.NormalizedData["StartTime"])!.Value,
                        ParseTime(row.NormalizedData["EndTime"])!.Value,
                        DateTimeOffset.UtcNow));
                    createdCount++;
                }

                if (newShifts.Count > 0)
                {
                    await shiftRepository.AddRangeAsync(newShifts, token);
                }

                await shiftRepository.SaveChangesAsync(token);
            },
            cancellationToken);

        var skippedCount = preview.Rows.Count - createdCount - updatedCount;

        return new CapacityImportCommitResultDto(createdCount, updatedCount, skippedCount, preview.Rows);
    }

    public async Task<ExcelImportPreviewDto> PreviewRouteAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        var workbook = await workbookReader.ReadAsync(stream, fileName, cancellationToken);
        var sheet = SelectSheet(workbook, sheetName);
        var suggestions = ExcelColumnMappingSuggester.Suggest(sheet.Headers, ExcelImportProfiles.Route);
        var mapping = suggestions
            .GroupBy(suggestion => suggestion.TargetField)
            .ToDictionary(group => group.Key, group => group.First().SourceHeader, StringComparer.Ordinal);
        var missingFields = RequiredRouteFields
            .Where(field => !mapping.ContainsKey(field))
            .ToArray();
        var rows = sheet.Rows
            .Select(row => MapRouteRow(row, mapping, missingFields))
            .ToArray();
        rows = await AddRouteConflictDetectionAsync(rows, cancellationToken);

        return new ExcelImportPreviewDto(
            workbook.FileName,
            sheet.Name,
            sheet.Headers,
            suggestions,
            rows);
    }

    public async Task<RouteImportCommitResultDto> CommitRouteAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewRouteAsync(stream, fileName, sheetName, cancellationToken);
        if (preview.Rows.Any(row => row.Status == "Error"))
        {
            throw new BusinessConflictException(
                "ROUTE_IMPORT_PREVIEW_HAS_ERRORS",
                "Guzergah import commit icin onizlemede hata bulunmamali.");
        }

        var importRows = preview.Rows
            .Where(row => row.Action is "Create" or "Update")
            .ToArray();
        var shuttleCodes = importRows
            .Select(row => row.NormalizedData["PhysicalShuttleCode"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var shifts = await shiftRepository.ListByShuttleCodesAsync(shuttleCodes, cancellationToken);
        var shiftsByKey = shifts.ToDictionary(
            shift => ShiftKey(shift.PhysicalShuttle?.Code ?? string.Empty, shift.Name),
            StringComparer.OrdinalIgnoreCase);
        var routePoints = await routePointRepository.ListByShiftIdsAsync(
            shifts.Select(shift => shift.Id).ToArray(),
            cancellationToken);
        var routePointsByKey = routePoints.ToDictionary(
            routePoint => RouteKey(
                routePoint.ShuttleShift?.PhysicalShuttle?.Code ?? string.Empty,
                routePoint.ShuttleShift?.Name ?? string.Empty,
                routePoint.Order),
            StringComparer.OrdinalIgnoreCase);
        var createdCount = 0;
        var updatedCount = 0;

        await routePointRepository.ExecuteInTransactionAsync(
            async token =>
            {
                var newRoutePoints = new List<RoutePoint>();

                foreach (var row in importRows)
                {
                    var shuttleCode = row.NormalizedData["PhysicalShuttleCode"]!;
                    var shiftName = row.NormalizedData["ShiftName"]!;
                    var order = int.Parse(row.NormalizedData["Order"]!, System.Globalization.CultureInfo.InvariantCulture);
                    var routeKey = RouteKey(shuttleCode, shiftName, order);

                    if (routePointsByKey.TryGetValue(routeKey, out var routePoint))
                    {
                        UpdateRoutePoint(routePoint, row);
                        updatedCount++;
                        continue;
                    }

                    var shift = shiftsByKey[ShiftKey(shuttleCode, shiftName)];
                    newRoutePoints.Add(CreateRoutePoint(shift.Id, row));
                    createdCount++;
                }

                if (newRoutePoints.Count > 0)
                {
                    await routePointRepository.AddRangeAsync(newRoutePoints, token);
                }

                await routePointRepository.SaveChangesAsync(token);
            },
            cancellationToken);

        var skippedCount = preview.Rows.Count - createdCount - updatedCount;

        return new RouteImportCommitResultDto(createdCount, updatedCount, skippedCount, preview.Rows);
    }

    private static ExcelSheetDto SelectSheet(ExcelWorkbookDto workbook, string? sheetName)
    {
        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            return workbook.Sheets.FirstOrDefault(sheet =>
                    string.Equals(sheet.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                ?? throw new BusinessConflictException(
                    "EXCEL_SHEET_NOT_FOUND",
                    "Excel sayfasi bulunamadi.");
        }

        return workbook.Sheets.FirstOrDefault(sheet => !sheet.IsEmpty)
            ?? throw new BusinessConflictException(
                "EXCEL_WORKBOOK_EMPTY",
                "Excel dosyasinda dolu sayfa bulunamadi.");
    }

    private static ExcelPreviewRowDto MapPersonnelRow(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        IReadOnlyList<string> missingFields)
    {
        var normalizedData = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["RegistrationNumber"] = NormalizeMappedValue(row, mapping, "RegistrationNumber", ExcelValueNormalizer.NormalizeRegistrationNumber),
            ["FirstName"] = NormalizeMappedValue(row, mapping, "FirstName", ExcelValueNormalizer.NullIfWhiteSpace),
            ["LastName"] = NormalizeMappedValue(row, mapping, "LastName", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Department"] = NormalizeMappedValue(row, mapping, "Department", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Title"] = NormalizeMappedValue(row, mapping, "Title", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Phone"] = NormalizeMappedValue(row, mapping, "Phone", ExcelValueNormalizer.NormalizePhone),
            ["Email"] = NormalizeMappedValue(row, mapping, "Email", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Address"] = NormalizeMappedValue(row, mapping, "Address", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Latitude"] = NormalizeCoordinate(row, mapping, "Latitude"),
            ["Longitude"] = NormalizeCoordinate(row, mapping, "Longitude")
        };

        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var missingField in missingFields)
        {
            errors.Add($"{missingField} sutunu eslestirilemedi.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["RegistrationNumber"]))
        {
            errors.Add("Sicil numarasi bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["FirstName"]))
        {
            errors.Add("Ad bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["LastName"]))
        {
            errors.Add("Soyad bos birakilamaz.");
        }

        AddCoordinateWarningIfInvalid(row, mapping, "Latitude", "Enlem", warnings);
        AddCoordinateWarningIfInvalid(row, mapping, "Longitude", "Boylam", warnings);

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : "Valid";
        var action = errors.Count > 0 ? "Skip" : "Create";

        return new ExcelPreviewRowDto(row.RowNumber, status, action, normalizedData, errors, warnings);
    }

    private static ExcelPreviewRowDto MapCapacityRow(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        IReadOnlyList<string> missingFields)
    {
        var normalizedData = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["PhysicalShuttleCode"] = NormalizeMappedValue(row, mapping, "PhysicalShuttleCode", ExcelValueNormalizer.NormalizeCode),
            ["ShiftName"] = NormalizeMappedValue(row, mapping, "ShiftName", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Capacity"] = NormalizeInteger(row, mapping, "Capacity"),
            ["ShiftType"] = NormalizeMappedValue(row, mapping, "ShiftType", ExcelValueNormalizer.NullIfWhiteSpace),
            ["StartTime"] = NormalizeTime(row, mapping, "StartTime"),
            ["EndTime"] = NormalizeTime(row, mapping, "EndTime"),
            ["CurrentCapacity"] = null,
            ["Occupancy"] = null
        };

        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var missingField in missingFields)
        {
            errors.Add($"{missingField} sutunu eslestirilemedi.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["PhysicalShuttleCode"]))
        {
            errors.Add("Servis kodu bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["ShiftName"]))
        {
            errors.Add("Vardiya adi bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["Capacity"]))
        {
            errors.Add("Kapasite bos veya gecersiz olamaz.");
        }
        else if (int.Parse(normalizedData["Capacity"]!, System.Globalization.CultureInfo.InvariantCulture) <= 0)
        {
            errors.Add("Kapasite pozitif olmalidir.");
        }

        AddIntegerWarningIfInvalid(row, mapping, "Capacity", "Kapasite", warnings);

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : "Valid";
        var action = errors.Count > 0 ? "Skip" : "Create";

        return new ExcelPreviewRowDto(row.RowNumber, status, action, normalizedData, errors, warnings);
    }

    private static ExcelPreviewRowDto MapRouteRow(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        IReadOnlyList<string> missingFields)
    {
        var normalizedData = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["PhysicalShuttleCode"] = NormalizeMappedValue(row, mapping, "PhysicalShuttleCode", ExcelValueNormalizer.NormalizeCode),
            ["ShiftName"] = NormalizeMappedValue(row, mapping, "ShiftName", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Order"] = NormalizeInteger(row, mapping, "Order"),
            ["Name"] = NormalizeMappedValue(row, mapping, "Name", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Address"] = NormalizeMappedValue(row, mapping, "Address", ExcelValueNormalizer.NullIfWhiteSpace),
            ["Latitude"] = NormalizeCoordinate(row, mapping, "Latitude"),
            ["Longitude"] = NormalizeCoordinate(row, mapping, "Longitude"),
            ["IsActive"] = NormalizeBoolean(row, mapping, "IsActive"),
            ["CurrentName"] = null
        };

        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var missingField in missingFields)
        {
            errors.Add($"{missingField} sutunu eslestirilemedi.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["PhysicalShuttleCode"]))
        {
            errors.Add("Servis kodu bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["ShiftName"]))
        {
            errors.Add("Vardiya adi bos birakilamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["Order"]))
        {
            errors.Add("Sira bos veya gecersiz olamaz.");
        }
        else if (int.Parse(normalizedData["Order"]!, System.Globalization.CultureInfo.InvariantCulture) <= 0)
        {
            errors.Add("Sira pozitif olmalidir.");
        }

        if (string.IsNullOrWhiteSpace(normalizedData["Name"]))
        {
            errors.Add("Durak adi bos birakilamaz.");
        }

        AddRequiredCoordinateErrorIfInvalid(row, mapping, "Latitude", "Enlem", errors);
        AddRequiredCoordinateErrorIfInvalid(row, mapping, "Longitude", "Boylam", errors);
        AddBooleanWarningIfInvalid(row, mapping, "IsActive", "Aktif", warnings);

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : "Valid";
        var action = errors.Count > 0 ? "Skip" : "Create";

        return new ExcelPreviewRowDto(row.RowNumber, status, action, normalizedData, errors, warnings);
    }

    private async Task<ExcelPreviewRowDto[]> AddPersonnelConflictDetectionAsync(
        IReadOnlyList<ExcelPreviewRowDto> rows,
        CancellationToken cancellationToken)
    {
        var duplicateRegistrationNumbers = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.NormalizedData["RegistrationNumber"]))
            .GroupBy(row => row.NormalizedData["RegistrationNumber"]!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var registrationNumbers = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.NormalizedData["RegistrationNumber"]))
            .Select(row => row.NormalizedData["RegistrationNumber"]!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var existingPersonnel = await personnelRepository.GetByRegistrationNumbersAsync(
            registrationNumbers,
            cancellationToken);

        return rows
            .Select(row => AddPersonnelConflictDetection(row, duplicateRegistrationNumbers, existingPersonnel))
            .ToArray();
    }

    private async Task<ExcelPreviewRowDto[]> AddCapacityConflictDetectionAsync(
        IReadOnlyList<ExcelPreviewRowDto> rows,
        CancellationToken cancellationToken)
    {
        var duplicateKeys = rows
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.NormalizedData["PhysicalShuttleCode"])
                && !string.IsNullOrWhiteSpace(row.NormalizedData["ShiftName"]))
            .GroupBy(row => CapacityKey(
                row.NormalizedData["PhysicalShuttleCode"]!,
                row.NormalizedData["ShiftName"]!),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var shuttleCodes = rows
            .Select(row => row.NormalizedData["PhysicalShuttleCode"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var existingShifts = await shiftRepository.ListByShuttleCodesAsync(shuttleCodes, cancellationToken);
        var shiftsByKey = existingShifts.ToDictionary(
            shift => CapacityKey(shift.PhysicalShuttle?.Code ?? string.Empty, shift.Name),
            StringComparer.OrdinalIgnoreCase);
        var shuttlesByCode = await shiftRepository.GetShuttlesByCodesAsync(shuttleCodes, cancellationToken);
        var occupancyByShiftId = await shiftRepository.GetActiveAssignmentCountsAsync(
            existingShifts.Select(shift => shift.Id).ToArray(),
            cancellationToken);

        return rows
            .Select(row => AddCapacityConflictDetection(row, duplicateKeys, shiftsByKey, shuttlesByCode, occupancyByShiftId))
            .ToArray();
    }

    private async Task<ExcelPreviewRowDto[]> AddRouteConflictDetectionAsync(
        IReadOnlyList<ExcelPreviewRowDto> rows,
        CancellationToken cancellationToken)
    {
        var duplicateKeys = rows
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.NormalizedData["PhysicalShuttleCode"])
                && !string.IsNullOrWhiteSpace(row.NormalizedData["ShiftName"])
                && !string.IsNullOrWhiteSpace(row.NormalizedData["Order"]))
            .GroupBy(row => RouteKey(
                row.NormalizedData["PhysicalShuttleCode"]!,
                row.NormalizedData["ShiftName"]!,
                int.Parse(row.NormalizedData["Order"]!, System.Globalization.CultureInfo.InvariantCulture)),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var shuttleCodes = rows
            .Select(row => row.NormalizedData["PhysicalShuttleCode"])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var shifts = await shiftRepository.ListByShuttleCodesAsync(shuttleCodes, cancellationToken);
        var shiftsByKey = shifts.ToDictionary(
            shift => ShiftKey(shift.PhysicalShuttle?.Code ?? string.Empty, shift.Name),
            StringComparer.OrdinalIgnoreCase);
        var routePoints = await routePointRepository.ListByShiftIdsAsync(
            shifts.Select(shift => shift.Id).ToArray(),
            cancellationToken);
        var routePointsByKey = routePoints.ToDictionary(
            routePoint => RouteKey(
                routePoint.ShuttleShift?.PhysicalShuttle?.Code ?? string.Empty,
                routePoint.ShuttleShift?.Name ?? string.Empty,
                routePoint.Order),
            StringComparer.OrdinalIgnoreCase);

        return rows
            .Select(row => AddRouteConflictDetection(row, duplicateKeys, shiftsByKey, routePointsByKey))
            .ToArray();
    }

    private static ExcelPreviewRowDto AddPersonnelConflictDetection(
        ExcelPreviewRowDto row,
        IReadOnlySet<string> duplicateRegistrationNumbers,
        IReadOnlyDictionary<string, Personnel> existingPersonnel)
    {
        var errors = row.Errors.ToList();
        var warnings = row.Warnings.ToList();
        var action = row.Action;
        var registrationNumber = row.NormalizedData["RegistrationNumber"];

        if (!string.IsNullOrWhiteSpace(registrationNumber)
            && duplicateRegistrationNumbers.Contains(registrationNumber))
        {
            errors.Add("Excel dosyasinda ayni sicil numarasi birden fazla satirda bulunuyor.");
            action = "Conflict";
        }

        if (errors.Count == 0
            && !string.IsNullOrWhiteSpace(registrationNumber)
            && existingPersonnel.TryGetValue(registrationNumber, out var personnel))
        {
            action = HasPersonnelChanges(personnel, row) ? "Update" : "NoChange";
            if (action == "Update")
            {
                warnings.Add("Bu sicil numarasi sistemde mevcut, kayit guncelleme adayi.");
            }
            else
            {
                warnings.Add("Bu sicil numarasi sistemde mevcut, degisiklik yok.");
            }
        }

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : row.Status;

        return row with
        {
            Status = status,
            Action = action,
            Errors = errors,
            Warnings = warnings
        };
    }

    private static ExcelPreviewRowDto AddCapacityConflictDetection(
        ExcelPreviewRowDto row,
        IReadOnlySet<string> duplicateKeys,
        IReadOnlyDictionary<string, ShuttleShift> shiftsByKey,
        IReadOnlyDictionary<string, PhysicalShuttle> shuttlesByCode,
        IReadOnlyDictionary<Guid, int> occupancyByShiftId)
    {
        var errors = row.Errors.ToList();
        var warnings = row.Warnings.ToList();
        var normalizedData = row.NormalizedData.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        var shuttleCode = row.NormalizedData["PhysicalShuttleCode"];
        var shiftName = row.NormalizedData["ShiftName"];
        var action = row.Action;

        if (!string.IsNullOrWhiteSpace(shuttleCode)
            && !string.IsNullOrWhiteSpace(shiftName)
            && duplicateKeys.Contains(CapacityKey(shuttleCode, shiftName)))
        {
            errors.Add("Excel dosyasinda ayni servis/vardiya birden fazla satirda bulunuyor.");
            action = "Conflict";
        }

        if (errors.Count == 0 && !string.IsNullOrWhiteSpace(shuttleCode) && !shuttlesByCode.ContainsKey(shuttleCode))
        {
            errors.Add("Servis kodu sistemde bulunamadi.");
            action = "Conflict";
        }

        if (errors.Count == 0
            && !string.IsNullOrWhiteSpace(shuttleCode)
            && !string.IsNullOrWhiteSpace(shiftName)
            && shiftsByKey.TryGetValue(CapacityKey(shuttleCode, shiftName), out var shift))
        {
            var occupancy = occupancyByShiftId.TryGetValue(shift.Id, out var value) ? value : 0;
            var importedCapacity = int.Parse(row.NormalizedData["Capacity"]!, System.Globalization.CultureInfo.InvariantCulture);
            normalizedData["CurrentCapacity"] = shift.Capacity.ToString(System.Globalization.CultureInfo.InvariantCulture);
            normalizedData["Occupancy"] = occupancy.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (importedCapacity < occupancy)
            {
                errors.Add("Yeni kapasite mevcut aktif atama sayisindan dusuk olamaz.");
                action = "Conflict";
            }
            else if (importedCapacity == shift.Capacity)
            {
                warnings.Add("Servis vardiyasi sistemde mevcut, kapasite degisikligi yok.");
                action = "NoChange";
            }
            else
            {
                warnings.Add("Servis vardiyasi sistemde mevcut, kapasite guncelleme adayi.");
                action = "Update";
            }
        }

        if (errors.Count == 0 && action == "Create")
        {
            if (ParseShiftType(row.NormalizedData["ShiftType"]) is null)
            {
                errors.Add("Yeni vardiya olusturmak icin vardiya tipi gereklidir.");
                action = "Conflict";
            }

            if (ParseTime(row.NormalizedData["StartTime"]) is null)
            {
                errors.Add("Yeni vardiya olusturmak icin baslangic saati gereklidir.");
                action = "Conflict";
            }

            if (ParseTime(row.NormalizedData["EndTime"]) is null)
            {
                errors.Add("Yeni vardiya olusturmak icin bitis saati gereklidir.");
                action = "Conflict";
            }
        }

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : row.Status;

        return row with
        {
            Status = status,
            Action = action,
            NormalizedData = normalizedData,
            Errors = errors,
            Warnings = warnings
        };
    }

    private static ExcelPreviewRowDto AddRouteConflictDetection(
        ExcelPreviewRowDto row,
        IReadOnlySet<string> duplicateKeys,
        IReadOnlyDictionary<string, ShuttleShift> shiftsByKey,
        IReadOnlyDictionary<string, RoutePoint> routePointsByKey)
    {
        var errors = row.Errors.ToList();
        var warnings = row.Warnings.ToList();
        var normalizedData = row.NormalizedData.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        var shuttleCode = row.NormalizedData["PhysicalShuttleCode"];
        var shiftName = row.NormalizedData["ShiftName"];
        var orderText = row.NormalizedData["Order"];
        var action = row.Action;

        if (!string.IsNullOrWhiteSpace(shuttleCode)
            && !string.IsNullOrWhiteSpace(shiftName)
            && !string.IsNullOrWhiteSpace(orderText))
        {
            var order = int.Parse(orderText, System.Globalization.CultureInfo.InvariantCulture);
            var routeKey = RouteKey(shuttleCode, shiftName, order);

            if (duplicateKeys.Contains(routeKey))
            {
                errors.Add("Excel dosyasinda ayni servis/vardiya/sira birden fazla satirda bulunuyor.");
                action = "Conflict";
            }

            if (errors.Count == 0 && !shiftsByKey.ContainsKey(ShiftKey(shuttleCode, shiftName)))
            {
                errors.Add("Servis vardiyasi sistemde bulunamadi.");
                action = "Conflict";
            }

            if (errors.Count == 0 && routePointsByKey.TryGetValue(routeKey, out var routePoint))
            {
                normalizedData["CurrentName"] = routePoint.Name;
                if (HasRoutePointChanges(routePoint, row))
                {
                    warnings.Add("Guzergah noktasi sistemde mevcut, guncelleme adayi.");
                    action = "Update";
                }
                else
                {
                    warnings.Add("Guzergah noktasi sistemde mevcut, degisiklik yok.");
                    action = "NoChange";
                }
            }
        }

        var status = errors.Count > 0 ? "Error" : warnings.Count > 0 ? "Warning" : row.Status;

        return row with
        {
            Status = status,
            Action = action,
            NormalizedData = normalizedData,
            Errors = errors,
            Warnings = warnings
        };
    }

    private static bool HasPersonnelChanges(Personnel personnel, ExcelPreviewRowDto row)
    {
        return personnel.FirstName != row.NormalizedData["FirstName"]
            || personnel.LastName != row.NormalizedData["LastName"]
            || personnel.Department != row.NormalizedData["Department"]
            || personnel.Title != row.NormalizedData["Title"]
            || personnel.Phone != row.NormalizedData["Phone"]
            || personnel.Email != row.NormalizedData["Email"]
            || personnel.Address != row.NormalizedData["Address"]
            || personnel.Latitude != ParseNullableDecimal(row.NormalizedData["Latitude"])
            || personnel.Longitude != ParseNullableDecimal(row.NormalizedData["Longitude"]);
    }

    private static bool HasRoutePointChanges(RoutePoint routePoint, ExcelPreviewRowDto row)
    {
        var importedIsActive = ParseNullableBoolean(row.NormalizedData["IsActive"]);

        return routePoint.Name != row.NormalizedData["Name"]
            || routePoint.Address != row.NormalizedData["Address"]
            || routePoint.Latitude != ParseNullableDecimal(row.NormalizedData["Latitude"])
            || routePoint.Longitude != ParseNullableDecimal(row.NormalizedData["Longitude"])
            || (importedIsActive.HasValue && routePoint.IsActive != importedIsActive.Value);
    }

    private static Personnel CreatePersonnel(ExcelPreviewRowDto row)
    {
        return new Personnel(
            row.NormalizedData["RegistrationNumber"]!,
            row.NormalizedData["FirstName"]!,
            row.NormalizedData["LastName"]!,
            row.NormalizedData["Department"],
            row.NormalizedData["Title"],
            row.NormalizedData["Phone"],
            row.NormalizedData["Email"],
            row.NormalizedData["Address"],
            ParseNullableDecimal(row.NormalizedData["Latitude"]),
            ParseNullableDecimal(row.NormalizedData["Longitude"]),
            DateTimeOffset.UtcNow);
    }

    private static void UpdatePersonnel(Personnel personnel, ExcelPreviewRowDto row)
    {
        personnel.Update(
            row.NormalizedData["FirstName"]!,
            row.NormalizedData["LastName"]!,
            row.NormalizedData["Department"],
            row.NormalizedData["Title"],
            row.NormalizedData["Phone"],
            row.NormalizedData["Email"],
            row.NormalizedData["Address"],
            ParseNullableDecimal(row.NormalizedData["Latitude"]),
            ParseNullableDecimal(row.NormalizedData["Longitude"]),
            DateTimeOffset.UtcNow);
    }

    private static RoutePoint CreateRoutePoint(Guid shuttleShiftId, ExcelPreviewRowDto row)
    {
        var now = DateTimeOffset.UtcNow;
        var routePoint = new RoutePoint(
            shuttleShiftId,
            int.Parse(row.NormalizedData["Order"]!, System.Globalization.CultureInfo.InvariantCulture),
            row.NormalizedData["Name"]!,
            row.NormalizedData["Address"],
            ParseNullableDecimal(row.NormalizedData["Latitude"])!.Value,
            ParseNullableDecimal(row.NormalizedData["Longitude"])!.Value,
            now);

        var importedIsActive = ParseNullableBoolean(row.NormalizedData["IsActive"]);
        if (importedIsActive.HasValue && !importedIsActive.Value)
        {
            routePoint.SetActiveStatus(false, now);
        }

        return routePoint;
    }

    private static void UpdateRoutePoint(RoutePoint routePoint, ExcelPreviewRowDto row)
    {
        var now = DateTimeOffset.UtcNow;
        routePoint.Update(
            row.NormalizedData["Name"]!,
            row.NormalizedData["Address"],
            ParseNullableDecimal(row.NormalizedData["Latitude"])!.Value,
            ParseNullableDecimal(row.NormalizedData["Longitude"])!.Value,
            now);

        var importedIsActive = ParseNullableBoolean(row.NormalizedData["IsActive"]);
        if (importedIsActive.HasValue && routePoint.IsActive != importedIsActive.Value)
        {
            routePoint.SetActiveStatus(importedIsActive.Value, now);
        }
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseDecimal(value, out var parsed)
            ? parsed
            : null;
    }

    private static string? NormalizeInteger(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is null)
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseInteger(rawValue, out var value)
            ? value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : null;
    }

    private static string? NormalizeTime(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is null)
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseTime(rawValue, out var value)
            ? value.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)
            : rawValue;
    }

    private static string? NormalizeBoolean(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is null)
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseBoolean(rawValue, out var value)
            ? value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()
            : rawValue;
    }

    private static void AddIntegerWarningIfInvalid(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField,
        string label,
        ICollection<string> warnings)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is not null && !ExcelValueNormalizer.TryParseInteger(rawValue, out _))
        {
            warnings.Add($"{label} tam sayiya cevrilemedi.");
        }
    }

    private static void AddBooleanWarningIfInvalid(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField,
        string label,
        ICollection<string> warnings)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is not null && !ExcelValueNormalizer.TryParseBoolean(rawValue, out _))
        {
            warnings.Add($"{label} evet/hayir degerine cevrilemedi.");
        }
    }

    private static void AddRequiredCoordinateErrorIfInvalid(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField,
        string label,
        ICollection<string> errors)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is null)
        {
            errors.Add($"{label} bos birakilamaz.");
            return;
        }

        if (!ExcelValueNormalizer.TryParseDecimal(rawValue, out _))
        {
            errors.Add($"{label} sayisal degere cevrilemedi.");
        }
    }

    private static string CapacityKey(string shuttleCode, string shiftName)
    {
        return $"{ExcelValueNormalizer.NormalizeCode(shuttleCode)}|{ExcelValueNormalizer.NormalizeWhitespace(shiftName)}";
    }

    private static string ShiftKey(string shuttleCode, string shiftName)
    {
        return CapacityKey(shuttleCode, shiftName);
    }

    private static string RouteKey(string shuttleCode, string shiftName, int order)
    {
        return $"{ShiftKey(shuttleCode, shiftName)}|{order}";
    }

    private static bool? ParseNullableBoolean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseBoolean(value, out var parsed)
            ? parsed
            : null;
    }

    private static ShiftType? ParseShiftType(string? value)
    {
        var normalized = ExcelColumnMappingSuggester.NormalizeKey(value ?? string.Empty);
        return normalized switch
        {
            "sabah" or "morning" or "1" => ShiftType.Morning,
            "aksam" or "evening" or "2" => ShiftType.Evening,
            "ozel" or "custom" or "3" => ShiftType.Custom,
            _ => null
        };
    }

    private static TimeOnly? ParseTime(string? value)
    {
        return ExcelValueNormalizer.TryParseTime(value, out var result) ? result : null;
    }

    private static string? NormalizeMappedValue(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField,
        Func<string?, string?> normalize)
    {
        if (!mapping.TryGetValue(targetField, out var sourceHeader)
            || !row.Values.TryGetValue(sourceHeader, out var value))
        {
            return null;
        }

        return normalize(value);
    }

    private static string? NormalizeCoordinate(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is null)
        {
            return null;
        }

        return ExcelValueNormalizer.TryParseDecimal(rawValue, out var value)
            ? value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : rawValue;
    }

    private static void AddCoordinateWarningIfInvalid(
        ExcelSheetRowDto row,
        IReadOnlyDictionary<string, string> mapping,
        string targetField,
        string label,
        ICollection<string> warnings)
    {
        var rawValue = NormalizeMappedValue(row, mapping, targetField, ExcelValueNormalizer.NullIfWhiteSpace);
        if (rawValue is not null && !ExcelValueNormalizer.TryParseDecimal(rawValue, out _))
        {
            warnings.Add($"{label} sayisal degere cevrilemedi.");
        }
    }
}
