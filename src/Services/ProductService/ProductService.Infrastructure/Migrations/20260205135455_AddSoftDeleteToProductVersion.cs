using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToProductVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_version_sku",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_master_global_sku",
                table: "product_master");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "product_version",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "product_version",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.CreateIndex(
                name: "IX_product_version_sku",
                table: "product_version",
                column: "sku");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_global_sku",
                table: "product_master",
                column: "global_sku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_version_sku",
                table: "product_version");

            migrationBuilder.DropIndex(
                name: "IX_product_master_global_sku",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "product_version");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "product_version");

            migrationBuilder.AlterColumn<string>(
                name: "global_sku",
                table: "product_master",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_product_version_sku",
                table: "product_version",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_global_sku",
                table: "product_master",
                column: "global_sku",
                unique: true);
        }
    }
}
