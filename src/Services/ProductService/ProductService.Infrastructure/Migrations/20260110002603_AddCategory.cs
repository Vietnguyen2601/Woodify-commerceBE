using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_category_category_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_is_active",
                table: "category",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_category_name",
                table: "category",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_category_parent_category_id",
                table: "category",
                column: "parent_category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category");
        }
    }
}
