CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL
);

CREATE TABLE "Gifts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Gifts" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Link" TEXT NULL,
    "Category" TEXT NULL,
    "IsTaken" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_Gifts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Volunteers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Volunteers" PRIMARY KEY AUTOINCREMENT,
    "GiftId" INTEGER NOT NULL,
    "VolunteerUserId" INTEGER NOT NULL,
    CONSTRAINT "FK_Volunteers_Gifts_GiftId" FOREIGN KEY ("GiftId") REFERENCES "Gifts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Volunteers_Users_VolunteerUserId" FOREIGN KEY ("VolunteerUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Gifts_UserId" ON "Gifts" ("UserId");

CREATE INDEX "IX_Volunteers_GiftId" ON "Volunteers" ("GiftId");

CREATE INDEX "IX_Volunteers_VolunteerUserId" ON "Volunteers" ("VolunteerUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251123085124_InitialCreate', '10.0.0');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "Users" ADD "Email" TEXT NULL;

CREATE TABLE "GiftDays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GiftDays" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_GiftDays_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_GiftDays_UserId" ON "GiftDays" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251214071433_GiftDays', '10.0.0');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "GiftDays" ADD "Day" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "GiftDays" ADD "Month" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "ef_temp_GiftDays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GiftDays" PRIMARY KEY AUTOINCREMENT,
    "Day" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_GiftDays_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_GiftDays" ("Id", "Day", "Month", "Title", "UserId")
SELECT "Id", "Day", "Month", "Title", "UserId"
FROM "GiftDays";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "GiftDays";

ALTER TABLE "ef_temp_GiftDays" RENAME TO "GiftDays";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_GiftDays_UserId" ON "GiftDays" ("UserId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251214080320_ConvertGiftDaysToRecurringDate', '10.0.0');

BEGIN TRANSACTION;
ALTER TABLE "GiftDays" ADD "Protected" INTEGER NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251214081112_Protected_GiftDays', '10.0.0');

COMMIT;

