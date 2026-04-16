using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "OrderMetricsSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "OrderMetricsSnapshots",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "OrderMetricsSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderMetricsSnapshots",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "OrderMetricsSnapshots");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "OrderMetricsSnapshots");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderMetricsSnapshots");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderMetricsSnapshots");
        }
    }
}
