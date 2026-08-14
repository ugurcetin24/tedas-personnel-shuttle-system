namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ExcelSheetRowDto(
    int RowNumber,
    IReadOnlyDictionary<string, string?> Values);
