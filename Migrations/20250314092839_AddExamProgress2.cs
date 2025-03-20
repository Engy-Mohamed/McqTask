using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class AddExamProgress2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId1",
                table: "ResultRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    CurrentQuestionIndex = table.Column<int>(type: "int", nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchingAnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlaggedQuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeLeftInSeconds = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamProgress_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamProgress_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecords_StudentId1",
                table: "ResultRecords",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_ExamProgress_ExamId",
                table: "ExamProgress",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamProgress_StudentId",
                table: "ExamProgress",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GroupId",
                table: "Students",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_Students_StudentId1",
                table: "ResultRecords",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_Students_StudentId1",
                table: "ResultRecords");

            migrationBuilder.DropTable(
                name: "ExamProgress");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_ResultRecords_StudentId1",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "ResultRecords");
        }
    }
}
