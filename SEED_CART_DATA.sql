-- =============================================
-- GOK Cafe - Seed Data for Cart System
-- Run this script AFTER CREATE_CART_TABLES.sql
-- =============================================

USE [GOKCafe];
GO

PRINT 'Starting seed data insertion...'
PRINT ''

-- =============================================
-- 1. CATEGORIES
-- =============================================
PRINT 'Seeding Categories...'

DECLARE @CoffeeCategoryId UNIQUEIDENTIFIER = NEWID()
DECLARE @TeaCategoryId UNIQUEIDENTIFIER = NEWID()
DECLARE @SpecialtyCategoryId UNIQUEIDENTIFIER = NEWID()

IF NOT EXISTS (SELECT * FROM Categories WHERE Name = 'Coffee')
BEGIN
    INSERT INTO Categories (Id, Name, Slug, Description, DisplayOrder, IsActive, CreatedAt, IsDeleted)
    VALUES
        (@CoffeeCategoryId, 'Coffee', 'coffee', 'Premium coffee selections', 1, 1, GETUTCDATE(), 0),
        (@TeaCategoryId, 'Tea', 'tea', 'Traditional and specialty teas', 2, 1, GETUTCDATE(), 0),
        (@SpecialtyCategoryId, 'Specialty Drinks', 'specialty-drinks', 'Unique and seasonal beverages', 3, 1, GETUTCDATE(), 0)

    PRINT '  ✓ Categories inserted'
END
ELSE
BEGIN
    SELECT @CoffeeCategoryId = Id FROM Categories WHERE Name = 'Coffee'
    SELECT @TeaCategoryId = Id FROM Categories WHERE Name = 'Tea'
    SELECT @SpecialtyCategoryId = Id FROM Categories WHERE Name = 'Specialty Drinks'
    PRINT '  ⓘ Categories already exist, using existing IDs'
END
GO

-- =============================================
-- 2. FLAVOUR PROFILES
-- =============================================
PRINT 'Seeding Flavour Profiles...'

DECLARE @BoldId UNIQUEIDENTIFIER = NEWID()
DECLARE @SmoothId UNIQUEIDENTIFIER = NEWID()
DECLARE @FruityId UNIQUEIDENTIFIER = NEWID()
DECLARE @NuttyId UNIQUEIDENTIFIER = NEWID()
DECLARE @FloralId UNIQUEIDENTIFIER = NEWID()

INSERT INTO FlavourProfiles (Id, Name, Description, DisplayOrder, IsActive, CreatedAt, IsDeleted)
VALUES
    (@BoldId, 'Bold', 'Strong and intense flavor', 1, 1, GETUTCDATE(), 0),
    (@SmoothId, 'Smooth', 'Mild and balanced taste', 2, 1, GETUTCDATE(), 0),
    (@FruityId, 'Fruity', 'Sweet with fruit notes', 3, 1, GETUTCDATE(), 0),
    (@NuttyId, 'Nutty', 'Rich nutty undertones', 4, 1, GETUTCDATE(), 0),
    (@FloralId, 'Floral', 'Delicate floral aroma', 5, 1, GETUTCDATE(), 0)

PRINT '  ✓ Flavour Profiles inserted'
GO

-- =============================================
-- 3. EQUIPMENT
-- =============================================
PRINT 'Seeding Equipment...'

DECLARE @EspressoMachineId UNIQUEIDENTIFIER = NEWID()
DECLARE @FrenchPressId UNIQUEIDENTIFIER = NEWID()
DECLARE @PourOverId UNIQUEIDENTIFIER = NEWID()
DECLARE @ColdBrewId UNIQUEIDENTIFIER = NEWID()
DECLARE @TeaInfuserId UNIQUEIDENTIFIER = NEWID()

