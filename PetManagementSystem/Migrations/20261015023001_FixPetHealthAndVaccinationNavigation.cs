using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixPetHealthAndVaccinationNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "pet_images",
                newName: "uploaded_at");

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "pet_images",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "uploaded_at",
                table: "pet_images",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "pet_images",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
