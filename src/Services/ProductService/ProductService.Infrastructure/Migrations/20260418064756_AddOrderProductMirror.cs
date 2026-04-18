using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProductMirror : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_product_mirror",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    subtotal_vnd = table.Column<double>(type: "double precision", nullable: false),
                    total_amount_vnd = table.Column<double>(type: "double precision", nullable: false),
                    commission_vnd = table.Column<long>(type: "bigint", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_address = table.Column<string>(type: "text", nullable: true),
                    provider_service_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_snapshot_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    line_items_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_product_mirror", x => x.order_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_product_mirror_account_id",
                table: "order_product_mirror",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_product_mirror_last_snapshot_at",
                table: "order_product_mirror",
                column: "last_snapshot_at");

            migrationBuilder.CreateIndex(
                name: "IX_order_product_mirror_shop_id",
                table: "order_product_mirror",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_product_mirror_status",
                table: "order_product_mirror",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_product_mirror");
        }
    }
}
