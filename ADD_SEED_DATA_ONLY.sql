-- =============================================
-- GOK Cafe - Add Seed Data Only
-- Quick script to add relationships and test data
-- =============================================

USE [GOKCafe];
GO

PRINT '========================================='
PRINT 'Adding Seed Data to Existing Tables'
PRINT '========================================='
PRINT ''

-- =============================================
-- 1. Add Product-Flavour Relationships
-- =============================================
PRINT 'Adding Product-Flavour Profile relationships...'

-- Get existing IDs from your database
DECLARE @GreenTeaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Green Tea%')
DECLARE @MatchaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Matcha%')
DECLARE @CappuccinoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cappuccino%')
DECLARE @LatteId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name = 'Latte')
DECLARE @EspressoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Espresso%')
DECLARE @EarlGreyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Earl Grey%')
DECLARE @ColdBrewId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cold Brew%')
DECLARE @ChamomileId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Chamomile%')

DECLARE @BoldId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Bold')
DECLARE @SmoothId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Smooth')
DECLARE @FloralId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Floral')
DECLARE @NuttyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Nutty')
DECLARE @FruityId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Fruity')

-- Only add if no relationships exist yet
IF NOT EXISTS (SELECT * FROM ProductFlavourProfiles)
BEGIN
    -- Green Tea → Floral, Smooth
    IF @GreenTeaId IS NOT NULL AND @FloralId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@GreenTeaId, @FloralId), (@GreenTeaId, @SmoothId)

    -- Matcha → Smooth
    IF @MatchaId IS NOT NULL AND @SmoothId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@MatchaId, @SmoothId)

    -- Cappuccino → Smooth, Nutty
    IF @CappuccinoId IS NOT NULL AND @SmoothId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@CappuccinoId, @SmoothId), (@CappuccinoId, @NuttyId)

    -- Latte → Smooth
    IF @LatteId IS NOT NULL AND @SmoothId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@LatteId, @SmoothId)

    -- Espresso → Bold
    IF @EspressoId IS NOT NULL AND @BoldId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@EspressoId, @BoldId)

    -- Earl Grey → Floral
    IF @EarlGreyId IS NOT NULL AND @FloralId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@EarlGreyId, @FloralId)

    -- Cold Brew → Smooth, Fruity
    IF @ColdBrewId IS NOT NULL AND @SmoothId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@ColdBrewId, @SmoothId), (@ColdBrewId, @FruityId)

    -- Chamomile → Floral
    IF @ChamomileId IS NOT NULL AND @FloralId IS NOT NULL
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@ChamomileId, @FloralId)

    PRINT '  ✓ Product-Flavour relationships added'
    PRINT '  ' + CAST(@@ROWCOUNT AS VARCHAR) + ' relationships inserted'
END
ELSE
    PRINT '  ⓘ Product-Flavour relationships already exist'

GO

-- =============================================
-- 2. Add Product-Equipment Relationships
-- =============================================
PRINT 'Adding Product-Equipment relationships...'

DECLARE @EspressoMachineId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Espresso Machine')
DECLARE @TeaInfuserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Tea Infuser')
DECLARE @ColdBrewMakerId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Cold Brew Maker')

-- Get product IDs
DECLARE @GreenTea UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Green Tea%')
DECLARE @Matcha UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Matcha%')
DECLARE @Cappuccino UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cappuccino%')
DECLARE @Latte UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name = 'Latte')
DECLARE @Espresso UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Espresso%')
DECLARE @EarlGrey UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Earl Grey%')
DECLARE @ColdBrew UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cold Brew%')
DECLARE @Chamomile UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Chamomile%')

