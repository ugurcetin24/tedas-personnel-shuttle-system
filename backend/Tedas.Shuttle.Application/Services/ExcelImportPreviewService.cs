using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Imports;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Application.Services;

public sealed class ExcelImportPreviewService(IExcelWorkbookReader workbookReader) : IExcelImportPreviewService
{
    private static readonly IReadOnlySet<string> RequiredPersonnelFields =
        new HashSet<string>(["RegistrationNumber", "FirstName", "LastName"], StringComparer.Ordinal);

    public async Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
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

        return new ExcelImportPreviewDto(
            workbook.FileName,
            sheet.Name,
            sheet.Headers,
            suggestions,
            rows);
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

        return new ExcelPreviewRowDto(row.RowNumber, status, normalizedData, errors, warnings);
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
