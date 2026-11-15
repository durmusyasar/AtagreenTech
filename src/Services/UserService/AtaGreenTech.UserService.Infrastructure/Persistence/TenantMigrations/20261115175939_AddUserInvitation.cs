using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.UserService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddUserInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvitationTokenExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitationTokenHash",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InvitationTokenHash",
                table: "Users",
                column: "InvitationTokenHash",
                unique: true,
                filter: "\"InvitationTokenHash\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_InvitationTokenHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InvitationTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InvitationTokenHash",
                table: "Users");
        }
    }
}