INSERT INTO Equipments (Id, Name, Description, DisplayOrder, IsActive, CreatedAt, IsDeleted)
VALUES
    (@EspressoMachineId, 'Espresso Machine', 'Professional espresso maker', 1, 1, GETUTCDATE(), 0),
    (@FrenchPressId, 'French Press', 'Classic immersion brewing', 2, 1, GETUTCDATE(), 0),
    (@PourOverId, 'Pour Over', 'Manual drip brewing method', 3, 1, GETUTCDATE(), 0),
    (@ColdBrewId, 'Cold Brew Maker', 'Slow-steeped cold coffee', 4, 1, GETUTCDATE(), 0),
    (@TeaInfuserId, 'Tea Infuser', 'Traditional tea brewing', 5, 1, GETUTCDATE(), 0)

PRINT '  ✓ Equipment inserted'
GO

-- =============================================
-- 4. PRODUCTS
-- =============================================
PRINT 'Seeding Products...'

DECLARE @Product1 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product2 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product3 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product4 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product5 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product6 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product7 UNIQUEIDENTIFIER = NEWID()
DECLARE @Product8 UNIQUEIDENTIFIER = NEWID()

-- Get category IDs
DECLARE @CoffeeCat UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Coffee')
DECLARE @TeaCat UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Tea')
DECLARE @SpecialtyCat UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Categories WHERE Name = 'Specialty Drinks')

INSERT INTO Products (Id, Name, Slug, Description, Price, DiscountPrice, StockQuantity, ReservedQuantity,
                      IsActive, IsFeatured, CategoryId, DisplayOrder, CreatedAt, IsDeleted)
VALUES
    -- Coffee Products
    (@Product1, 'Vietnamese Drip Coffee', 'vietnamese-drip-coffee',
     'Strong coffee brewed with traditional Vietnamese phin filter',
     5.50, 4.99, 100, 0, 1, 1, @CoffeeCat, 1, GETUTCDATE(), 0),

    (@Product2, 'Cappuccino', 'cappuccino',
     'Rich espresso with steamed milk and foam',
     4.50, NULL, 150, 0, 1, 1, @CoffeeCat, 2, GETUTCDATE(), 0),

    (@Product3, 'Iced Americano', 'iced-americano',
     'Bold espresso with cold water over ice',
     3.75, 3.25, 120, 0, 1, 0, @CoffeeCat, 3, GETUTCDATE(), 0),

    (@Product4, 'Cold Brew', 'cold-brew',
     'Smooth cold-steeped coffee, less acidic',
     5.00, NULL, 80, 0, 1, 1, @CoffeeCat, 4, GETUTCDATE(), 0),

    -- Tea Products
    (@Product5, 'Jasmine Green Tea', 'jasmine-green-tea',
     'Delicate green tea with jasmine flowers',
     4.00, 3.50, 90, 0, 1, 1, @TeaCat, 5, GETUTCDATE(), 0),

    (@Product6, 'Earl Grey', 'earl-grey',
     'Classic black tea with bergamot',
     3.50, NULL, 110, 0, 1, 0, @TeaCat, 6, GETUTCDATE(), 0),

    -- Specialty Products
    (@Product7, 'Matcha Latte', 'matcha-latte',
     'Japanese green tea powder with steamed milk',
     5.75, 5.25, 60, 0, 1, 1, @SpecialtyCat, 7, GETUTCDATE(), 0),

    (@Product8, 'Salted Caramel Mocha', 'salted-caramel-mocha',
     'Espresso, chocolate, caramel, and sea salt',
     6.25, NULL, 70, 0, 1, 0, @SpecialtyCat, 8, GETUTCDATE(), 0)

PRINT '  ✓ Products inserted'
GO

-- =============================================
-- 5. PRODUCT FLAVOUR PROFILES
-- =============================================
PRINT 'Seeding Product-Flavour Profile relationships...'

-- Get IDs
DECLARE @VietCoffeeId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'vietnamese-drip-coffee')
DECLARE @CappuccinoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cappuccino')
DECLARE @AmericanoId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'iced-americano')
DECLARE @ColdBrewId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cold-brew')
DECLARE @JasmineId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'jasmine-green-tea')
DECLARE @EarlGreyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'earl-grey')
DECLARE @MatchaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'matcha-latte')
DECLARE @MochaId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'salted-caramel-mocha')

