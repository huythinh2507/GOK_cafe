-- =============================================
-- GOK Cafe - COMPLETE Cart System Setup
-- This script will:
-- 1. Ensure all cart tables exist
-- 2. Add seed data for testing
-- =============================================

USE [GOKCafe];
GO

PRINT '========================================='
PRINT 'Starting Complete Cart System Setup'
PRINT '========================================='
PRINT ''

-- =============================================
-- PART 1: Ensure Cart Tables Exist
-- =============================================
PRINT 'PART 1: Checking/Creating Cart Tables...'
PRINT ''

-- Check and create Carts table if needed
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Carts table...'
    CREATE TABLE [dbo].[Carts] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [UserId] uniqueidentifier NULL,
        [SessionId] nvarchar(450) NULL,
        [ExpiresAt] datetime2(7) NULL,
        [CreatedAt] datetime2(7) NOT NULL,
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Carts_Users_UserId] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Carts_UserId] ON [dbo].[Carts] ([UserId]);
    CREATE INDEX [IX_Carts_SessionId] ON [dbo].[Carts] ([SessionId]);
    PRINT '  ✓ Carts table created'
END
ELSE
    PRINT '  ⓘ Carts table already exists'

-- Check and create CartItems table if needed
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND type in (N'U'))
BEGIN
    PRINT 'Creating CartItems table...'
    CREATE TABLE [dbo].[CartItems] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [CartId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountPrice] decimal(18,2) NULL,
        [CreatedAt] datetime2(7) NOT NULL,
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId])
            REFERENCES [dbo].[Carts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId])
            REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CartItems_CartId] ON [dbo].[CartItems] ([CartId]);
    CREATE INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems] ([ProductId]);
    PRINT '  ✓ CartItems table created'
END
ELSE
    PRINT '  ⓘ CartItems table already exists'

-- Add ReservedQuantity column if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'ReservedQuantity')
BEGIN
    PRINT 'Adding ReservedQuantity column to Products...'
    ALTER TABLE [dbo].[Products] ADD [ReservedQuantity] int NOT NULL DEFAULT 0;
    PRINT '  ✓ ReservedQuantity column added'
END
ELSE
    PRINT '  ⓘ ReservedQuantity column already exists'

-- Check and create FlavourProfiles table if needed
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FlavourProfiles]') AND type in (N'U'))
BEGIN
    PRINT 'Creating FlavourProfiles table...'
    CREATE TABLE [dbo].[FlavourProfiles] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2(7) NOT NULL,
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0
    );
    PRINT '  ✓ FlavourProfiles table created'
END
ELSE
    PRINT '  ⓘ FlavourProfiles table already exists'

-- Check and create Equipments table if needed
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Equipments]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Equipments table...'
    CREATE TABLE [dbo].[Equipments] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2(7) NOT NULL,
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0
    );
    PRINT '  ✓ Equipments table created'
END
ELSE
    PRINT '  ⓘ Equipments table already exists'

-- Check and create ProductFlavourProfiles junction table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductFlavourProfiles]') AND type in (N'U'))
BEGIN
    PRINT 'Creating ProductFlavourProfiles table...'
    CREATE TABLE [dbo].[ProductFlavourProfiles] (
        [ProductId] uniqueidentifier NOT NULL,
        [FlavourProfileId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ProductFlavourProfiles] PRIMARY KEY ([ProductId], [FlavourProfileId]),
        CONSTRAINT [FK_ProductFlavourProfiles_Products_ProductId] FOREIGN KEY ([ProductId])
            REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductFlavourProfiles_FlavourProfiles_FlavourProfileId] FOREIGN KEY ([FlavourProfileId])
            REFERENCES [dbo].[FlavourProfiles] ([Id]) ON DELETE CASCADE
    );
    PRINT '  ✓ ProductFlavourProfiles table created'
END
ELSE
    PRINT '  ⓘ ProductFlavourProfiles table already exists'

-- Check and create ProductEquipments junction table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductEquipments]') AND type in (N'U'))
BEGIN
    PRINT 'Creating ProductEquipments table...'
    CREATE TABLE [dbo].[ProductEquipments] (
        [ProductId] uniqueidentifier NOT NULL,
        [EquipmentId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ProductEquipments] PRIMARY KEY ([ProductId], [EquipmentId]),
        CONSTRAINT [FK_ProductEquipments_Products_ProductId] FOREIGN KEY ([ProductId])
            REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductEquipments_Equipments_EquipmentId] FOREIGN KEY ([EquipmentId])
            REFERENCES [dbo].[Equipments] ([Id]) ON DELETE CASCADE
    );
    PRINT '  ✓ ProductEquipments table created'
END
ELSE
    PRINT '  ⓘ ProductEquipments table already exists'

GO

