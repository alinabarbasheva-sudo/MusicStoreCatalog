using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStoreCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shedules_Users_UserId",
                table: "Shedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shedules",
                table: "Shedules");

            migrationBuilder.RenameTable(
                name: "Shedules",
                newName: "Schedules");

            migrationBuilder.RenameIndex(
                name: "IX_Shedules_UserId",
                table: "Schedules",
                newName: "IX_Schedules_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules");

            migrationBuilder.RenameTable(
                name: "Schedules",
                newName: "Shedules");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_UserId",
                table: "Shedules",
                newName: "IX_Shedules_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shedules",
                table: "Shedules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shedules_Users_UserId",
                table: "Shedules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
