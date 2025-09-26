using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicantManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteEntityConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Applicants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Applicants",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedReason",
                table: "Applicants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Applicants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Applicants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Applicants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Applicants",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "DeletedReason",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Applicants");
        }
    }
}
