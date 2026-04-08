using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_cache",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    shop_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shop_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    shop_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    default_pickup_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    default_provider = table.Column<Guid>(type: "uuid", nullable: true),
                    default_provider_service_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_cache", x => x.shop_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_cache_is_deleted",
                table: "shop_cache",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_shop_cache_owner_account_id",
                table: "shop_cache",
                column: "owner_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_cache");
        }
    }
}
