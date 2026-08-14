using Tedas.Shuttle.Application.DTOs.Imports;

namespace Tedas.Shuttle.Application.Services;

public interface IExcelImportPreviewService
{
    Task<ExcelImportPreviewDto> PreviewPersonnelAsync(
        Stream stream,
        string fileName,
        string? sheetName,
        CancellationToken cancellationToken);
}
