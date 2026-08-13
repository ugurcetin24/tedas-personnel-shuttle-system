using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tedas.Shuttle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnelAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonnelAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PersonnelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShuttleShiftId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BoardingRoutePointId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeactivatedAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelAssignments_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonnelAssignments_ShuttleShifts_ShuttleShiftId",
                        column: x => x.ShuttleShiftId,
                        principalTable: "ShuttleShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelAssignments_IsActive",
                table: "PersonnelAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelAssignments_PersonnelId",
                table: "PersonnelAssignments",
                column: "PersonnelId",
                unique: true,
                filter: "\"IsActive\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelAssignments_ShuttleShiftId",
                table: "PersonnelAssignments",
                column: "ShuttleShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonnelAssignments");
        }
    }
}
