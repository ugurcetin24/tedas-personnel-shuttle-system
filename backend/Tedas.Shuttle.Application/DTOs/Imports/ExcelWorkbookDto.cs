namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ExcelWorkbookDto(
    string FileName,
    IReadOnlyList<ExcelSheetDto> Sheets);
