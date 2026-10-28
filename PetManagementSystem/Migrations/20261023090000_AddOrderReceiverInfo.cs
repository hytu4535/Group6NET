using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReceiverInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: ứng dụng cũng tự đảm bảo 2 cột này khi khởi động (Program.cs) và có script Database/AddOrderReceiverInfo.sql.
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.orders', 'receiver_name') IS NULL ALTER TABLE dbo.orders ADD receiver_name NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.orders', 'receiver_email') IS NULL ALTER TABLE dbo.orders ADD receiver_email NVARCHAR(150) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.orders', 'receiver_name') IS NOT NULL ALTER TABLE dbo.orders DROP COLUMN receiver_name;
IF COL_LENGTH('dbo.orders', 'receiver_email') IS NOT NULL ALTER TABLE dbo.orders DROP COLUMN receiver_email;");
        }
    }
}
