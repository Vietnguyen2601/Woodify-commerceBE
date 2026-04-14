using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductColumnsToVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "OrderMetricsSnapshots",
                newName: "ProductVersionName");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "OrderMetricsSnapshots",
                newName: "ProductVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductVersionName",
                table: "OrderMetricsSnapshots",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "ProductVersionId",
                table: "OrderMetricsSnapshots",
                newName: "ProductId");
        }
    }
}
