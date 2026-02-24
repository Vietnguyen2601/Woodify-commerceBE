using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductVersionCacheSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_version_cache_sku",
                table: "product_version_cache");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "product_version_cache",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "product_version_cache",
                newName: "version_name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "product_version_cache",
                newName: "product_description");

            migrationBuilder.AlterColumn<long>(
                name: "price_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "allow_backorder",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "base_price_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bulky_type",
                table: "product_version_cache",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "bundle_discount_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "height_cm",
                table: "product_version_cache",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_bundle",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_fragile",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "length_cm",
                table: "product_version_cache",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "low_stock_threshold",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "primary_image_url",
                table: "product_version_cache",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_special_handling",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "seller_sku",
                table: "product_version_cache",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "shop_id",
                table: "product_version_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "stock_quantity",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "version_number",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "volume_cm3",
                table: "product_version_cache",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "warranty_months",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<string>(
                name: "warranty_terms",
                table: "product_version_cache",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight_grams",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "width_cm",
                table: "product_version_cache",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_is_active",
                table: "product_version_cache",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_seller_sku",
                table: "product_version_cache",
                column: "seller_sku");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_shop_id",
                table: "product_version_cache",
                column: "shop_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_version_cache_is_active",
                table: "product_version_cache");

            migrationBuilder.DropIndex(
                name: "IX_product_version_cache_seller_sku",
                table: "product_version_cache");

            migrationBuilder.DropIndex(
                name: "IX_product_version_cache_shop_id",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "allow_backorder",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "base_price_cents",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "bulky_type",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "bundle_discount_cents",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "height_cm",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_bundle",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_fragile",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "length_cm",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "low_stock_threshold",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "primary_image_url",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "requires_special_handling",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "seller_sku",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "shop_id",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "stock_quantity",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "version_number",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "volume_cm3",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "warranty_months",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "warranty_terms",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "weight_grams",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "width_cm",
                table: "product_version_cache");

            migrationBuilder.RenameColumn(
                name: "version_name",
                table: "product_version_cache",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "product_version_cache",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "product_description",
                table: "product_version_cache",
                newName: "description");

            migrationBuilder.AlterColumn<long>(
                name: "price_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_cache_sku",
                table: "product_version_cache",
                column: "sku");
        }
    }
}
