namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record CapacityImportCommitResultDto(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    IReadOnlyList<ExcelPreviewRowDto> Rows);
