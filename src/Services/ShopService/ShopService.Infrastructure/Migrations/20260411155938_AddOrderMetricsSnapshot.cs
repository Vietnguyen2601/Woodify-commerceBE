using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderMetricsSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderMetricsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalAmountCents = table.Column<long>(type: "bigint", nullable: false),
                    CommissionCents = table.Column<long>(type: "bigint", nullable: false),
                    NetAmountCents = table.Column<long>(type: "bigint", nullable: false),
                    OrderCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundAmountCents = table.Column<long>(type: "bigint", nullable: true),
                    RefundReason = table.Column<string>(type: "text", nullable: true),
                    IsReturn = table.Column<bool>(type: "boolean", nullable: false),
                    IsSLAViolated = table.Column<bool>(type: "boolean", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderYear = table.Column<int>(type: "integer", nullable: false),
                    OrderMonth = table.Column<int>(type: "integer", nullable: false),
                    OrderDay = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMetricsSnapshots", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderMetricsSnapshots");
        }
    }
}
