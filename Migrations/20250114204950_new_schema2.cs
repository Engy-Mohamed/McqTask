using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class new_schema2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Category_CategoryId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Exams_ExamId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchingPair_Questions_QuestionId",
                table: "MatchingPair");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecord_Exams_ExamId",
                table: "ResultRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecord_Students_StudentId",
                table: "ResultRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Exams_ExamId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Group_GroupId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ExamId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ExamId",
                table: "Exams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResultRecord",
                table: "ResultRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchingPair",
                table: "MatchingPair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "Exams");

            migrationBuilder.RenameTable(
                name: "ResultRecord",
                newName: "ResultRecords");

            migrationBuilder.RenameTable(
                name: "MatchingPair",
                newName: "MatchingPairs");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "Groups");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_ResultRecord_StudentId",
                table: "ResultRecords",
                newName: "IX_ResultRecords_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ResultRecord_ExamId",
                table: "ResultRecords",
                newName: "IX_ResultRecords_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchingPair_QuestionId",
                table: "MatchingPairs",
                newName: "IX_MatchingPairs_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResultRecords",
                table: "ResultRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchingPairs",
                table: "MatchingPairs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Categories_CategoryId",
                table: "Exams",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchingPairs_Questions_QuestionId",
                table: "MatchingPairs",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_Exams_ExamId",
                table: "ResultRecords",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_Students_StudentId",
                table: "ResultRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Groups_GroupId",
                table: "Students",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Categories_CategoryId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchingPairs_Questions_QuestionId",
                table: "MatchingPairs");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_Exams_ExamId",
                table: "ResultRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_Students_StudentId",
                table: "ResultRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Groups_GroupId",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResultRecords",
                table: "ResultRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchingPairs",
                table: "MatchingPairs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "ResultRecords",
                newName: "ResultRecord");

            migrationBuilder.RenameTable(
                name: "MatchingPairs",
                newName: "MatchingPair");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Group");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_ResultRecords_StudentId",
                table: "ResultRecord",
                newName: "IX_ResultRecord_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ResultRecords_ExamId",
                table: "ResultRecord",
                newName: "IX_ResultRecord_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchingPairs_QuestionId",
                table: "MatchingPair",
                newName: "IX_MatchingPair_QuestionId");

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResultRecord",
                table: "ResultRecord",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchingPair",
                table: "MatchingPair",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ExamId",
                table: "Students",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamId",
                table: "Exams",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Category_CategoryId",
                table: "Exams",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Exams_ExamId",
                table: "Exams",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchingPair_Questions_QuestionId",
                table: "MatchingPair",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecord_Exams_ExamId",
                table: "ResultRecord",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecord_Students_StudentId",
                table: "ResultRecord",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Exams_ExamId",
                table: "Students",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Group_GroupId",
                table: "Students",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
