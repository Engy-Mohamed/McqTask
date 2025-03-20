using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class initiateDatabse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamProgress_Students_StudentId",
                table: "ExamProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_Students_StudentId1",
                table: "ResultRecords");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_ResultRecords_StudentId1",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "ResultRecords");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "ExamProgress",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamProgress_AspNetUsers_StudentId",
                table: "ExamProgress",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamProgress_AspNetUsers_StudentId",
                table: "ExamProgress");

            migrationBuilder.AddColumn<int>(
                name: "StudentId1",
                table: "ResultRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "ExamProgress",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecords_StudentId1",
                table: "ResultRecords",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GroupId",
                table: "Students",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamProgress_Students_StudentId",
                table: "ExamProgress",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_Students_StudentId1",
                table: "ResultRecords",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
