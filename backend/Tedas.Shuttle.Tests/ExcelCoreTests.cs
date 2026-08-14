using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Excel;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class ExcelCoreTests
{
    [Fact]
    public async Task WorkbookReader_WithValidWorkbook_DetectsSheetHeadersAndRows()
    {
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil No";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(2, 1).Value = " test-1001 ";
            sheet.Cell(2, 2).Value = "Ahmet";
            sheet.Cell(2, 3).Value = "Yilmaz";
        });
        var reader = new ClosedXmlWorkbookReader();

        var workbook = await reader.ReadAsync(stream, "personel.xlsx", CancellationToken.None);

        Assert.Single(workbook.Sheets);
        Assert.Equal("Personel", workbook.Sheets[0].Name);
        Assert.Equal(["Sicil No", "Ad", "Soyad"], workbook.Sheets[0].Headers);
        Assert.Single(workbook.Sheets[0].Rows);
        Assert.Equal("test-1001", workbook.Sheets[0].Rows[0].Values["Sicil No"]);
    }

    [Fact]
    public async Task WorkbookReader_WithEmptySheet_MarksSheetAsEmpty()
    {
        await using var stream = CreateWorkbook(workbook => workbook.Worksheets.Add("Bos"));
        var reader = new ClosedXmlWorkbookReader();

        var workbook = await reader.ReadAsync(stream, "bos.xlsx", CancellationToken.None);

        Assert.True(workbook.Sheets[0].IsEmpty);
        Assert.Empty(workbook.Sheets[0].Headers);
        Assert.Empty(workbook.Sheets[0].Rows);
    }

    [Fact]
    public async Task WorkbookReader_WithUnsupportedExtension_ThrowsConflict()
    {
        await using var stream = CreateWorkbook(workbook => workbook.Worksheets.Add("Personel"));
        var reader = new ClosedXmlWorkbookReader();

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            reader.ReadAsync(stream, "personel.csv", CancellationToken.None));

        Assert.Equal("EXCEL_FILE_TYPE_UNSUPPORTED", exception.Code);
    }

    [Fact]
    public void ColumnMappingSuggester_WithTurkishAliases_SuggestsTargetFields()
    {
        var suggestions = ExcelColumnMappingSuggester.Suggest(
            ["Sicil No", "Ad", "Soyad", "Birim"],
            ExcelImportProfiles.Personnel);

        Assert.Contains(suggestions, suggestion =>
            suggestion.SourceHeader == "Sicil No" && suggestion.TargetField == "RegistrationNumber");
        Assert.Contains(suggestions, suggestion =>
            suggestion.SourceHeader == "Birim" && suggestion.TargetField == "Department");
    }

    [Fact]
    public void ValueNormalizer_HandlesRegistrationPhoneIntegerAndDecimalComma()
    {
        Assert.Equal("TEST-1001", ExcelValueNormalizer.NormalizeRegistrationNumber(" test-1001 "));
        Assert.Equal("+905321112233", ExcelValueNormalizer.NormalizePhone("+90 (532) 111 22 33"));
        Assert.True(ExcelValueNormalizer.TryParseInteger(" 42 ", out var number));
        Assert.Equal(42, number);
        Assert.True(ExcelValueNormalizer.TryParseDecimal("39,925", out var coordinate));
        Assert.Equal(39.925m, coordinate);
    }

    [Fact]
    public async Task PreviewPersonnelAsync_WithValidRows_ReturnsNormalizedPreview()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(1, 4).Value = "Enlem";
            sheet.Cell(1, 5).Value = "Boylam";
            sheet.Cell(2, 1).Value = " test-1001 ";
            sheet.Cell(2, 2).Value = " Ayse ";
            sheet.Cell(2, 3).Value = " Demir ";
            sheet.Cell(2, 4).Value = "39,925";
            sheet.Cell(2, 5).Value = "32,854";
        });
        var service = fixture.CreateService();

        var preview = await service.PreviewPersonnelAsync(stream, "personel.xlsx", null, CancellationToken.None);

        Assert.Single(preview.Rows);
        Assert.Equal("Valid", preview.Rows[0].Status);
        Assert.Equal("Create", preview.Rows[0].Action);
        Assert.Equal("TEST-1001", preview.Rows[0].NormalizedData["RegistrationNumber"]);
        Assert.Equal("39.925", preview.Rows[0].NormalizedData["Latitude"]);
        Assert.Equal("32.854", preview.Rows[0].NormalizedData["Longitude"]);
    }

    [Fact]
    public async Task PreviewPersonnelAsync_WithMissingRequiredHeader_ReturnsRowErrors()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
        });
        var service = fixture.CreateService();

        var preview = await service.PreviewPersonnelAsync(stream, "personel.xlsx", null, CancellationToken.None);

        Assert.Equal("Error", preview.Rows[0].Status);
        Assert.Equal("Skip", preview.Rows[0].Action);
        Assert.Contains(preview.Rows[0].Errors, error => error.Contains("LastName", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PreviewPersonnelAsync_WithInvalidCoordinate_ReturnsWarning()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(1, 4).Value = "Enlem";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
            sheet.Cell(2, 3).Value = "Demir";
            sheet.Cell(2, 4).Value = "Ankara";
        });
        var service = fixture.CreateService();

        var preview = await service.PreviewPersonnelAsync(stream, "personel.xlsx", null, CancellationToken.None);

        Assert.Equal("Warning", preview.Rows[0].Status);
        Assert.Contains(preview.Rows[0].Warnings, warning => warning.Contains("Enlem", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PreviewPersonnelAsync_WithExistingPersonnel_MarksUpdateOrNoChange()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await fixture.AddPersonnelAsync("TEST-1001", "Ayse", "Demir", "BT");
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(1, 4).Value = "Birim";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
            sheet.Cell(2, 3).Value = "Demir";
            sheet.Cell(2, 4).Value = "IK";
        });
        var service = fixture.CreateService();

        var preview = await service.PreviewPersonnelAsync(
            stream,
            "personel.xlsx",
            null,
            new Application.DTOs.Imports.PersonnelImportPreviewOptions(IncludeConflictDetection: true),
            CancellationToken.None);

        Assert.Equal("Warning", preview.Rows[0].Status);
        Assert.Equal("Update", preview.Rows[0].Action);
    }

    [Fact]
    public async Task PreviewPersonnelAsync_WithDuplicateRegistrationInWorkbook_MarksConflict()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
            sheet.Cell(2, 3).Value = "Demir";
            sheet.Cell(3, 1).Value = "TEST-1001";
            sheet.Cell(3, 2).Value = "Mehmet";
            sheet.Cell(3, 3).Value = "Yilmaz";
        });
        var service = fixture.CreateService();

        var preview = await service.PreviewPersonnelAsync(
            stream,
            "personel.xlsx",
            null,
            new Application.DTOs.Imports.PersonnelImportPreviewOptions(IncludeConflictDetection: true),
            CancellationToken.None);

        Assert.All(preview.Rows, row =>
        {
            Assert.Equal("Error", row.Status);
            Assert.Equal("Conflict", row.Action);
        });
    }

    [Fact]
    public async Task CommitPersonnelAsync_WithCreateAndUpdate_PersistsInTransaction()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await fixture.AddPersonnelAsync("TEST-1001", "Ayse", "Demir", "BT");
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(1, 3).Value = "Soyad";
            sheet.Cell(1, 4).Value = "Birim";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
            sheet.Cell(2, 3).Value = "Demir";
            sheet.Cell(2, 4).Value = "IK";
            sheet.Cell(3, 1).Value = "TEST-1002";
            sheet.Cell(3, 2).Value = "Mehmet";
            sheet.Cell(3, 3).Value = "Yilmaz";
            sheet.Cell(3, 4).Value = "Operasyon";
        });
        var service = fixture.CreateService();

        var result = await service.CommitPersonnelAsync(stream, "personel.xlsx", null, CancellationToken.None);

        Assert.Equal(1, result.CreatedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal("IK", (await fixture.GetPersonnelAsync("TEST-1001"))!.Department);
        Assert.NotNull(await fixture.GetPersonnelAsync("TEST-1002"));
    }

    [Fact]
    public async Task CommitPersonnelAsync_WithPreviewErrors_DoesNotPersist()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Personel");
            sheet.Cell(1, 1).Value = "Sicil";
            sheet.Cell(1, 2).Value = "Ad";
            sheet.Cell(2, 1).Value = "TEST-1001";
            sheet.Cell(2, 2).Value = "Ayse";
        });
        var service = fixture.CreateService();

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CommitPersonnelAsync(stream, "personel.xlsx", null, CancellationToken.None));

        Assert.Equal("PERSONNEL_IMPORT_PREVIEW_HAS_ERRORS", exception.Code);
        Assert.Null(await fixture.GetPersonnelAsync("TEST-1001"));
    }

    private static MemoryStream CreateWorkbook(Action<XLWorkbook> configure)
    {
        using var workbook = new XLWorkbook();
        configure(workbook);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }

    private sealed class ExcelImportFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private AppDbContext DbContext { get; }

        private ExcelImportFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<ExcelImportFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new ExcelImportFixture(connection, dbContext);
        }

        public ExcelImportPreviewService CreateService()
        {
            return new ExcelImportPreviewService(
                new ClosedXmlWorkbookReader(),
                new PersonnelRepository(DbContext));
        }

        public async Task AddPersonnelAsync(
            string registrationNumber,
            string firstName,
            string lastName,
            string? department)
        {
            DbContext.Personnel.Add(new Personnel(
                registrationNumber,
                firstName,
                lastName,
                department,
                null,
                null,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow));
            await DbContext.SaveChangesAsync();
        }

        public Task<Personnel?> GetPersonnelAsync(string registrationNumber)
        {
            return DbContext.Personnel.FirstOrDefaultAsync(
                personnel => personnel.RegistrationNumber == registrationNumber);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
