using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePropDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "follower_count",
                table: "shops");

            migrationBuilder.RenameColumn(
                name: "updatedat",
                table: "shops",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "shops",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "shop_followers",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "shops",
                type: "text",
                nullable: false,
                defaultValue: "INACTIVE",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "rating",
                table: "shops",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "shops",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "shops",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "shops",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "cover_image_url",
                table: "shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_pickup_address_id",
                table: "shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "default_provider",
                table: "shops",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "review_count",
                table: "shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_orders",
                table: "shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_products",
                table: "shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cover_image_url",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "default_pickup_address_id",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "default_provider",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "review_count",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "total_orders",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "total_products",
                table: "shops");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "shops",
                newName: "updatedat");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "shops",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "shop_followers",
                newName: "createdat");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "shops",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "INACTIVE");

            migrationBuilder.AlterColumn<decimal>(
                name: "rating",
                table: "shops",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "shops",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "shops",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updatedat",
                table: "shops",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "follower_count",
                table: "shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
