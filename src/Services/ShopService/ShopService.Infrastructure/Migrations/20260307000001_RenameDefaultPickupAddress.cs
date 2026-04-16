using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultPickupAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent rename: chỉ đổi tên nếu cột cũ vẫn còn tồn tại
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'shops' AND column_name = 'default_pickup_address_id'
                    ) THEN
                        ALTER TABLE shops RENAME COLUMN default_pickup_address_id TO default_pickup_address;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'shops' AND column_name = 'default_pickup_address'
                    ) THEN
                        ALTER TABLE shops RENAME COLUMN default_pickup_address TO default_pickup_address_id;
                    END IF;
                END $$;
            ");
        }
    }
}
