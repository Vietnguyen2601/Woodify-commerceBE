using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVersionCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_version_cache",
                columns: table => new
                {
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price_cents = table.Column<long>(type: "bigint", nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sku = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    product_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_version_cache", x => x.version_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_product_id",
                table: "product_version_cache",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_sku",
                table: "product_version_cache",
                column: "sku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_version_cache");
        }
    }
}