-- =============================================
-- PART 2: Seed Data
-- =============================================
PRINT ''
PRINT 'PART 2: Adding Seed Data...'
PRINT ''

-- Seed FlavourProfiles
PRINT 'Seeding Flavour Profiles...'
IF NOT EXISTS (SELECT * FROM FlavourProfiles WHERE Name = 'Bold')
BEGIN
    INSERT INTO FlavourProfiles (Id, Name, Description, DisplayOrder, IsActive, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), 'Bold', 'Strong and intense flavor', 1, 1, GETUTCDATE(), 0),
        (NEWID(), 'Smooth', 'Mild and balanced taste', 2, 1, GETUTCDATE(), 0),
        (NEWID(), 'Fruity', 'Sweet with fruit notes', 3, 1, GETUTCDATE(), 0),
        (NEWID(), 'Nutty', 'Rich nutty undertones', 4, 1, GETUTCDATE(), 0),
        (NEWID(), 'Floral', 'Delicate floral aroma', 5, 1, GETUTCDATE(), 0)
    PRINT '  ✓ Flavour Profiles inserted'
END
ELSE
    PRINT '  ⓘ Flavour Profiles already exist'

-- Seed Equipments
PRINT 'Seeding Equipment...'
IF NOT EXISTS (SELECT * FROM Equipments WHERE Name = 'Espresso Machine')
BEGIN
    INSERT INTO Equipments (Id, Name, Description, DisplayOrder, IsActive, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), 'Espresso Machine', 'Professional espresso maker', 1, 1, GETUTCDATE(), 0),
        (NEWID(), 'French Press', 'Classic immersion brewing', 2, 1, GETUTCDATE(), 0),
        (NEWID(), 'Pour Over', 'Manual drip brewing method', 3, 1, GETUTCDATE(), 0),
        (NEWID(), 'Cold Brew Maker', 'Slow-steeped cold coffee', 4, 1, GETUTCDATE(), 0),
        (NEWID(), 'Tea Infuser', 'Traditional tea brewing', 5, 1, GETUTCDATE(), 0)
    PRINT '  ✓ Equipment inserted'
END
ELSE
    PRINT '  ⓘ Equipment already exists'

GO

-- Seed Product Relationships
PRINT 'Seeding Product-Flavour Profile relationships...'
-- Get existing product and flavour IDs
DECLARE @GreenTeaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Green Tea%')
DECLARE @MatchaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Matcha%')
DECLARE @CappuccinoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Cappuccino%')
DECLARE @LatteId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Latte%' AND Name NOT LIKE '%Matcha%')
DECLARE @EspressoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Name LIKE '%Espresso%')

DECLARE @BoldId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Bold')
DECLARE @SmoothId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Smooth')
DECLARE @FloralId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Floral')
DECLARE @NuttyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Nutty')

IF @GreenTeaId IS NOT NULL AND @FloralId IS NOT NULL AND NOT EXISTS (SELECT * FROM ProductFlavourProfiles)
BEGIN
    -- Green Tea → Floral, Smooth
    IF @GreenTeaId IS NOT NULL
    BEGIN
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@GreenTeaId, @FloralId), (@GreenTeaId, @SmoothId)
    END

    -- Matcha → Smooth
    IF @MatchaId IS NOT NULL
    BEGIN
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@MatchaId, @SmoothId)
    END

    -- Cappuccino → Smooth, Nutty
    IF @CappuccinoId IS NOT NULL
    BEGIN
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@CappuccinoId, @SmoothId), (@CappuccinoId, @NuttyId)
    END

    -- Latte → Smooth
    IF @LatteId IS NOT NULL
    BEGIN
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@LatteId, @SmoothId)
    END

    -- Espresso → Bold
    IF @EspressoId IS NOT NULL
    BEGIN
        INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
        VALUES (@EspressoId, @BoldId)
    END

    PRINT '  ✓ Product-Flavour relationships inserted'
END
ELSE IF EXISTS (SELECT * FROM ProductFlavourProfiles)
    PRINT '  ⓘ Product-Flavour relationships already exist'
ELSE
    PRINT '  ⚠ Skipped - products or flavours not found'

-- Seed Product-Equipment relationships
PRINT 'Seeding Product-Equipment relationships...'
DECLARE @EspressoMachineId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Espresso Machine')
DECLARE @TeaInfuserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Tea Infuser')
DECLARE @ColdBrewMakerId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Cold Brew Maker')

