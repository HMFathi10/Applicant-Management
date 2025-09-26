using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicantManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_Status",
                table: "Applicants");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_YearsOfExperience",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LastInterviewDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Applicants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Applicants",
                type: "nvarchar(max)",
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

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Applicants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Applicants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Applicants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "Applicants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Status",
                table: "Applicants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_YearsOfExperience",
                table: "Applicants",
                column: "YearsOfExperience");
        }
    }
}
