using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtaGreenTech.StockService.Infrastructure.Persistence.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseProductCatalog : Migration
    {
        // Mevcut ürün adları ve açıklamaları bu dildeki çeviriye taşınır (Localization:DefaultLanguage'ın ilk değeri).
        private const string ExistingNamesLanguage = "tr";

        // Para birimi eklenmeden önceki fiyatlar ve hareketler bu para birimindedir (CurrencyCode.Default).
        private const string ExistingPricesCurrency = "TRY";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Products",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: ExistingPricesCurrency);

            migrationBuilder.Sql("""ALTER TABLE "Products" ALTER COLUMN "Currency" DROP DEFAULT;""");

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql($"""
                INSERT INTO "ProductTranslations"
                    ("Id", "ProductId", "LanguageCode", "Name", "Description", "CreatedAt", "CreatedBy", "IsDeleted", "DeletedAt")
                SELECT gen_random_uuid(), "Id", '{ExistingNamesLanguage}', "Name", NULLIF(TRIM("Description"), ''), now(), "CreatedBy",
                       "IsDeleted", "DeletedAt"
                FROM "Products";
                """);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductId_LanguageCode",
                table: "ProductTranslations",
                columns: new[] { "ProductId", "LanguageCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Varsayılan dildeki ad ve açıklama geri taşınır; çeviri yoksa ad olarak SKU kullanılır.
            migrationBuilder.Sql($"""
                UPDATE "Products" AS p
                SET "Name" = COALESCE(t."Name", p."Sku"),
                    "Description" = t."Description"
                FROM "Products" AS p2
                LEFT JOIN LATERAL (
                    SELECT pt."Name", pt."Description" FROM "ProductTranslations" AS pt
                    WHERE pt."ProductId" = p2."Id" AND pt."LanguageCode" = '{ExistingNamesLanguage}' AND NOT pt."IsDeleted"
                    LIMIT 1
                ) AS t ON true
                WHERE p2."Id" = p."Id";

                ALTER TABLE "Products" ALTER COLUMN "Name" DROP DEFAULT;
                """);

            migrationBuilder.DropTable(
                name: "ProductTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Products");
        }
    }
}
