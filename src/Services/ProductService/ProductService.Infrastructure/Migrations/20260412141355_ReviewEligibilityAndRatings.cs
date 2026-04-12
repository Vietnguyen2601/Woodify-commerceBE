using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewEligibilityAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_reviews_order_id_account_id",
                table: "product_reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "order_item_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "average_rating",
                table: "product_master",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "review_count",
                table: "product_master",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill denormalized columns; assign synthetic order_item_id per row so the new unique index applies to legacy data.
            migrationBuilder.Sql("""
                UPDATE product_reviews pr
                SET product_id = pv.product_id
                FROM product_versions pv
                WHERE pr.version_id = pv.version_id;

                UPDATE product_reviews
                SET order_item_id = gen_random_uuid()
                WHERE order_item_id = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateTable(
                name: "review_purchase_eligibility",
                columns: table => new
                {
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eligible_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_consumed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_purchase_eligibility", x => x.order_item_id);
                });

            migrationBuilder.CreateTable(
                name: "shop_rating_stats",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    average_rating = table.Column<double>(type: "double precision", nullable: true),
                    review_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_rating_stats", x => x.shop_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_order_item_id_account_id",
                table: "product_reviews",
                columns: new[] { "order_item_id", "account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_product_id",
                table: "product_reviews",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_purchase_eligibility_account_id",
                table: "review_purchase_eligibility",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_purchase_eligibility_order_id",
                table: "review_purchase_eligibility",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_purchase_eligibility_product_id",
                table: "review_purchase_eligibility",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_purchase_eligibility");

            migrationBuilder.DropTable(
                name: "shop_rating_stats");

            migrationBuilder.DropIndex(
                name: "IX_product_reviews_order_item_id_account_id",
                table: "product_reviews");

            migrationBuilder.DropIndex(
                name: "IX_product_reviews_product_id",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "order_item_id",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "average_rating",
                table: "product_master");

            migrationBuilder.DropColumn(
                name: "review_count",
                table: "product_master");

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_order_id_account_id",
                table: "product_reviews",
                columns: new[] { "order_id", "account_id" },
                unique: true);
        }
    }
}
