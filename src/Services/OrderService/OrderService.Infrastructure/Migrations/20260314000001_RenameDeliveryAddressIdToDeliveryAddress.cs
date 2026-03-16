using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeliveryAddressIdToDeliveryAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old column with wrong name
            migrationBuilder.DropColumn(
                name: "delivery_address_id",
                table: "orders");

            // Add the new column with correct name and type (text, not uuid)
            migrationBuilder.AddColumn<string>(
                name: "delivery_address",
                table: "orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: drop new column
            migrationBuilder.DropColumn(
                name: "delivery_address",
                table: "orders");

            // Rollback: restore old column
            migrationBuilder.AddColumn<string>(
                name: "delivery_address_id",
                table: "orders",
                type: "text",
                nullable: true);
        }
    }
}
