using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileService.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_visit",
                table: "profiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "status_online",
                table: "profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "email",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "last_visit",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "status_online",
                table: "profiles");
        }
    }
}
