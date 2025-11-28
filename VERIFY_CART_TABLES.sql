-- =============================================
-- GOK Cafe - Verify Cart Tables in Azure DB
-- Run this to check if all tables exist
-- =============================================

USE [GOKCafe];
GO

PRINT '========================================='
PRINT 'Checking Cart-Related Tables in Azure DB'
PRINT '========================================='
PRINT ''

-- =============================================
-- 1. Check if Carts table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND type in (N'U'))
BEGIN
    PRINT '✓ Carts table EXISTS'
    SELECT COUNT(*) AS CartCount FROM Carts
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ Carts table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 2. Check if CartItems table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND type in (N'U'))
BEGIN
    PRINT '✓ CartItems table EXISTS'
    SELECT COUNT(*) AS CartItemCount FROM CartItems
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ CartItems table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 3. Check if FlavourProfiles table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FlavourProfiles]') AND type in (N'U'))
BEGIN
    PRINT '✓ FlavourProfiles table EXISTS'
    SELECT COUNT(*) AS FlavourProfileCount FROM FlavourProfiles
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ FlavourProfiles table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 4. Check if Equipments table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Equipments]') AND type in (N'U'))
BEGIN
    PRINT '✓ Equipments table EXISTS'
    SELECT COUNT(*) AS EquipmentCount FROM Equipments
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ Equipments table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 5. Check if ProductFlavourProfiles table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductFlavourProfiles]') AND type in (N'U'))
BEGIN
    PRINT '✓ ProductFlavourProfiles table EXISTS'
    SELECT COUNT(*) AS RelationshipCount FROM ProductFlavourProfiles
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ ProductFlavourProfiles table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 6. Check if ProductEquipments table exists
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductEquipments]') AND type in (N'U'))
BEGIN
    PRINT '✓ ProductEquipments table EXISTS'
    SELECT COUNT(*) AS RelationshipCount FROM ProductEquipments
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ ProductEquipments table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 7. Check if Products table has ReservedQuantity column
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'ReservedQuantity')
BEGIN
    PRINT '✓ Products.ReservedQuantity column EXISTS'
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ Products.ReservedQuantity column DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 8. Check Products table
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    PRINT '✓ Products table EXISTS'
    SELECT COUNT(*) AS ProductCount FROM Products WHERE IsDeleted = 0
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ Products table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- 9. Check Categories table
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    PRINT '✓ Categories table EXISTS'
    SELECT COUNT(*) AS CategoryCount FROM Categories WHERE IsDeleted = 0
    PRINT ''
END
ELSE
BEGIN
    PRINT '✗ Categories table DOES NOT EXIST'
    PRINT ''
END

-- =============================================
-- DETAILED CHECK - Show existing tables
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'All Tables in GOKCafe Database:'
PRINT '========================================='
SELECT
    TABLE_NAME,
    CASE
        WHEN TABLE_NAME IN ('Carts', 'CartItems', 'FlavourProfiles', 'Equipments', 'ProductFlavourProfiles', 'ProductEquipments')
        THEN '← CART RELATED'
        WHEN TABLE_NAME IN ('Products', 'Categories', 'ProductImages')
        THEN '← PRODUCT RELATED'
        WHEN TABLE_NAME IN ('Orders', 'OrderItems')
        THEN '← ORDER RELATED'
        WHEN TABLE_NAME IN ('Users')
        THEN '← USER RELATED'
        ELSE ''
    END AS TableType
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME

PRINT ''
PRINT '========================================='
PRINT 'Products with Stock Information:'
PRINT '========================================='
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'ReservedQuantity')
    BEGIN
        SELECT TOP 10
            Name,
            Price,
            StockQuantity,
            ReservedQuantity,
            (StockQuantity - ReservedQuantity) AS AvailableStock,
            IsActive
        FROM Products
        WHERE IsDeleted = 0
        ORDER BY DisplayOrder
    END
    ELSE
    BEGIN
        SELECT TOP 10
            Name,
            Price,
            StockQuantity,
            IsActive
        FROM Products
        WHERE IsDeleted = 0
        ORDER BY DisplayOrder

        PRINT ''
        PRINT 'WARNING: ReservedQuantity column does not exist yet!'
    END
END
ELSE
BEGIN
    PRINT 'Products table does not exist!'
END

PRINT ''
PRINT '========================================='
PRINT 'Cart Data (if exists):'
PRINT '========================================='
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND type in (N'U'))
BEGIN
    SELECT
        c.Id AS CartId,
        CASE WHEN c.UserId IS NOT NULL THEN 'Authenticated' ELSE 'Anonymous' END AS CartType,
        c.SessionId,
        COUNT(ci.Id) AS ItemCount,
        ISNULL(SUM(ci.Quantity), 0) AS TotalItems,
        c.CreatedAt
    FROM Carts c
    LEFT JOIN CartItems ci ON c.Id = ci.CartId AND ci.IsDeleted = 0
    WHERE c.IsDeleted = 0
    GROUP BY c.Id, c.UserId, c.SessionId, c.CreatedAt
    ORDER BY c.CreatedAt DESC
END
ELSE
BEGIN
    PRINT 'Carts table does not exist!'
END

PRINT ''
PRINT '========================================='
PRINT 'Summary:'
PRINT '========================================='
DECLARE @TableCount INT = 0
DECLARE @CartTables INT = 0

SELECT @TableCount = COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'

SELECT @CartTables = COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN ('Carts', 'CartItems', 'FlavourProfiles', 'Equipments', 'ProductFlavourProfiles', 'ProductEquipments')

PRINT 'Total tables in database: ' + CAST(@TableCount AS VARCHAR)
PRINT 'Cart-related tables found: ' + CAST(@CartTables AS VARCHAR) + ' / 6'

IF @CartTables = 6
BEGIN
    PRINT ''
    PRINT '✓✓✓ ALL CART TABLES EXIST! ✓✓✓'
    PRINT 'You can now use the cart feature!'
END
ELSE
BEGIN
    PRINT ''
    PRINT '✗✗✗ MISSING CART TABLES! ✗✗✗'
    PRINT 'Run CREATE_CART_TABLES.sql to create them.'
END

GO