DECLARE @Bold UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Bold')
DECLARE @Smooth UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Smooth')
DECLARE @Fruity UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Fruity')
DECLARE @Nutty UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Nutty')
DECLARE @Floral UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM FlavourProfiles WHERE Name = 'Floral')

INSERT INTO ProductFlavourProfiles (ProductId, FlavourProfileId)
VALUES
    (@VietCoffeeId, @Bold),
    (@VietCoffeeId, @Nutty),
    (@CappuccinoId, @Smooth),
    (@CappuccinoId, @Nutty),
    (@AmericanoId, @Bold),
    (@ColdBrewId, @Smooth),
    (@ColdBrewId, @Fruity),
    (@JasmineId, @Floral),
    (@JasmineId, @Smooth),
    (@EarlGreyId, @Floral),
    (@MatchaId, @Smooth),
    (@MochaId, @Nutty)

PRINT '  ✓ Product-Flavour Profile relationships inserted'
GO

-- =============================================
-- 6. PRODUCT EQUIPMENT
-- =============================================
PRINT 'Seeding Product-Equipment relationships...'

-- Get Equipment IDs
DECLARE @EspressoMachine UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Espresso Machine')
DECLARE @FrenchPress UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'French Press')
DECLARE @PourOver UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Pour Over')
DECLARE @ColdBrew UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Cold Brew Maker')
DECLARE @TeaInfuser UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Equipments WHERE Name = 'Tea Infuser')

-- Get Product IDs
DECLARE @VietCoffee UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'vietnamese-drip-coffee')
DECLARE @Cappuccino UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cappuccino')
DECLARE @Americano UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'iced-americano')
DECLARE @ColdBrewProd UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cold-brew')
DECLARE @Jasmine UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'jasmine-green-tea')
DECLARE @EarlGrey UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'earl-grey')
DECLARE @Matcha UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'matcha-latte')
DECLARE @Mocha UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'salted-caramel-mocha')

INSERT INTO ProductEquipments (ProductId, EquipmentId)
VALUES
    (@VietCoffee, @PourOver),
    (@Cappuccino, @EspressoMachine),
    (@Americano, @EspressoMachine),
    (@ColdBrewProd, @ColdBrew),
    (@Jasmine, @TeaInfuser),
    (@EarlGrey, @TeaInfuser),
    (@Matcha, @EspressoMachine),
    (@Mocha, @EspressoMachine)

PRINT '  ✓ Product-Equipment relationships inserted'
GO

-- =============================================
-- 7. SAMPLE USERS (for testing cart)
-- =============================================
PRINT 'Seeding Test Users...'

DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID()
DECLARE @TestUser2Id UNIQUEIDENTIFIER = NEWID()

-- Note: Password is hashed for "Password123!"
-- You'll need to use the auth/register endpoint to create real users
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'testuser@gokcafe.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt, IsDeleted)
    VALUES
        (@TestUserId, 'testuser@gokcafe.com',
         'AQAAAAIAAYagAAAAEJ1234567890abcdefghijklmnopqrstuvwxyz==', -- Placeholder hash
         'John', 'Doe', '+84901234567', 0, 1, GETUTCDATE(), 0),

        (@TestUser2Id, 'customer@gokcafe.com',
         'AQAAAAIAAYagAAAAEJ1234567890abcdefghijklmnopqrstuvwxyz==', -- Placeholder hash
         'Jane', 'Smith', '+84907654321', 0, 1, GETUTCDATE(), 0)

    PRINT '  ✓ Test users inserted (use auth/register to create real users)'
END
ELSE
BEGIN
    SELECT @TestUserId = Id FROM Users WHERE Email = 'testuser@gokcafe.com'
    PRINT '  ⓘ Test users already exist'
END
GO

-- =============================================
-- 8. SAMPLE CARTS
-- =============================================
PRINT 'Seeding Sample Carts...'

DECLARE @Cart1Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Cart2Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Cart3Id UNIQUEIDENTIFIER = NEWID()

-- Get a test user ID
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'testuser@gokcafe.com')

