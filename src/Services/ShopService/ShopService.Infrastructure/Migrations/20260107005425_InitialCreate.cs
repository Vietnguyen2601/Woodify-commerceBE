using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rating = table.Column<decimal>(type: "numeric", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    follower_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shops", x => x.shop_id);
                });

            migrationBuilder.CreateTable(
                name: "shop_followers",
                columns: table => new
                {
                    shop_follower_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_followers", x => x.shop_follower_id);
                    table.ForeignKey(
                        name: "FK_shop_followers_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "shop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shop_followers_shop_id",
                table: "shop_followers",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_shops_name",
                table: "shops",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_shops_owner_account_id",
                table: "shops",
                column: "owner_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_followers");

            migrationBuilder.DropTable(
                name: "shops");
        }
    }
}
