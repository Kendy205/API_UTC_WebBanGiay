using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanHang.Data.Migrations
{
    /// <inheritdoc />
    public partial class addImgpublicidProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "img_public_id",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "img_public_id",
                table: "products");
        }
    }
}
