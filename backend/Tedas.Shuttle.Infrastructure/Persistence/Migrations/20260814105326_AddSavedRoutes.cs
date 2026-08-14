using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tedas.Shuttle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShuttleShiftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    DistanceMeters = table.Column<double>(type: "REAL", nullable: false),
                    DurationSeconds = table.Column<double>(type: "REAL", nullable: false),
                    Geometry = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedRoutes_ShuttleShifts_ShuttleShiftId",
                        column: x => x.ShuttleShiftId,
                        principalTable: "ShuttleShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedRoutes_ShuttleShiftId",
                table: "SavedRoutes",
                column: "ShuttleShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedRoutes");
        }
    }
}
