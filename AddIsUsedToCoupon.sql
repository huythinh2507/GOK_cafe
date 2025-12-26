-- Add IsUsed column to Coupons table if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Coupons' AND COLUMN_NAME = 'IsUsed'
)
BEGIN
    ALTER TABLE Coupons
    ADD IsUsed BIT NOT NULL DEFAULT 0;

    PRINT 'IsUsed column added to Coupons table';
END
ELSE
BEGIN
    PRINT 'IsUsed column already exists in Coupons table';
END
GO
