using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tedas.Shuttle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShuttleShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShuttleShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhysicalShuttleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShiftType = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    EndTime = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShuttleShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShuttleShifts_PhysicalShuttles_PhysicalShuttleId",
                        column: x => x.PhysicalShuttleId,
                        principalTable: "PhysicalShuttles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShuttleShifts_IsActive",
                table: "ShuttleShifts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ShuttleShifts_PhysicalShuttleId",
                table: "ShuttleShifts",
                column: "PhysicalShuttleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShuttleShifts");
        }
    }
}
