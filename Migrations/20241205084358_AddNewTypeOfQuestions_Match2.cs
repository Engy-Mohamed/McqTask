using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTypeOfQuestions_Match2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchingPair",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    LeftSideText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RightSideText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingPair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchingPair_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchingPair_QuestionId",
                table: "MatchingPair",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchingPair");
        }
    }
}
