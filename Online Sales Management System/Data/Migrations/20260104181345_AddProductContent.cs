using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSalesManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Giữ lại cột Content
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            // 2. Giữ lại cột Description (Mới thêm)
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa Content khi rollback
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Products");

            // Xóa Description khi rollback
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");
        }
    }
}