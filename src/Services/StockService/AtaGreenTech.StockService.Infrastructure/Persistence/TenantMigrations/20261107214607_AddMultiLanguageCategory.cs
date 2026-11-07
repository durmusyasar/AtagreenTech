using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.StockService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddMultiLanguageCategory : Migration
    {
        // Mevcut kategori adları ve açıklamaları bu dildeki çeviriye taşınır (Localization:DefaultLanguage'ın ilk değeri).
        private const string ExistingNamesLanguage = "tr";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryTranslations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql($"""
                INSERT INTO "CategoryTranslations"
                    ("Id", "CategoryId", "LanguageCode", "Name", "Description", "CreatedAt", "CreatedBy", "IsDeleted")
                SELECT gen_random_uuid(), "Id", '{ExistingNamesLanguage}', "Name", NULLIF(TRIM("Description"), ''), now(), "CreatedBy", "IsDeleted"
                FROM "Categories";
                """);

            // Kod addan türetilir (Damla Sulama -> DAMLA-SULAMA, Kuruyemiş -> KURUYEMIS); aynı koda düşen kategorilere
            // sıra numarası eklenir (SULAMA, SULAMA-2).
            migrationBuilder.Sql("""
                WITH base AS (
                    SELECT "Id", "IsDeleted", "CreatedAt",
                           COALESCE(NULLIF(TRIM(BOTH '-' FROM LEFT(REGEXP_REPLACE(
                               UPPER(TRANSLATE("Name", 'çğıöşüÇĞİÖŞÜ', 'cgiosuCGIOSU')), '[^A-Z0-9]+', '-', 'g'), 40)), ''),
                               'CATEGORY') AS code
                    FROM "Categories"
                ),
                numbered AS (
                    SELECT "Id", code,
                           ROW_NUMBER() OVER (PARTITION BY code ORDER BY "IsDeleted", "CreatedAt", "Id") AS rn
                    FROM base
                )
                UPDATE "Categories" AS c
                SET "Code" = CASE WHEN n.rn = 1 THEN n.code ELSE n.code || '-' || n.rn END
                FROM numbered AS n
                WHERE n."Id" = c."Id";

                ALTER TABLE "Categories" ALTER COLUMN "Code" DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Code",
                table: "Categories",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_CategoryId_LanguageCode",
                table: "CategoryTranslations",
                columns: new[] { "CategoryId", "LanguageCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Varsayılan dildeki ad ve açıklama geri taşınır; çeviri yoksa ad olarak kod kullanılır.
            migrationBuilder.Sql($"""
                UPDATE "Categories" AS c
                SET "Name" = COALESCE(LEFT(t."Name", 100), c."Code"),
                    "Description" = t."Description"
                FROM "Categories" AS c2
                LEFT JOIN LATERAL (
                    SELECT ct."Name", ct."Description" FROM "CategoryTranslations" AS ct
                    WHERE ct."CategoryId" = c2."Id" AND ct."LanguageCode" = '{ExistingNamesLanguage}' AND NOT ct."IsDeleted"
                    LIMIT 1
                ) AS t ON true
                WHERE c2."Id" = c."Id";
                """);

            migrationBuilder.DropTable(
                name: "CategoryTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Code",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Categories");
        }
    }
}
