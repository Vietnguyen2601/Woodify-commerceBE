using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_review",
                columns: table => new
                {
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    helpful_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_review", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_product_review_product_master_product_id",
                        column: x => x.product_id,
                        principalTable: "product_master",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_review_account_id",
                table: "product_review",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_created_at",
                table: "product_review",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_is_verified",
                table: "product_review",
                column: "is_verified");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_order_id",
                table: "product_review",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_review_order_id_account_id",
                table: "product_review",
                columns: new[] { "order_id", "account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_review_product_id",
                table: "product_review",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_review");
        }
    }
}
