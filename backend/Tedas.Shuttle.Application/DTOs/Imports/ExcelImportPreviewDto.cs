namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ExcelImportPreviewDto(
    string FileName,
    string SheetName,
    IReadOnlyList<string> Headers,
    IReadOnlyList<ColumnMappingSuggestionDto> MappingSuggestions,
    IReadOnlyList<ExcelPreviewRowDto> Rows);
