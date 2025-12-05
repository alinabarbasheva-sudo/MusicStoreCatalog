using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStoreCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstrumentId = table.Column<int>(type: "INTEGER", nullable: true),
                    InstrumentName = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    RequestedById = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRequests_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderRequests_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_UserId",
                table: "Schedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRequests_ApprovedById",
                table: "OrderRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRequests_InstrumentId",
                table: "OrderRequests",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRequests_RequestedById",
                table: "OrderRequests",
                column: "RequestedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "OrderRequests");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_UserId",
                table: "Schedules");
        }
    }
}
