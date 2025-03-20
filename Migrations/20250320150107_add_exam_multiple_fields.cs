using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class add_exam_multiple_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPracticeMode",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "IsPracticeMode",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Exams");
        }
    }
}
