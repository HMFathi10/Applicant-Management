using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicantManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedApplicantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAdress",
                table: "Applicants",
                newName: "EmailAddress");

            migrationBuilder.RenameIndex(
                name: "IX_Applicants_EmailAdress",
                table: "Applicants",
                newName: "IX_Applicants_EmailAddress");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Applicants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Hired",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Hired",
                table: "Applicants");

            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "Applicants",
                newName: "EmailAdress");

            migrationBuilder.RenameIndex(
                name: "IX_Applicants_EmailAddress",
                table: "Applicants",
                newName: "IX_Applicants_EmailAdress");
        }
    }
}
