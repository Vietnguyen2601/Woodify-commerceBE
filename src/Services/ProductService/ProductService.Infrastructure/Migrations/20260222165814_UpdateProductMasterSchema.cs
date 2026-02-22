using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductMasterSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_master_global_sku",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "certified",
                table: "product_master");

            migrationBuilder.RenameColumn(
                name: "current_version_id",
                table: "product_master",
                newName: "moderated_by");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "product_master",
                type: "text",
                nullable: false,
                defaultValue: "DRAFT",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "global_sku",
                table: "product_master",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

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

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "img_url",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "moderated_at",
                table: "product_master",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moderation_notes",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moderation_status",
                table: "product_master",
                type: "text",
                nullable: false,
                defaultValue: "PENDING");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "product_master",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "published_at",
                table: "product_master",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "product_master",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sold_count",
                table: "product_master",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_global_sku",
                table: "product_master",
                column: "global_sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_moderation_status",
                table: "product_master",
                column: "moderation_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_master_global_sku",
                table: "product_master");

            migrationBuilder.DropIndex(
                name: "IX_product_master_moderation_status",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "ar_available",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "ar_model_url",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "description",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "img_url",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "moderated_at",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "moderation_notes",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "moderation_status",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "name",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "published_at",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "sold_count",
                table: "product_master");

            migrationBuilder.RenameColumn(
                name: "moderated_by",
                table: "product_master",
                newName: "current_version_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "product_master",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "DRAFT");

            migrationBuilder.AlterColumn<string>(
                name: "global_sku",
                table: "product_master",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "certified",
                table: "product_master",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_global_sku",
                table: "product_master",
                column: "global_sku");
        }
    }
}
