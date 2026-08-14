using Microsoft.AspNetCore.Mvc;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Imports;
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
            new PersonnelImportPreviewOptions(IncludeConflictDetection: true),
            cancellationToken);

        return Ok(preview);
    }

    [HttpPost("personnel/commit")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CommitPersonnel(
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
        var result = await excelImportPreviewService.CommitPersonnelAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("capacity/preview")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> PreviewCapacity(
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
        var preview = await excelImportPreviewService.PreviewCapacityAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(preview);
    }

    [HttpPost("capacity/commit")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CommitCapacity(
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
        var result = await excelImportPreviewService.CommitCapacityAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("routes/preview")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> PreviewRoute(
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
        var preview = await excelImportPreviewService.PreviewRouteAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(preview);
    }

    [HttpPost("routes/commit")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CommitRoute(
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
        var result = await excelImportPreviewService.CommitRouteAsync(
            stream,
            file.FileName,
            sheetName,
            cancellationToken);

        return Ok(result);
    }
}
