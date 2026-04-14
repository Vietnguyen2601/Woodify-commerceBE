using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ShopCountersLedgers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_order_counter_ledger",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_order_counter_ledger", x => x.order_id);
                });

            migrationBuilder.CreateTable(
                name: "shop_product_counter_ledger",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uncounted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_product_counter_ledger", x => x.product_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shop_order_counter_ledger_shop_id",
                table: "shop_order_counter_ledger",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_product_counter_ledger_shop_id",
                table: "shop_product_counter_ledger",
                column: "shop_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_order_counter_ledger");

            migrationBuilder.DropTable(
                name: "shop_product_counter_ledger");
        }
    }
}
