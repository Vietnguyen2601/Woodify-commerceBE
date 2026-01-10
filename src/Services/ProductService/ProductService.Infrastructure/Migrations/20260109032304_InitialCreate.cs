using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_master",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    global_sku = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    certified = table.Column<bool>(type: "boolean", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    avg_rating = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    review_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_master", x => x.product_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_master_global_sku",
                table: "product_master",
                column: "global_sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_shop_id",
                table: "product_master",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_status",
                table: "product_master",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_master");
        }
    }
}
