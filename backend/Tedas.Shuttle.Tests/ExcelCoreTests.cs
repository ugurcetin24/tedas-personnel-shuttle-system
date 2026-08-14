using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Imports;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Domain.Enums;
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

    [Fact]
    public async Task PreviewCapacityAsync_WithExistingShift_MarksUpdate()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        var shuttle = await fixture.AddShuttleAsync("SERVIS-01");
        await fixture.AddShiftAsync(shuttle, "Sabah", 18);
        await using var stream = CreateCapacityWorkbook("SERVIS-01", "Sabah", "20");
        var service = fixture.CreateService();

        var preview = await service.PreviewCapacityAsync(stream, "kapasite.xlsx", null, CancellationToken.None);

        Assert.Equal("Warning", preview.Rows[0].Status);
        Assert.Equal("Update", preview.Rows[0].Action);
        Assert.Equal("18", preview.Rows[0].NormalizedData["CurrentCapacity"]);
    }

    [Fact]
    public async Task PreviewCapacityAsync_WhenCapacityBelowOccupancy_MarksConflict()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        var shuttle = await fixture.AddShuttleAsync("SERVIS-01");
        var shift = await fixture.AddShiftAsync(shuttle, "Sabah", 20);
        await fixture.AddAssignmentsAsync(shift, 3);
        await using var stream = CreateCapacityWorkbook("SERVIS-01", "Sabah", "2");
        var service = fixture.CreateService();

        var preview = await service.PreviewCapacityAsync(stream, "kapasite.xlsx", null, CancellationToken.None);

        Assert.Equal("Error", preview.Rows[0].Status);
        Assert.Equal("Conflict", preview.Rows[0].Action);
        Assert.Contains(preview.Rows[0].Errors, error => error.Contains("aktif atama", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CommitCapacityAsync_WithExistingAndNewShift_PersistsInTransaction()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        var shuttle = await fixture.AddShuttleAsync("SERVIS-01");
        await fixture.AddShiftAsync(shuttle, "Sabah", 18);
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Kapasite");
            AddCapacityHeaders(sheet);
            sheet.Cell(2, 1).Value = "SERVIS-01";
            sheet.Cell(2, 2).Value = "Sabah";
            sheet.Cell(2, 3).Value = "20";
            sheet.Cell(3, 1).Value = "SERVIS-01";
            sheet.Cell(3, 2).Value = "Aksam";
            sheet.Cell(3, 3).Value = "16";
            sheet.Cell(3, 4).Value = "Aksam";
            sheet.Cell(3, 5).Value = "17:30";
            sheet.Cell(3, 6).Value = "19:00";
        });
        var service = fixture.CreateService();

        var result = await service.CommitCapacityAsync(stream, "kapasite.xlsx", null, CancellationToken.None);

        Assert.Equal(1, result.CreatedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(20, (await fixture.GetShiftAsync("SERVIS-01", "Sabah"))!.Capacity);
        Assert.Equal(16, (await fixture.GetShiftAsync("SERVIS-01", "Aksam"))!.Capacity);
    }

    [Fact]
    public async Task CommitCapacityAsync_WithDuplicateRows_DoesNotPersist()
    {
        await using var fixture = await ExcelImportFixture.CreateAsync();
        var shuttle = await fixture.AddShuttleAsync("SERVIS-01");
        await fixture.AddShiftAsync(shuttle, "Sabah", 18);
        await using var stream = CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Kapasite");
            AddCapacityHeaders(sheet);
            sheet.Cell(2, 1).Value = "SERVIS-01";
            sheet.Cell(2, 2).Value = "Sabah";
            sheet.Cell(2, 3).Value = "20";
            sheet.Cell(3, 1).Value = "SERVIS-01";
            sheet.Cell(3, 2).Value = "Sabah";
            sheet.Cell(3, 3).Value = "21";
        });
        var service = fixture.CreateService();

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CommitCapacityAsync(stream, "kapasite.xlsx", null, CancellationToken.None));

        Assert.Equal("CAPACITY_IMPORT_PREVIEW_HAS_ERRORS", exception.Code);
        Assert.Equal(18, (await fixture.GetShiftAsync("SERVIS-01", "Sabah"))!.Capacity);
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

    private static MemoryStream CreateCapacityWorkbook(string shuttleCode, string shiftName, string capacity)
    {
        return CreateWorkbook(workbook =>
        {
            var sheet = workbook.Worksheets.Add("Kapasite");
            AddCapacityHeaders(sheet);
            sheet.Cell(2, 1).Value = shuttleCode;
            sheet.Cell(2, 2).Value = shiftName;
            sheet.Cell(2, 3).Value = capacity;
        });
    }

    private static void AddCapacityHeaders(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Servis Kodu";
        sheet.Cell(1, 2).Value = "Vardiya";
        sheet.Cell(1, 3).Value = "Kapasite";
        sheet.Cell(1, 4).Value = "Vardiya Tipi";
        sheet.Cell(1, 5).Value = "Baslangic";
        sheet.Cell(1, 6).Value = "Bitis";
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
                new PersonnelRepository(DbContext),
                new ShiftRepository(DbContext));
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

        public async Task<PhysicalShuttle> AddShuttleAsync(string code)
        {
            var shuttle = new PhysicalShuttle(code, "06 TEST 01", "Test servis", DateTimeOffset.UtcNow);
            DbContext.PhysicalShuttles.Add(shuttle);
            await DbContext.SaveChangesAsync();

            return shuttle;
        }

        public async Task<ShuttleShift> AddShiftAsync(
            PhysicalShuttle shuttle,
            string name,
            int capacity)
        {
            var shift = new ShuttleShift(
                shuttle.Id,
                name,
                ShiftType.Morning,
                capacity,
                new TimeOnly(7, 30),
                new TimeOnly(9, 0),
                DateTimeOffset.UtcNow);
            DbContext.ShuttleShifts.Add(shift);
            await DbContext.SaveChangesAsync();

            return shift;
        }

        public async Task AddAssignmentsAsync(ShuttleShift shift, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var personnel = new Personnel(
                    $"TEST-{2000 + index}",
                    "Test",
                    $"Personel{index}",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    DateTimeOffset.UtcNow);
                DbContext.Personnel.Add(personnel);
                DbContext.PersonnelAssignments.Add(new PersonnelAssignment(
                    personnel.Id,
                    shift.Id,
                    null,
                    DateTimeOffset.UtcNow));
            }

            await DbContext.SaveChangesAsync();
        }

        public Task<ShuttleShift?> GetShiftAsync(string shuttleCode, string shiftName)
        {
            return DbContext.ShuttleShifts
                .Include(shift => shift.PhysicalShuttle)
                .FirstOrDefaultAsync(shift =>
                    shift.PhysicalShuttle!.Code == shuttleCode
                    && shift.Name == shiftName);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
