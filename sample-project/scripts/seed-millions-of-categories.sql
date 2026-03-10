-- =============================================================================
-- Seed 5 million categories into the SampleProject database.
--
-- Purpose: Force the pagination COUNT query to exceed the 200ms timeout,
--          triggering the fallback count (9,999,999,999) in the admin dashboard.
--
-- Prerequisites:
--   1. SQL Server running:  docker compose up -d db
--   2. Sample app started at least once to create the schema:
--        cd sample-project/src && dotnet run -- api
--      (stop the app after schema is created, or leave it running)
--
-- Usage:
--   docker exec -i easydata-db-1 /opt/mssql-tools18/bin/sqlcmd \
--     -S localhost -U sa -P 'Password1' -C -d SampleProject \
--     -i /dev/stdin < sample-project/scripts/seed-millions-of-categories.sql
--
-- Duration: ~1-3 minutes depending on hardware.
-- =============================================================================

SET NOCOUNT ON;

DECLARE @TotalTarget INT = 5000000;
DECLARE @BatchSize   INT = 100000;
DECLARE @Inserted    INT = 0;
DECLARE @Existing    INT;

SELECT @Existing = COUNT(*) FROM Categories;
PRINT CONCAT('Existing categories: ', @Existing);

IF @Existing >= @TotalTarget
BEGIN
    PRINT CONCAT('Already have ', @Existing, ' categories. Nothing to do.');
    RETURN;
END

-- Adjust target to account for existing rows
SET @TotalTarget = @TotalTarget - @Existing;
PRINT CONCAT('Will insert ', @TotalTarget, ' new categories...');

WHILE @Inserted < @TotalTarget
BEGIN
    DECLARE @CurrentBatch INT = CASE
        WHEN @TotalTarget - @Inserted < @BatchSize
        THEN @TotalTarget - @Inserted
        ELSE @BatchSize
    END;

    -- Generate rows using a cross-joined CTE (no recursion limits)
    ;WITH
        E1(N) AS (SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
                   UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1),
        E2(N) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b),           -- 100
        E3(N) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b),           -- 10,000
        E5(N) AS (SELECT 1 FROM E3 a CROSS JOIN E2 b),           -- 1,000,000
        Numbers AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N FROM E5)
    INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt)
    SELECT
        CONCAT('seed_cat_', FORMAT(@Existing + @Inserted + N, '00000000')),
        CONCAT('Auto-generated category #', @Existing + @Inserted + N),
        GETUTCDATE(),
        GETUTCDATE()
    FROM Numbers
    WHERE N <= @CurrentBatch;

    SET @Inserted = @Inserted + @CurrentBatch;
    PRINT CONCAT('  Progress: ', @Inserted, ' / ', @TotalTarget, ' inserted');
END

PRINT '';
PRINT CONCAT('Done. Total categories now: ', @Existing + @Inserted);
PRINT 'Open http://localhost:8000/admin/Category/ to see the fallback count in action.';
GO
