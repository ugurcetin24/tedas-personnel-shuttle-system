using Tedas.Shuttle.Application.DTOs.Imports;

namespace Tedas.Shuttle.Application.Services;

public interface IExcelImportPreviewService
{
    Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken);

    Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        PersonnelImportPreviewOptions options,
        CancellationToken cancellationToken);

    Task<PersonnelImportCommitResultDto> CommitPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken);
}
