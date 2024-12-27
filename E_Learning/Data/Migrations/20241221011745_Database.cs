using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Learning.Data.Migrations
{
    /// <inheritdoc />
    public partial class Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.AddColumn<int>(
                name: "TrainingId",
                table: "Lecture",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lecture_TrainingId",
                table: "Lecture",
                column: "TrainingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecture_Trainings_TrainingId",
                table: "Lecture",
                column: "TrainingId",
                principalTable: "Trainings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecture_Trainings_TrainingId",
                table: "Lecture");

            migrationBuilder.DropIndex(
                name: "IX_Lecture_TrainingId",
                table: "Lecture");

            migrationBuilder.DropColumn(
                name: "TrainingId",
                table: "Lecture");

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LectureId = table.Column<int>(type: "int", nullable: false),
                    TrainingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Course_Lecture_LectureId",
                        column: x => x.LectureId,
                        principalTable: "Lecture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Course_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_LectureId",
                table: "Course",
                column: "LectureId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_TrainingId",
                table: "Course",
                column: "TrainingId");
        }
    }
}
