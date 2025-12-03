USE [GOKCafe]
GO

-- Check if Products table exists
IF OBJECT_ID('[dbo].[Products]', 'U') IS NOT NULL
BEGIN
    PRINT 'Products table exists'

    -- Check total products
    SELECT 'Total Products' as Info, COUNT(*) as Count FROM [dbo].[Products]

    -- Check featured products
    SELECT 'Featured Products' as Info, COUNT(*) as Count FROM [dbo].[Products] WHERE IsFeatured = 1

    -- Show all featured products
    SELECT
        Name,
        Price,
        IsFeatured,
        IsActive,
        ImageUrl,
        CategoryId
    FROM [dbo].[Products]
    WHERE IsFeatured = 1
    ORDER BY DisplayOrder

    -- If no featured products, show all products
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE IsFeatured = 1)
    BEGIN
        PRINT 'No featured products found. Showing all products:'
        SELECT TOP 10
            Name,
            Price,
            IsFeatured,
            IsActive,
            ImageUrl
        FROM [dbo].[Products]
        ORDER BY CreatedDate DESC
    END
END
ELSE
BEGIN
    PRINT 'Products table does not exist!'
END
GO
