using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToProductMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete existing product_master records to avoid FK constraint violation
            migrationBuilder.Sql("DELETE FROM product_master;");

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "product_master",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_product_master_category_id",
                table: "product_master",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_master_category_category_id",
                table: "product_master",
                column: "category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_master_category_category_id",
                table: "product_master");

            migrationBuilder.DropIndex(
                name: "IX_product_master_category_id",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "product_master");
        }
    }
}
