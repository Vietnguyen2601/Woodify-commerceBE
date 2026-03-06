using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "support_phone",
                table: "Shipping_Providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "support_email",
                table: "Shipping_Providers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Shipping_Providers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Shipments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "DRAFT",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "PENDING");

            migrationBuilder.AlterColumn<string>(
                name: "failure_reason",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cancel_reason",
                table: "Shipments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bulky_type",
                table: "Shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "speed_level",
                table: "Provider_Services",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Provider_Services",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "Provider_Services",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "UQ_Shipping_Providers_name",
                table: "Shipping_Providers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_order_id",
                table: "Shipments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Shipments_tracking_number",
                table: "Shipments",
                column: "tracking_number",
                unique: true,
                filter: "tracking_number IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Shipments_bulky_type",
                table: "Shipments",
                sql: "bulky_type IS NULL OR bulky_type IN ('NORMAL','BULKY','SUPER_BULKY')");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Shipments_status",
                table: "Shipments",
                sql: "status IN ('DRAFT','PENDING','PICKUP_SCHEDULED','PICKED_UP','IN_TRANSIT','OUT_FOR_DELIVERY','DELIVERED','DELIVERY_FAILED','RETURNING','RETURNED','CANCELLED')");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Provider_Services_code",
                table: "Provider_Services",
                sql: "code IN ('ECO','STD','EXP','SUP')");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Provider_Services_speed_level",
                table: "Provider_Services",
                sql: "speed_level IS NULL OR speed_level IN ('ECONOMY','STANDARD','EXPRESS','SUPER_EXPRESS')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Shipping_Providers_name",
                table: "Shipping_Providers");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_order_id",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "UQ_Shipments_tracking_number",
                table: "Shipments");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Shipments_bulky_type",
                table: "Shipments");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Shipments_status",
                table: "Shipments");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Provider_Services_code",
                table: "Provider_Services");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Provider_Services_speed_level",
                table: "Provider_Services");

            migrationBuilder.AlterColumn<string>(
                name: "support_phone",
                table: "Shipping_Providers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "support_email",
                table: "Shipping_Providers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Shipping_Providers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Shipments",
                type: "text",
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "DRAFT");

            migrationBuilder.AlterColumn<string>(
                name: "failure_reason",
                table: "Shipments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cancel_reason",
                table: "Shipments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bulky_type",
                table: "Shipments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "speed_level",
                table: "Provider_Services",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Provider_Services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "Provider_Services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
