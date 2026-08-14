namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record PersonnelImportCommitResultDto(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    IReadOnlyList<ExcelPreviewRowDto> Rows);
