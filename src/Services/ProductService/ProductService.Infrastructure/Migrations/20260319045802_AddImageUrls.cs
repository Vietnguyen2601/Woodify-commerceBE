using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "image_urls",
                columns: table => new
                {
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    original_url = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_urls", x => x.image_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_image_urls_created_at",
                table: "image_urls",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_image_urls_image_type_reference_id",
                table: "image_urls",
                columns: new[] { "image_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "IX_image_urls_reference_id",
                table: "image_urls",
                column: "reference_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "image_urls");
        }
    }
}
