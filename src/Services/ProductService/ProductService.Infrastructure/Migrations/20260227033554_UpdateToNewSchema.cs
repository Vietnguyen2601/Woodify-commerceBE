using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_category_category_parent_category_id",
                table: "category");

            migrationBuilder.DropForeignKey(
                name: "FK_product_master_category_category_id",
                table: "product_master");

            migrationBuilder.DropForeignKey(
                name: "FK_product_review_product_master_product_id",
                table: "product_review");

            migrationBuilder.DropForeignKey(
                name: "FK_product_review_product_version_version_id",
                table: "product_review");

            migrationBuilder.DropForeignKey(
                name: "FK_product_version_product_master_product_id",
                table: "product_version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_version",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_version_is_default",
                table: "product_version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_review",
                table: "product_review");

            migrationBuilder.DropIndex(
                name: "IX_product_review_product_id",
                table: "product_review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_category",
                table: "category");

            migrationBuilder.DropIndex(
                name: "IX_category_display_order",
                table: "category");

            migrationBuilder.DropColumn(
                name: "ar_available",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "ar_model_url",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "avg_rating",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "img_url",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "moderated_by",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "moderation_notes",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "review_count",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "sold_count",
                table: "product_master");

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
                name: "is_bundle",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_fragile",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "low_stock_threshold",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "price_cents",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "primary_image_url",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "requires_special_handling",
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
                name: "hidden_at",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "hidden_by",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "product_review");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "category");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "category");

            migrationBuilder.RenameTable(
                name: "product_version",
                newName: "product_versions");

            migrationBuilder.RenameTable(
                name: "product_review",
                newName: "product_reviews");

            migrationBuilder.RenameTable(
                name: "category",
                newName: "categories");

            migrationBuilder.RenameIndex(
                name: "IX_product_version_seller_sku",
                table: "product_versions",
                newName: "IX_product_versions_seller_sku");

            migrationBuilder.RenameIndex(
                name: "IX_product_version_product_id",
                table: "product_versions",
                newName: "IX_product_versions_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_version_is_active",
                table: "product_versions",
                newName: "IX_product_versions_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_product_version_created_at",
                table: "product_versions",
                newName: "IX_product_versions_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_version_id",
                table: "product_reviews",
                newName: "IX_product_reviews_version_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_order_id_account_id",
                table: "product_reviews",
                newName: "IX_product_reviews_order_id_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_order_id",
                table: "product_reviews",
                newName: "IX_product_reviews_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_is_visible",
                table: "product_reviews",
                newName: "IX_product_reviews_is_visible");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_created_at",
                table: "product_reviews",
                newName: "IX_product_reviews_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_product_review_account_id",
                table: "product_reviews",
                newName: "IX_product_reviews_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_category_parent_category_id",
                table: "categories",
                newName: "IX_categories_parent_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_category_name",
                table: "categories",
                newName: "IX_categories_name");

            migrationBuilder.RenameIndex(
                name: "IX_category_level",
                table: "categories",
                newName: "IX_categories_level");

            migrationBuilder.RenameIndex(
                name: "IX_category_is_active",
                table: "categories",
                newName: "IX_categories_is_active");

            migrationBuilder.AddColumn<double>(
                name: "price",
                table: "product_versions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<Guid>(
                name: "version_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_versions",
                table: "product_versions",
                column: "version_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_reviews",
                table: "product_reviews",
                column: "review_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id",
                principalTable: "categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_master_categories_category_id",
                table: "product_master",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_reviews_product_versions_version_id",
                table: "product_reviews",
                column: "version_id",
                principalTable: "product_versions",
                principalColumn: "version_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_versions_product_master_product_id",
                table: "product_versions",
                column: "product_id",
                principalTable: "product_master",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_product_master_categories_category_id",
                table: "product_master");

            migrationBuilder.DropForeignKey(
                name: "FK_product_reviews_product_versions_version_id",
                table: "product_reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_product_versions_product_master_product_id",
                table: "product_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_versions",
                table: "product_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_reviews",
                table: "product_reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "price",
                table: "product_versions");

            migrationBuilder.RenameTable(
                name: "product_versions",
                newName: "product_version");

            migrationBuilder.RenameTable(
                name: "product_reviews",
                newName: "product_review");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "category");

            migrationBuilder.RenameIndex(
                name: "IX_product_versions_seller_sku",
                table: "product_version",
                newName: "IX_product_version_seller_sku");

            migrationBuilder.RenameIndex(
                name: "IX_product_versions_product_id",
                table: "product_version",
                newName: "IX_product_version_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_versions_is_active",
                table: "product_version",
                newName: "IX_product_version_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_product_versions_created_at",
                table: "product_version",
                newName: "IX_product_version_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_version_id",
                table: "product_review",
                newName: "IX_product_review_version_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_order_id_account_id",
                table: "product_review",
                newName: "IX_product_review_order_id_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_order_id",
                table: "product_review",
                newName: "IX_product_review_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_is_visible",
                table: "product_review",
                newName: "IX_product_review_is_visible");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_created_at",
                table: "product_review",
                newName: "IX_product_review_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_product_reviews_account_id",
                table: "product_review",
                newName: "IX_product_review_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_categories_parent_category_id",
                table: "category",
                newName: "IX_category_parent_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_categories_name",
                table: "category",
                newName: "IX_category_name");

            migrationBuilder.RenameIndex(
                name: "IX_categories_level",
                table: "category",
                newName: "IX_category_level");

            migrationBuilder.RenameIndex(
                name: "IX_categories_is_active",
                table: "category",
                newName: "IX_category_is_active");

            migrationBuilder.AddColumn<bool>(
                name: "ar_available",
                table: "product_master",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ar_model_url",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "avg_rating",
                table: "product_master",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "img_url",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "moderated_by",
                table: "product_master",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moderation_notes",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "review_count",
                table: "product_master",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sold_count",
                table: "product_master",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<int>(
                name: "low_stock_threshold",
                table: "product_version",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<long>(
                name: "price_cents",
                table: "product_version",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "primary_image_url",
                table: "product_version",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_special_handling",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AlterColumn<Guid>(
                name: "version_id",
                table: "product_review",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "product_review",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_version",
                table: "product_version",
                column: "version_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_review",
                table: "product_review",
                column: "review_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_category",
                table: "category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_version_is_default",
                table: "product_version",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_product_id",
                table: "product_review",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_display_order",
                table: "category",
                column: "display_order");

            migrationBuilder.AddForeignKey(
                name: "FK_category_category_parent_category_id",
                table: "category",
                column: "parent_category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_master_category_category_id",
                table: "product_master",
                column: "category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_review_product_master_product_id",
                table: "product_review",
                column: "product_id",
                principalTable: "product_master",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_review_product_version_version_id",
                table: "product_review",
                column: "version_id",
                principalTable: "product_version",
                principalColumn: "version_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_product_version_product_master_product_id",
                table: "product_version",
                column: "product_id",
                principalTable: "product_master",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
