namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ExcelSheetDto(
    string Name,
    bool IsEmpty,
    int HeaderRowNumber,
    IReadOnlyList<string> Headers,
    IReadOnlyList<ExcelSheetRowDto> Rows);
