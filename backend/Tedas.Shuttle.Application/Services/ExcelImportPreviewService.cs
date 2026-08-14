using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Imports;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Services;

public sealed class ExcelImportPreviewService(
    IExcelWorkbookReader workbookReader,
    IPersonnelRepository personnelRepository)
    : IExcelImportPreviewService
{
    private static readonly IReadOnlySet<string> RequiredPersonnelFields =
        new HashSet<string>(["RegistrationNumber", "FirstName", "LastName"], StringComparer.Ordinal);

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
