using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCategoryProductVersionProductReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_version_sku",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_review_is_verified",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "ar_available",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "description",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "sku",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "title",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "helpful_count",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "title",
                table: "product_review");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "product_version",
                newName: "requires_special_handling");

            migrationBuilder.AlterColumn<long>(
                name: "price_cents",
                table: "product_version",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "allow_backorder",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "base_price_cents",
                table: "product_version",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bulky_type",
                table: "product_version",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "bundle_discount_cents",
                table: "product_version",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "height_cm",
                table: "product_version",
                type: "numeric(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_bundle",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_fragile",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "length_cm",
                table: "product_version",
                type: "numeric(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "low_stock_threshold",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "primary_image_url",
                table: "product_version",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_sku",
                table: "product_version",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "stock_quantity",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "version_name",
                table: "product_version",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version_number",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "volume_cm3",
                table: "product_version",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "warranty_months",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<string>(
                name: "warranty_terms",
                table: "product_version",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight_grams",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "width_cm",
                table: "product_version",
                type: "numeric(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "hidden_at",
                table: "product_review",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "hidden_by",
                table: "product_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_visible",
                table: "product_review",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "shop_response",
                table: "product_review",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "shop_response_at",
                table: "product_review",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "version_id",
                table: "product_review",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "category",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "category",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "category",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "category",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "category",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_product_version_is_active",
                table: "product_version",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_is_default",
                table: "product_version",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_seller_sku",
                table: "product_version",
                column: "seller_sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_review_is_visible",
                table: "product_review",
                column: "is_visible");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_version_id",
                table: "product_review",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_display_order",
                table: "category",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "IX_category_level",
                table: "category",
                column: "level");

            migrationBuilder.AddForeignKey(
                name: "FK_product_review_product_version_version_id",
                table: "product_review",
                column: "version_id",
                principalTable: "product_version",
                principalColumn: "version_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_review_product_version_version_id",
                table: "product_review");

            migrationBuilder.DropIndex(
                name: "IX_product_version_is_active",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_version_is_default",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_version_seller_sku",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_review_is_visible",
                table: "product_review");

            migrationBuilder.DropIndex(
                name: "IX_product_review_version_id",
                table: "product_review");

            migrationBuilder.DropIndex(
                name: "IX_category_display_order",
                table: "category");

            migrationBuilder.DropIndex(
                name: "IX_category_level",
                table: "category");

            migrationBuilder.DropColumn(
                name: "allow_backorder",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "base_price_cents",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "bulky_type",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "bundle_discount_cents",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "height_cm",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_bundle",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_fragile",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "length_cm",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "low_stock_threshold",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "primary_image_url",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "seller_sku",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "stock_quantity",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "version_name",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "version_number",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "volume_cm3",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "warranty_months",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "warranty_terms",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "weight_grams",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "width_cm",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "hidden_at",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "hidden_by",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "is_visible",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "shop_response",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "shop_response_at",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "version_id",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "category");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "category");

            migrationBuilder.DropColumn(
                name: "level",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "requires_special_handling",
                table: "product_version",
                newName: "is_deleted");

            migrationBuilder.AlterColumn<long>(
                name: "price_cents",
                table: "product_version",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "ar_available",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "product_version",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "product_version",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "product_version",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "product_version",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sku",
                table: "product_version",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "product_version",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "helpful_count",
                table: "product_review",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "product_review",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "product_review",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "category",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "category",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_sku",
                table: "product_version",
                column: "sku");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_is_verified",
                table: "product_review",
                column: "is_verified");
        }
    }
}
