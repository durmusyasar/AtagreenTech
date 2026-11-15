using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.UserService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddSessionDeviceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceInfo",
                table: "RefreshTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "RefreshTokens");
        }
    }
}
