using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyOrderSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_order_code",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "base_price_cents",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "bulky_type",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "bundle_discount_cents",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_bundle",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "is_fragile",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "low_stock_threshold",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "price_cents",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "primary_image_url",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "requires_special_handling",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "version_number",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "volume_cm3",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "warranty_months",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "warranty_terms",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "cancel_reason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_email",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_phone",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivered_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_cents",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "order_code",
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
                name: "placed_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipped_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_fee_cents",
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
                name: "product_name",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "refunded_amount_cents",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "returned_quantity",
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
                name: "updated_at",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "added_at",
                table: "cart_items");

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
                name: "unit_price_cents",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "allow_backorder",
                table: "product_version_cache",
                newName: "AllowBackorder");

            migrationBuilder.RenameColumn(
                name: "voucher_applied",
                table: "orders",
                newName: "voucher_id");

            migrationBuilder.RenameColumn(
                name: "cancelled_by",
                table: "orders",
                newName: "payment");

            migrationBuilder.AlterColumn<bool>(
                name: "AllowBackorder",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "price",
                table: "product_version_cache",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "total_amount_cents",
                table: "orders",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "subtotal_cents",
                table: "orders",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

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
                name: "delivery_address_id",
                table: "orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "account_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "tax_cents",
                table: "order_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);

            migrationBuilder.AlterColumn<double>(
                name: "line_total_cents",
                table: "order_items",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<double>(
                name: "price",
                table: "cart_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "product_version_cache");

            migrationBuilder.DropColumn(
                name: "price",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "AllowBackorder",
                table: "product_version_cache",
                newName: "allow_backorder");

            migrationBuilder.RenameColumn(
                name: "voucher_id",
                table: "orders",
                newName: "voucher_applied");

            migrationBuilder.RenameColumn(
                name: "payment",
                table: "orders",
                newName: "cancelled_by");

            migrationBuilder.AlterColumn<bool>(
                name: "allow_backorder",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<long>(
                name: "base_price_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bulky_type",
                table: "product_version_cache",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "bundle_discount_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_bundle",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_fragile",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "low_stock_threshold",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<long>(
                name: "price_cents",
                table: "product_version_cache",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "primary_image_url",
                table: "product_version_cache",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_special_handling",
                table: "product_version_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "version_number",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "volume_cm3",
                table: "product_version_cache",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "warranty_months",
                table: "product_version_cache",
                type: "integer",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<string>(
                name: "warranty_terms",
                table: "product_version_cache",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "total_amount_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "subtotal_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<Guid>(
                name: "shop_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "delivery_address_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<string>(
                name: "customer_email",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_note",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_phone",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "delivered_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "discount_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "order_code",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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
                name: "placed_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "shipped_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "shipping_fee_cents",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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

            migrationBuilder.AlterColumn<long>(
                name: "tax_cents",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<long>(
                name: "line_total_cents",
                table: "order_items",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "product_name",
                table: "order_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "refunded_amount_cents",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "returned_quantity",
                table: "order_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "carts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "added_at",
                table: "cart_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<long>(
                name: "unit_price_cents",
                table: "cart_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "cart_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_code",
                table: "orders",
                column: "order_code",
                unique: true);
        }
    }
}
