using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.UserService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddUserMfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMfaEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecoveryCodeHashesJson",
                table: "Users",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMfaEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RecoveryCodeHashesJson",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "Users");
        }
    }
}
