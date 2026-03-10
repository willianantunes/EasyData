-- =============================================================================
-- Remove all seeded categories (those with the 'seed_cat_' prefix).
-- Leaves manually-created categories intact.
--
-- Usage:
--   docker exec -i easydata-db-1 /opt/mssql-tools18/bin/sqlcmd \
--     -S localhost -U sa -P 'Password1' -C -d SampleProject \
--     -i /dev/stdin < sample-project/scripts/cleanup-seeded-categories.sql
-- =============================================================================

SET NOCOUNT ON;

DECLARE @CountBefore INT;
SELECT @CountBefore = COUNT(*) FROM Categories WHERE Name LIKE 'seed_cat_%';
PRINT CONCAT('Seeded categories to remove: ', @CountBefore);

DELETE FROM Categories WHERE Name LIKE 'seed_cat_%';

PRINT CONCAT('Removed ', @CountBefore, ' seeded categories.');

DECLARE @Remaining INT;
SELECT @Remaining = COUNT(*) FROM Categories;
PRINT CONCAT('Remaining categories: ', @Remaining);
GO
