using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Api.Controllers;

[ApiController]
[Route("api/imports")]
public sealed class ImportsController(IExcelImportPreviewService excelImportPreviewService) : ControllerBase
{
    [HttpPost("personnel/preview")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> PreviewPersonnel(
        IFormFile file,
        [FromForm] string? sheetName,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new BusinessConflictException(
                "EXCEL_FILE_EMPTY",
                "Excel dosyasi bos olamaz.");
        }

        await using var stream = file.OpenReadStream();
        var preview = await excelImportPreviewService.PreviewPersonnelAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(preview);
    }
}
