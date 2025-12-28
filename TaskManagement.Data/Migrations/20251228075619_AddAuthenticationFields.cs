using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_Name",
                table: "Tags");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserDetails",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "UserDetails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "UserDetails",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserDetailsId",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name_UserId",
                table: "Tags",
                columns: new[] { "Name", "UserDetailsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserDetailsId",
                table: "Tags",
                column: "UserDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_UserDetails_UserDetailsId",
                table: "Tags",
                column: "UserDetailsId",
                principalTable: "UserDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_UserDetails_UserDetailsId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Name_UserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserDetailsId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "UserDetailsId",
                table: "Tags");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }
    }
}
