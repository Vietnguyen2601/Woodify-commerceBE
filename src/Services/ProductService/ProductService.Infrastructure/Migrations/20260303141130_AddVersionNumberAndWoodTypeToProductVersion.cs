using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionNumberAndWoodTypeToProductVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "version_number",
                table: "product_versions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wood_type",
                table: "product_versions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version_number",
                table: "product_versions");

            migrationBuilder.DropColumn(
                name: "wood_type",
                table: "product_versions");
        }
    }
}
