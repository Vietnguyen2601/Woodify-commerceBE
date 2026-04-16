using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndProductMasterCaches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_cache",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_cache", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "product_master_cache",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    moderation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    has_versions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_master_cache", x => x.product_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_cache_is_active",
                table: "category_cache",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_category_cache_parent_category_id",
                table: "category_cache",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_cache_category_id",
                table: "product_master_cache",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_cache_is_deleted",
                table: "product_master_cache",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_cache_shop_id",
                table: "product_master_cache",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_cache_status",
                table: "product_master_cache",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_cache");

            migrationBuilder.DropTable(
                name: "product_master_cache");
        }
    }
}
