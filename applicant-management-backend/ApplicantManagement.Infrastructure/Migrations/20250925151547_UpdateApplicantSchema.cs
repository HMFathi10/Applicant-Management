using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicantManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicantSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Applicants_CreatedDate",
                table: "Applicants",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Hired",
                table: "Applicants",
                column: "Hired");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_IsDeleted",
                table: "Applicants",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_IsDeleted_CreatedDate",
                table: "Applicants",
                columns: new[] { "IsDeleted", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applicants_CreatedDate",
                table: "Applicants");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_Hired",
                table: "Applicants");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_IsDeleted",
                table: "Applicants");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_IsDeleted_CreatedDate",
                table: "Applicants");
        }
    }
}