IF @GreenTeaId IS NOT NULL AND @TeaInfuserId IS NOT NULL AND NOT EXISTS (SELECT * FROM ProductEquipments)
BEGIN
    -- Green Tea → Tea Infuser
    IF @GreenTeaId IS NOT NULL
    BEGIN
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@GreenTeaId, @TeaInfuserId)
    END

    -- Espresso → Espresso Machine
    IF @EspressoId IS NOT NULL
    BEGIN
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@EspressoId, @EspressoMachineId)
    END

    -- Cappuccino → Espresso Machine
    IF @CappuccinoId IS NOT NULL
    BEGIN
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@CappuccinoId, @EspressoMachineId)
    END

    -- Latte → Espresso Machine
    IF @LatteId IS NOT NULL
    BEGIN
        INSERT INTO ProductEquipments (ProductId, EquipmentId)
        VALUES (@LatteId, @EspressoMachineId)
    END

    PRINT '  ✓ Product-Equipment relationships inserted'
END
ELSE IF EXISTS (SELECT * FROM ProductEquipments)
    PRINT '  ⓘ Product-Equipment relationships already exist'
ELSE
    PRINT '  ⚠ Skipped - products or equipment not found'

GO

-- Seed Sample Cart Items for testing
PRINT 'Seeding Sample Cart Items...'
-- Get an existing anonymous cart
DECLARE @TestCart UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Carts WHERE SessionId = 'session-abc123')
DECLARE @ProductForCart UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE IsDeleted = 0)

IF @TestCart IS NOT NULL AND @ProductForCart IS NOT NULL AND NOT EXISTS (SELECT * FROM CartItems WHERE CartId = @TestCart)
BEGIN
    DECLARE @ProductPrice DECIMAL(18,2) = (SELECT Price FROM Products WHERE Id = @ProductForCart)

    INSERT INTO CartItems (Id, CartId, ProductId, Quantity, UnitPrice, DiscountPrice, CreatedAt, IsDeleted)
    VALUES (NEWID(), @TestCart, @ProductForCart, 2, @ProductPrice, NULL, GETUTCDATE(), 0)

    PRINT '  ✓ Sample cart items inserted'
END
ELSE IF EXISTS (SELECT * FROM CartItems)
    PRINT '  ⓘ Cart items already exist'
ELSE
    PRINT '  ⚠ Skipped - cart or products not found'

GO

-- =============================================
-- PART 3: Final Verification
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'PART 3: Final Verification'
PRINT '========================================='
PRINT ''

PRINT 'Database Tables Status:'
SELECT
    TABLE_NAME AS [Table Name],
    CASE
        WHEN TABLE_NAME IN ('Carts', 'CartItems') THEN '✓ CART CORE'
        WHEN TABLE_NAME IN ('FlavourProfiles', 'Equipments', 'ProductFlavourProfiles', 'ProductEquipments') THEN '✓ CART FEATURES'
        WHEN TABLE_NAME IN ('Products', 'Categories') THEN '✓ PRODUCTS'
        WHEN TABLE_NAME IN ('Orders', 'OrderItems') THEN '✓ ORDERS'
        WHEN TABLE_NAME = 'Users' THEN '✓ USERS'
        ELSE '  OTHER'
    END AS [Status]
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN ('Carts', 'CartItems', 'FlavourProfiles', 'Equipments', 'ProductFlavourProfiles', 'ProductEquipments', 'Products', 'Categories', 'Orders', 'OrderItems', 'Users')
ORDER BY TABLE_NAME

PRINT ''
PRINT 'Data Summary:'
SELECT 'Carts' AS [Table], COUNT(*) AS [Count] FROM Carts WHERE IsDeleted = 0
UNION ALL
SELECT 'CartItems', COUNT(*) FROM CartItems WHERE IsDeleted = 0
UNION ALL
SELECT 'FlavourProfiles', COUNT(*) FROM FlavourProfiles WHERE IsDeleted = 0
UNION ALL
SELECT 'Equipments', COUNT(*) FROM Equipments WHERE IsDeleted = 0
UNION ALL
SELECT 'ProductFlavourProfiles', COUNT(*) FROM ProductFlavourProfiles
UNION ALL
SELECT 'ProductEquipments', COUNT(*) FROM ProductEquipments
UNION ALL
SELECT 'Products', COUNT(*) FROM Products WHERE IsDeleted = 0
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories WHERE IsDeleted = 0
ORDER BY [Table]

PRINT ''
PRINT 'Sample Products with Stock:'
SELECT TOP 5
    Name,
    Price,
    StockQuantity,
    ReservedQuantity,
    (StockQuantity - ReservedQuantity) AS AvailableStock
FROM Products
WHERE IsDeleted = 0
ORDER BY DisplayOrder

PRINT ''
PRINT '========================================='
PRINT '✓✓✓ SETUP COMPLETE! ✓✓✓'
PRINT '========================================='
PRINT ''
PRINT 'Your cart system is ready to use!'
PRINT 'Test with sessionId: session-abc123'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Start your API: dotnet run --project GOKCafe.API'
PRINT '2. Test cart endpoints at /api/v1/cart'
PRINT '3. Test products at /api/v1/products'
PRINT ''

GO
