using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicantManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Countries",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Countries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterviewDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "Applicants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_YearsOfExperience",
                table: "Applicants",
                column: "YearsOfExperience");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_YearsOfExperience",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LastInterviewDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Applicants");
        }
    }
}
