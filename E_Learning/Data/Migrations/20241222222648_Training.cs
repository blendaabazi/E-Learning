using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Learning.Data.Migrations
{
    /// <inheritdoc />
    public partial class Training : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecture_Trainings_TrainingId",
                table: "Lecture");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_AspNetUsers_ApplicationUserID",
                table: "Trainings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trainings",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Lecture");

            migrationBuilder.RenameTable(
                name: "Trainings",
                newName: "Training");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserID",
                table: "Training",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Trainings_ApplicationUserID",
                table: "Training",
                newName: "IX_Training_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training",
                table: "Training",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecture_Training_TrainingId",
                table: "Lecture",
                column: "TrainingId",
                principalTable: "Training",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Training_AspNetUsers_UserId",
                table: "Training",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecture_Training_TrainingId",
                table: "Lecture");

            migrationBuilder.DropForeignKey(
                name: "FK_Training_AspNetUsers_UserId",
                table: "Training");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Training",
                table: "Training");

            migrationBuilder.RenameTable(
                name: "Training",
                newName: "Trainings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Trainings",
                newName: "ApplicationUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Training_UserId",
                table: "Trainings",
                newName: "IX_Trainings_ApplicationUserID");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Lecture",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trainings",
                table: "Trainings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecture_Trainings_TrainingId",
                table: "Lecture",
                column: "TrainingId",
                principalTable: "Trainings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_AspNetUsers_ApplicationUserID",
                table: "Trainings",
                column: "ApplicationUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
