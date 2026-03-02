using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Tạo bảng provider_services trước (được reference bởi shipments) ──
            migrationBuilder.CreateTable(
                name: "provider_services",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_code = table.Column<string>(type: "text", nullable: false),
                    provider_name = table.Column<string>(type: "text", nullable: false),
                    provider_logo_url = table.Column<string>(type: "text", nullable: true),
                    service_code = table.Column<string>(type: "text", nullable: true),
                    service_name = table.Column<string>(type: "text", nullable: false),
                    speed_level = table.Column<string>(type: "text", nullable: true),
                    estimated_delivery_days = table.Column<int>(type: "integer", nullable: true),
                    limitations = table.Column<string>(type: "text", nullable: true),
                    zone_config = table.Column<string>(type: "text", nullable: true),
                    pricing_rules = table.Column<string>(type: "text", nullable: true),
                    platform_fee_config = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    priority_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_services", x => x.service_id);
                });

            // ── Tạo bảng shipments ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_code = table.Column<string>(type: "text", nullable: false),
                    provider_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tracking_number = table.Column<string>(type: "text", nullable: true),
                    pickup_address_id = table.Column<string>(type: "text", nullable: true),
                    delivery_address_id = table.Column<string>(type: "text", nullable: true),
                    total_weight_grams = table.Column<double>(type: "double precision", nullable: false),
                    total_volume_cm3 = table.Column<double>(type: "double precision", nullable: true),
                    package_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    bulky_type = table.Column<string>(type: "text", nullable: false, defaultValue: "Normal"),
                    is_fragile = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    requires_insurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    insurance_fee_cents = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    final_shipping_fee_cents = table.Column<long>(type: "bigint", nullable: false),
                    is_free_shipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    pickup_scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_estimated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    cancel_reason = table.Column<string>(type: "text", nullable: true),
                    customer_note = table.Column<string>(type: "text", nullable: true),
                    internal_note = table.Column<string>(type: "text", nullable: true),
                    delivery_instruction = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.shipment_id);
                    table.ForeignKey(
                        name: "FK_shipments_provider_services_provider_service_id",
                        column: x => x.provider_service_id,
                        principalTable: "provider_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.SetNull);
                });

            // ── Indexes ──────────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_shipments_shipment_code",
                table: "shipments",
                column: "shipment_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipments_tracking_number",
                table: "shipments",
                column: "tracking_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipments_provider_service_id",
                table: "shipments",
                column: "provider_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_order_id",
                table: "shipments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_status",
                table: "shipments",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.DropTable(
                name: "provider_services");
        }
    }
}
