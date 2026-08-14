namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record RouteImportCommitResultDto(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    IReadOnlyList<ExcelPreviewRowDto> Rows);
