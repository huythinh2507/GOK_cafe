-- =============================================
-- Add ImageUrl column to Coupons table
-- Description: Adds ImageUrl field to support coupon images
-- =============================================

-- Check if the ImageUrl column already exists
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]')
    AND name = 'ImageUrl'
)
BEGIN
    -- Add ImageUrl column to Coupons table
    ALTER TABLE [dbo].[Coupons]
    ADD [ImageUrl] NVARCHAR(500) NULL;

    PRINT 'ImageUrl column added to Coupons table successfully.';
END
ELSE
BEGIN
    PRINT 'ImageUrl column already exists in Coupons table.';
END

GO
