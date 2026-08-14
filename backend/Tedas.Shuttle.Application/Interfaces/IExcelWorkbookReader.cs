using Tedas.Shuttle.Application.DTOs.Imports;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IExcelWorkbookReader
{
    Task<ExcelWorkbookDto> ReadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken);
}
