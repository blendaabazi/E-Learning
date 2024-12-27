using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Learning.Data.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Training_AspNetUsers_UserId",
                table: "Training");

            migrationBuilder.DropIndex(
                name: "IX_Training_UserId",
                table: "Training");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Training");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Lecture",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Lecture");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Training",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Training_UserId",
                table: "Training",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Training_AspNetUsers_UserId",
                table: "Training",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
