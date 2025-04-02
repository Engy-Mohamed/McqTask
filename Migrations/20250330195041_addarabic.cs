using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McqTask.Migrations
{
    /// <inheritdoc />
    public partial class addarabic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptDate",
                table: "ResultRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ArabicText",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArabicText",
                table: "Options",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArabicLeftSideText",
                table: "MatchingPairs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArabicRightSideText",
                table: "MatchingPairs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfAttempts",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptDate",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "ArabicText",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ArabicText",
                table: "Options");

            migrationBuilder.DropColumn(
                name: "ArabicLeftSideText",
                table: "MatchingPairs");

            migrationBuilder.DropColumn(
                name: "ArabicRightSideText",
                table: "MatchingPairs");

            migrationBuilder.DropColumn(
                name: "NumberOfAttempts",
                table: "Exams");
        }
    }
}
