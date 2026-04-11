using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE orders SET provider_service_code = 'STD' WHERE UPPER(TRIM(provider_service_code)) = 'STANDARD';
                UPDATE orders SET provider_service_code = 'ECO' WHERE UPPER(TRIM(provider_service_code)) = 'ECONOMY';
                UPDATE orders SET provider_service_code = 'EXP' WHERE UPPER(TRIM(provider_service_code)) IN ('EXPRESS','SUPER_EXPRESS');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "provider_service_code",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "STD",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: false,
                oldDefaultValue: "STANDARD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "provider_service_code",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "STANDARD",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: false,
                oldDefaultValue: "STD");

            migrationBuilder.Sql("""
                UPDATE orders SET provider_service_code = 'STANDARD' WHERE UPPER(TRIM(provider_service_code)) = 'STD';
                UPDATE orders SET provider_service_code = 'ECONOMY' WHERE UPPER(TRIM(provider_service_code)) = 'ECO';
                UPDATE orders SET provider_service_code = 'EXPRESS' WHERE UPPER(TRIM(provider_service_code)) = 'EXP';
                """);
        }
    }
}
