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
            migrationBuilder.CreateTable(
                name: "Shipping_Providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    support_phone = table.Column<string>(type: "text", nullable: true),
                    support_email = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipping_Providers", x => x.provider_id);
                });

            migrationBuilder.CreateTable(
                name: "Provider_Services",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    speed_level = table.Column<string>(type: "text", nullable: true),
                    estimated_days_min = table.Column<int>(type: "integer", nullable: true),
                    estimated_days_max = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    multiplier_fee = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider_Services", x => x.service_id);
                    table.ForeignKey(
                        name: "FK_Provider_Services_Shipping_Providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "Shipping_Providers",
                        principalColumn: "provider_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pickup_address_id = table.Column<string>(type: "text", nullable: true),
                    delivery_address_id = table.Column<string>(type: "text", nullable: true),
                    total_weight_grams = table.Column<double>(type: "double precision", nullable: false),
                    bulky_type = table.Column<string>(type: "text", nullable: true),
                    final_shipping_fee_cents = table.Column<long>(type: "bigint", nullable: false),
                    is_free_shipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    pickup_scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_estimated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "PENDING"),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    cancel_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.shipment_id);
                    table.ForeignKey(
                        name: "FK_Shipments_Provider_Services_provider_service_id",
                        column: x => x.provider_service_id,
                        principalTable: "Provider_Services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Provider_Services_provider_id",
                table: "Provider_Services",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_provider_service_id",
                table: "Shipments",
                column: "provider_service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "Provider_Services");

            migrationBuilder.DropTable(
                name: "Shipping_Providers");
        }
    }
}
