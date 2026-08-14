using ClosedXML.Excel;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Imports;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Infrastructure.Excel;

public sealed class ClosedXmlWorkbookReader : IExcelWorkbookReader
{
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new HashSet<string>([".xlsx", ".xlsm"], StringComparer.OrdinalIgnoreCase);

    public Task<ExcelWorkbookDto> ReadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFileName(fileName);

        try
        {
            using var workbook = new XLWorkbook(stream);
            var sheets = workbook.Worksheets
                .Select(ReadSheet)
                .ToArray();

            if (sheets.Length == 0)
            {
                throw new BusinessConflictException(
                    "EXCEL_WORKBOOK_EMPTY",
                    "Excel dosyasinda sayfa bulunamadi.");
            }

            return Task.FromResult(new ExcelWorkbookDto(Path.GetFileName(fileName), sheets));
        }
        catch (BusinessConflictException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new BusinessConflictException(
                "EXCEL_WORKBOOK_UNREADABLE",
                "Excel dosyasi okunamadi veya desteklenmeyen bir icerige sahip.");
        }
    }

    private static ExcelSheetDto ReadSheet(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return new ExcelSheetDto(worksheet.Name, true, 0, [], []);
        }

        var headerRow = usedRange.FirstRowUsed();
        if (headerRow is null)
        {
            return new ExcelSheetDto(worksheet.Name, true, 0, [], []);
        }

        var firstColumn = usedRange.FirstColumn().ColumnNumber();
        var lastColumn = usedRange.LastColumn().ColumnNumber();
        var headers = Enumerable
            .Range(firstColumn, lastColumn - firstColumn + 1)
            .Select(column => ExcelValueNormalizer.NormalizeWhitespace(headerRow.Cell(column).GetFormattedString()))
            .Select((header, index) => string.IsNullOrWhiteSpace(header) ? $"Column{index + 1}" : header)
            .ToArray();

        var rows = worksheet.RowsUsed()
            .Where(row => row.RowNumber() > headerRow.RowNumber())
            .Select(row => ReadRow(row, firstColumn, headers))
            .Where(row => row.Values.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
            .ToArray();

        return new ExcelSheetDto(
            worksheet.Name,
            headers.Length == 0 && rows.Length == 0,
            headerRow.RowNumber(),
            headers,
            rows);
    }

    private static ExcelSheetRowDto ReadRow(IXLRow row, int firstColumn, IReadOnlyList<string> headers)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < headers.Count; index++)
        {
            values[headers[index]] = ExcelValueNormalizer.NullIfWhiteSpace(
                row.Cell(firstColumn + index).GetFormattedString());
        }

        return new ExcelSheetRowDto(row.RowNumber(), values);
    }

    private static void ValidateFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (!SupportedExtensions.Contains(extension))
        {
            throw new BusinessConflictException(
                "EXCEL_FILE_TYPE_UNSUPPORTED",
                "Yalnizca .xlsx veya .xlsm Excel dosyalari desteklenir.");
        }
    }
}