INSERT INTO Carts (Id, UserId, SessionId, ExpiresAt, CreatedAt, IsDeleted)
VALUES
    -- Authenticated user cart
    (@Cart1Id, @UserId, NULL, DATEADD(DAY, 30, GETUTCDATE()), GETUTCDATE(), 0),

    -- Anonymous user carts
    (@Cart2Id, NULL, 'session-abc123', DATEADD(DAY, 30, GETUTCDATE()), GETUTCDATE(), 0),
    (@Cart3Id, NULL, 'session-xyz789', DATEADD(DAY, 30, GETUTCDATE()), GETUTCDATE(), 0)

PRINT '  ✓ Sample carts inserted'
GO

-- =============================================
-- 9. SAMPLE CART ITEMS
-- =============================================
PRINT 'Seeding Sample Cart Items...'

-- Get cart and product IDs
DECLARE @AuthCart UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Carts WHERE UserId IS NOT NULL)
DECLARE @AnonCart UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Carts WHERE SessionId = 'session-abc123')

DECLARE @VietCoffeeProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'vietnamese-drip-coffee')
DECLARE @CappuccinoProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cappuccino')
DECLARE @MatchaProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'matcha-latte')
DECLARE @ColdBrewProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Slug = 'cold-brew')

-- Authenticated user cart items
INSERT INTO CartItems (Id, CartId, ProductId, Quantity, UnitPrice, DiscountPrice, CreatedAt, IsDeleted)
VALUES
    (NEWID(), @AuthCart, @VietCoffeeProduct, 2, 5.50, 4.99, GETUTCDATE(), 0),
    (NEWID(), @AuthCart, @CappuccinoProduct, 1, 4.50, NULL, GETUTCDATE(), 0)

-- Anonymous user cart items
INSERT INTO CartItems (Id, CartId, ProductId, Quantity, UnitPrice, DiscountPrice, CreatedAt, IsDeleted)
VALUES
    (NEWID(), @AnonCart, @MatchaProduct, 1, 5.75, 5.25, GETUTCDATE(), 0),
    (NEWID(), @AnonCart, @ColdBrewProduct, 3, 5.00, NULL, GETUTCDATE(), 0)

PRINT '  ✓ Sample cart items inserted'
GO

-- =============================================
-- SUMMARY
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'Seed data insertion completed!'
PRINT '========================================='
PRINT ''
PRINT 'Summary of inserted data:'
PRINT '  ✓ 3 Categories (Coffee, Tea, Specialty Drinks)'
PRINT '  ✓ 5 Flavour Profiles (Bold, Smooth, Fruity, Nutty, Floral)'
PRINT '  ✓ 5 Equipment Types (Espresso Machine, French Press, etc.)'
PRINT '  ✓ 8 Products (Coffee, Tea, Specialty drinks)'
PRINT '  ✓ Product-Flavour & Product-Equipment relationships'
PRINT '  ✓ 2 Test Users (for cart testing)'
PRINT '  ✓ 3 Sample Carts (1 authenticated, 2 anonymous)'
PRINT '  ✓ Sample Cart Items'
PRINT ''
PRINT 'You can now test the cart system!'
PRINT ''
PRINT 'Test Cart IDs:'
SELECT
    c.Id AS CartId,
    CASE WHEN c.UserId IS NOT NULL THEN 'Authenticated' ELSE 'Anonymous' END AS CartType,
    c.SessionId,
    COUNT(ci.Id) AS ItemCount,
    SUM(ci.Quantity) AS TotalItems
FROM Carts c
LEFT JOIN CartItems ci ON c.Id = ci.CartId AND ci.IsDeleted = 0
WHERE c.IsDeleted = 0
GROUP BY c.Id, c.UserId, c.SessionId

PRINT ''
PRINT 'Products in stock:'
SELECT
    Name,
    Price,
    DiscountPrice,
    StockQuantity,
    ReservedQuantity,
    (StockQuantity - ReservedQuantity) AS AvailableStock
FROM Products
WHERE IsDeleted = 0
ORDER BY DisplayOrder

GO
