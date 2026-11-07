using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.StockService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddMultiLanguageUnitOfMeasure : Migration
    {
        // Mevcut birim adları bu dildeki çeviriye taşınır (Localization:DefaultLanguage'ın ilk değeri).
        private const string ExistingNamesLanguage = "tr";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut birimler aktif kalır.
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UnitsOfMeasure",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureTranslations_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureTranslations_UnitOfMeasureId_LanguageCode",
                table: "UnitOfMeasureTranslations",
                columns: new[] { "UnitOfMeasureId", "LanguageCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            // Ad, varsayılan dildeki çeviriye; kod, küçük harfle sembole taşınır (ör. KG -> kg). Diğer dillerin
            // çevirileri birim güncellenirken eklenir.
            migrationBuilder.Sql($"""
                INSERT INTO "UnitOfMeasureTranslations"
                    ("Id", "UnitOfMeasureId", "LanguageCode", "Name", "Symbol", "CreatedAt", "CreatedBy", "IsDeleted")
                SELECT gen_random_uuid(), "Id", '{ExistingNamesLanguage}', "Name", LEFT(LOWER("Code"), 20), now(), "CreatedBy", "IsDeleted"
                FROM "UnitsOfMeasure";
                """);

            migrationBuilder.DropColumn(
                name: "Name",
                table: "UnitsOfMeasure");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UnitsOfMeasure",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Varsayılan dildeki ad geri taşınır; yoksa kod kullanılır.
            migrationBuilder.Sql($"""
                UPDATE "UnitsOfMeasure" AS u
                SET "Name" = COALESCE(
                    (SELECT LEFT(t."Name", 50) FROM "UnitOfMeasureTranslations" AS t
                     WHERE t."UnitOfMeasureId" = u."Id" AND t."LanguageCode" = '{ExistingNamesLanguage}' AND NOT t."IsDeleted"
                     LIMIT 1),
                    u."Code");
                """);

            migrationBuilder.DropTable(
                name: "UnitOfMeasureTranslations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UnitsOfMeasure");
        }
    }
}
