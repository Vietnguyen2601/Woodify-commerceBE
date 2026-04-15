using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccountDirectoryRenameUsernameToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "account_directory",
                newName: "name");

            migrationBuilder.RenameIndex(
                name: "IX_account_directory_username",
                table: "account_directory",
                newName: "IX_account_directory_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "account_directory",
                newName: "username");

            migrationBuilder.RenameIndex(
                name: "IX_account_directory_name",
                table: "account_directory",
                newName: "IX_account_directory_username");
        }
    }
}
