using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopASP.Migrations
{
    /// <inheritdoc />
    public partial class CategoryPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "Categories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_PhotoId",
                table: "Categories",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Photos_PhotoId",
                table: "Categories",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Photos_PhotoId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_PhotoId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "Categories");
        }
    }
}
