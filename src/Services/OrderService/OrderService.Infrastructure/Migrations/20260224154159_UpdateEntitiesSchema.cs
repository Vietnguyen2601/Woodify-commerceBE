using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_items_product_version_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "shipping_address",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "sku_code",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "qty",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "sku_code",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "title",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "order_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "returned_qty",
                table: "order_items",
                newName: "returned_quantity");

            migrationBuilder.RenameColumn(
                name: "qty",
                table: "order_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "product_version_id",
                table: "order_items",
                newName: "shipment_id");

            migrationBuilder.RenameColumn(
                name: "fulfillment_status",
                table: "order_items",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "product_version_id",
                table: "cart_items",
                newName: "version_id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "cart_items",
                newName: "added_at");

            migrationBuilder.RenameIndex(
                name: "IX_cart_items_product_version_id",
                table: "cart_items",
                newName: "IX_cart_items_version_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "shop_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "customer_phone",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "customer_name",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "account_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "cancel_reason",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cancelled_by",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_email",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_note",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "delivered_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "delivery_address_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "discount_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PENDING");

            migrationBuilder.AddColumn<string>(
                name: "payment_transaction_id",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "shipped_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shop_name",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "shop_note",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tax_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "voucher_applied",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "discount_cents",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "refunded_amount_cents",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "seller_sku",
                table: "order_items",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "shipping_info",
                table: "order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "variant_name",
                table: "order_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "version_id",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "compare_at_price_cents",
                table: "cart_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customization_note",
                table: "cart_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "cart_items",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_selected",
                table: "cart_items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "cart_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "shop_id",
                table: "cart_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_order_items_version_id",
                table: "order_items",
                column: "version_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_items_version_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "cancel_reason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "cancelled_by",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_email",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivered_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_address_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_cents",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_transaction_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipped_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shop_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shop_note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_cents",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "voucher_applied",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_cents",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "refunded_amount_cents",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "seller_sku",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "shipping_info",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "variant_name",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "version_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "compare_at_price_cents",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "customization_note",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "is_selected",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "shop_id",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "order_items",
                newName: "fulfillment_status");

            migrationBuilder.RenameColumn(
                name: "shipment_id",
                table: "order_items",
                newName: "product_version_id");

            migrationBuilder.RenameColumn(
                name: "returned_quantity",
                table: "order_items",
                newName: "returned_qty");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "order_items",
                newName: "qty");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "order_items",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "version_id",
                table: "cart_items",
                newName: "product_version_id");

            migrationBuilder.RenameColumn(
                name: "added_at",
                table: "cart_items",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_cart_items_version_id",
                table: "cart_items",
                newName: "IX_cart_items_product_version_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "PENDING");

            migrationBuilder.AlterColumn<Guid>(
                name: "shop_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "customer_phone",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "customer_name",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<Guid>(
                name: "account_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipping_address",
                table: "orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sku_code",
                table: "order_items",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                table: "carts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "qty",
                table: "cart_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "sku_code",
                table: "cart_items",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "cart_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_version_id",
                table: "order_items",
                column: "product_version_id");
        }
    }
}