IF NOT EXISTS (SELECT * FROM ProductEquipments)
BEGIN
    -- Tea products → Tea Infuser
    IF @GreenTea IS NOT NULL AND @TeaInfuserId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@GreenTea, @TeaInfuserId)

    IF @EarlGrey IS NOT NULL AND @TeaInfuserId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@EarlGrey, @TeaInfuserId)

    IF @Chamomile IS NOT NULL AND @TeaInfuserId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@Chamomile, @TeaInfuserId)

    -- Coffee products → Espresso Machine
    IF @Espresso IS NOT NULL AND @EspressoMachineId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@Espresso, @EspressoMachineId)

    IF @Cappuccino IS NOT NULL AND @EspressoMachineId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@Cappuccino, @EspressoMachineId)

    IF @Latte IS NOT NULL AND @EspressoMachineId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@Latte, @EspressoMachineId)

    IF @Matcha IS NOT NULL AND @EspressoMachineId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@Matcha, @EspressoMachineId)

    -- Cold Brew → Cold Brew Maker
    IF @ColdBrew IS NOT NULL AND @ColdBrewMakerId IS NOT NULL
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@ColdBrew, @ColdBrewMakerId)

    PRINT '  ✓ Product-Equipment relationships added'
    PRINT '  ' + CAST(@@ROWCOUNT AS VARCHAR) + ' relationships inserted'
END
ELSE
    PRINT '  ⓘ Product-Equipment relationships already exist'

GO

-- =============================================
-- 3. Add Sample Cart Items for Testing
-- =============================================
PRINT 'Adding sample cart items for testing...'

-- Get a cart and some products
DECLARE @TestCartId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Carts WHERE SessionId = 'session-abc123')
DECLARE @TestProduct1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cappuccino%')
DECLARE @TestProduct2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Green Tea%')

IF @TestCartId IS NOT NULL AND @TestProduct1 IS NOT NULL
BEGIN
    -- Check if cart is empty
    IF NOT EXISTS (SELECT * FROM CartItems WHERE CartId = @TestCartId AND IsDeleted = 0)
    BEGIN
        -- Add Cappuccino
        IF @TestProduct1 IS NOT NULL
        BEGIN
            DECLARE @Price1 DECIMAL(18,2) = (SELECT Price FROM Products WHERE Id = @TestProduct1)
            INSERT INTO CartItems (Id, CartId, ProductId, Quantity, UnitPrice, DiscountPrice, CreatedAt, IsDeleted)
            VALUES (NEWID(), @TestCartId, @TestProduct1, 2, @Price1, NULL, GETUTCDATE(), 0)
        END

        -- Add Green Tea
        IF @TestProduct2 IS NOT NULL
        BEGIN
            DECLARE @Price2 DECIMAL(18,2) = (SELECT Price FROM Products WHERE Id = @TestProduct2)
            INSERT INTO CartItems (Id, CartId, ProductId, Quantity, UnitPrice, DiscountPrice, CreatedAt, IsDeleted)
            VALUES (NEWID(), @TestCartId, @TestProduct2, 1, @Price2, NULL, GETUTCDATE(), 0)
        END

        PRINT '  ✓ Sample cart items added'
        PRINT '  Cart session-abc123 now has ' + CAST(@@ROWCOUNT AS VARCHAR) + ' items'
    END
    ELSE
        PRINT '  ⓘ Cart already has items'
END
ELSE
    PRINT '  ⚠ Test cart not found - skipped'

GO

-- =============================================
-- 4. Summary
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'Seed Data Summary'
PRINT '========================================='

SELECT
    'FlavourProfiles' AS [Table],
    COUNT(*) AS [Total Records]
FROM FlavourProfiles WHERE IsDeleted = 0
UNION ALL
SELECT 'Equipments', COUNT(*) FROM Equipments WHERE IsDeleted = 0
UNION ALL
SELECT 'Product-Flavour Links', COUNT(*) FROM ProductFlavourProfiles
UNION ALL
SELECT 'Product-Equipment Links', COUNT(*) FROM ProductEquipments
UNION ALL
SELECT 'Cart Items', COUNT(*) FROM CartItems WHERE IsDeleted = 0

PRINT ''
PRINT 'Test Cart Contents:'
SELECT
    p.Name AS ProductName,
    ci.Quantity,
    ci.UnitPrice,
    (ci.Quantity * ci.UnitPrice) AS Total
FROM CartItems ci
INNER JOIN Products p ON ci.ProductId = p.Id
INNER JOIN Carts c ON ci.CartId = c.Id
WHERE c.SessionId = 'session-abc123'
  AND ci.IsDeleted = 0

PRINT ''
PRINT '========================================='
PRINT '✓✓✓ SEED DATA COMPLETE! ✓✓✓'
PRINT '========================================='
PRINT ''
PRINT 'Ready to test cart API!'
PRINT 'Test sessionId: session-abc123'
PRINT ''

GO
