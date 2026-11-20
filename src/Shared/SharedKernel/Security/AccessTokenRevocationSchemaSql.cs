namespace AtaGreenTech.SharedKernel.Security;

/// <summary>Master DB access-token iptal / stamp tabloları (migrate-master Ensure).</summary>
public static class AccessTokenRevocationSchemaSql
{
    public const string EnsureTables =
        """
        CREATE TABLE IF NOT EXISTS "AccessTokenRevocations" (
            "Key" character varying(128) PRIMARY KEY,
            "ExpiresAtUtc" timestamp with time zone NOT NULL
        );
        CREATE INDEX IF NOT EXISTS "IX_AccessTokenRevocations_ExpiresAtUtc"
            ON "AccessTokenRevocations" ("ExpiresAtUtc");

        CREATE TABLE IF NOT EXISTS "UserAccessStamps" (
            "TenantId" uuid NOT NULL,
            "UserId" uuid NOT NULL,
            "SecurityStamp" character varying(64) NOT NULL,
            "UpdatedAtUtc" timestamp with time zone NOT NULL,
            PRIMARY KEY ("TenantId", "UserId")
        );
        """;
}
