namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ExcelPreviewRowDto(
    int RowNumber,
    string Status,
    string Action,
    IReadOnlyDictionary<string, string?> NormalizedData,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);
