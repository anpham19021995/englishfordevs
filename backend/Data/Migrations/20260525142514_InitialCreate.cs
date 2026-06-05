using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishForDevs.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Users" (
                    "Id" uuid NOT NULL,
                    "Email" character varying(256) NOT NULL,
                    "PasswordHash" character varying(512) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
                );
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "PracticeAttempts" (
                    "Id" uuid NOT NULL,
                    "UserId" uuid NULL,
                    "Mode" character varying(32) NOT NULL,
                    "Message" character varying(4000) NOT NULL,
                    "Source" character varying(64) NOT NULL,
                    "DirectReply" character varying(4000) NOT NULL,
                    "CorrectedVersion" character varying(4000) NOT NULL,
                    "NaturalVersion" character varying(4000) NOT NULL,
                    "Vocabulary" text[] NOT NULL,
                    "ConfidenceFeedback" character varying(4000) NOT NULL,
                    "FollowUpQuestion" character varying(1000) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_PracticeAttempts" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_PracticeAttempts_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "PracticeAttempts" ADD COLUMN IF NOT EXISTS "UserId" uuid NULL;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_PracticeAttempts_CreatedAt"
                    ON "PracticeAttempts" ("CreatedAt");
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_PracticeAttempts_UserId_CreatedAt"
                    ON "PracticeAttempts" ("UserId", "CreatedAt");
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_PracticeAttempts_Users_UserId'
                    ) THEN
                        ALTER TABLE "PracticeAttempts"
                        ADD CONSTRAINT "FK_PracticeAttempts_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticeAttempts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
