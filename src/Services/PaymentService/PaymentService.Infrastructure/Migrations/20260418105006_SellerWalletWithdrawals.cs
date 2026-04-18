using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SellerWalletWithdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallet_account_id",
                table: "Wallet");

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "Wallet_Transaction",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_type",
                table: "Wallet_Transaction",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "related_shop_id",
                table: "Wallet_Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wallet_kind",
                table: "Wallet",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Buyer");

            migrationBuilder.CreateTable(
                name: "withdrawal_ticket",
                columns: table => new
                {
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_vnd = table.Column<long>(type: "bigint", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bank_account_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    bank_account_holder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reviewed_by_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawal_ticket", x => x.ticket_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_Transaction_idempotency_key",
                table: "Wallet_Transaction",
                column: "idempotency_key",
                unique: true,
                filter: "\"idempotency_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_account_id_wallet_kind",
                table: "Wallet",
                columns: new[] { "account_id", "wallet_kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_withdrawal_ticket_seller_account_id",
                table: "withdrawal_ticket",
                column: "seller_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawal_ticket_shop_id",
                table: "withdrawal_ticket",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawal_ticket_status",
                table: "withdrawal_ticket",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "withdrawal_ticket");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_Transaction_idempotency_key",
                table: "Wallet_Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_account_id_wallet_kind",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "Wallet_Transaction");

            migrationBuilder.DropColumn(
                name: "reference_type",
                table: "Wallet_Transaction");

            migrationBuilder.DropColumn(
                name: "related_shop_id",
                table: "Wallet_Transaction");

            migrationBuilder.DropColumn(
                name: "wallet_kind",
                table: "Wallet");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_account_id",
                table: "Wallet",
                column: "account_id");
        }
    }
}
