using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Personnel;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class PersonnelServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesActivePersonnel()
    {
        await using var fixture = await PersonnelServiceFixture.CreateAsync();
        var service = fixture.CreateService();

        var personnel = await service.CreateAsync(
            CreateRequest("TEST-1001"),
            CancellationToken.None);

        Assert.Equal("TEST-1001", personnel.RegistrationNumber);
        Assert.Equal("Ahmet Yılmaz", personnel.FullName);
        Assert.True(personnel.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateRegistrationNumber_ThrowsConflict()
    {
        await using var fixture = await PersonnelServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        await service.CreateAsync(CreateRequest("TEST-1001"), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(CreateRequest(" TEST-1001 "), CancellationToken.None));

        Assert.Equal("PERSONNEL_REGISTRATION_NUMBER_DUPLICATE", exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPersonnel_UpdatesEditableFields()
    {
        await using var fixture = await PersonnelServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        var personnel = await service.CreateAsync(CreateRequest("TEST-1001"), CancellationToken.None);

        var updated = await service.UpdateAsync(
            personnel.Id,
            new UpdatePersonnelRequest(
                "Ayşe",
                "Demir",
                "İnsan Kaynakları",
                "Uzman",
                "555 000 0000",
                "ayse.demir@example.test",
                "Test adres",
                39.925m,
                32.836m),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Ayşe Demir", updated.FullName);
        Assert.Equal("İnsan Kaynakları", updated.Department);
        Assert.Equal(39.925m, updated.Latitude);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingPersonnel_ChangesActiveStatus()
    {
        await using var fixture = await PersonnelServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        var personnel = await service.CreateAsync(CreateRequest("TEST-1001"), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            personnel.Id,
            new UpdatePersonnelStatusRequest(IsActive: false),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task SearchAsync_FiltersByRegistrationNumber()
    {
        await using var fixture = await PersonnelServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        await service.CreateAsync(CreateRequest("TEST-1001"), CancellationToken.None);
        await service.CreateAsync(CreateRequest("TEST-2001"), CancellationToken.None);

        var result = await service.SearchAsync(
            new PersonnelQuery(Search: "2001"),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("TEST-2001", result.Items.Single().RegistrationNumber);
    }

    private static CreatePersonnelRequest CreateRequest(string registrationNumber)
    {
        return new CreatePersonnelRequest(
            registrationNumber,
            "Ahmet",
            "Yılmaz",
            "Bilgi Teknolojileri",
            "Uzman",
            "555 000 0000",
            "ahmet.yilmaz@example.test",
            "Test adres",
            null,
            null);
    }

    private sealed class PersonnelServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private PersonnelServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<PersonnelServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new PersonnelServiceFixture(connection, dbContext);
        }

        public PersonnelService CreateService()
        {
            return new PersonnelService(
                new PersonnelRepository(DbContext),
                new CreatePersonnelRequestValidator(),
                new UpdatePersonnelRequestValidator());
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
