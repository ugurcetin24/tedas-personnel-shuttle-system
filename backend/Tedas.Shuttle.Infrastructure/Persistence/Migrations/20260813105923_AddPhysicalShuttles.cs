using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tedas.Shuttle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalShuttles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhysicalShuttles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PlateNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalShuttles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShuttles_Code",
                table: "PhysicalShuttles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShuttles_IsActive",
                table: "PhysicalShuttles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShuttles_PlateNumber",
                table: "PhysicalShuttles",
                column: "PlateNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhysicalShuttles");
        }
    }
}
