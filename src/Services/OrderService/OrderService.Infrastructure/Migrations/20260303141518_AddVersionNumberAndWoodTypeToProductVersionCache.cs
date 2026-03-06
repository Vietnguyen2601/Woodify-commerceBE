using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionNumberAndWoodTypeToProductVersionCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "version_number",
                table: "product_version_cache",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wood_type",
                table: "product_version_cache",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version_number",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "wood_type",
                table: "product_version_cache");
        }
    }
}
