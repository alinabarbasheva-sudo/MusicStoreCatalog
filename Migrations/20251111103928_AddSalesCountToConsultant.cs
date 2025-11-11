using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStoreCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesCountToConsultant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "specialization",
                table: "Users",
                newName: "Specialization");

            migrationBuilder.RenameColumn(
                name: "salecount",
                table: "Users",
                newName: "SalesCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Specialization",
                table: "Users",
                newName: "specialization");

            migrationBuilder.RenameColumn(
                name: "SalesCount",
                table: "Users",
                newName: "salecount");
        }
    }
}
