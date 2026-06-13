-- Migration: RemoveAmountColumn
-- Removes the Amount column from Products table

ALTER TABLE IF EXISTS "products" DROP COLUMN IF EXISTS "Amount";

-- Update the EF Migration history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260613060012_RemoveAmountColumn', '8.0.4');
