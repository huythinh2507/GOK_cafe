-- =============================================
-- GOK Cafe - Product Configuration System
-- Complete Setup Script (Schema + Seed Data)
-- =============================================
-- This script creates the dynamic product configuration system
-- and seeds Coffee and Clothes product types with their attributes
-- =============================================

BEGIN TRANSACTION;
GO

PRINT 'Starting Product Configuration System Setup...';
GO

-- =============================================
-- STEP 1: Create ProductTypes Table
-- =============================================
PRINT 'Creating ProductTypes table...';
GO

CREATE TABLE [ProductTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Slug] nvarchar(100) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProductTypes] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_ProductTypes_Slug] ON [ProductTypes] ([Slug]);
GO

CREATE INDEX [IX_ProductTypes_IsActive] ON [ProductTypes] ([IsActive]);
GO

CREATE INDEX [IX_ProductTypes_DisplayOrder] ON [ProductTypes] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductTypes_IsActive_DisplayOrder] ON [ProductTypes] ([IsActive], [DisplayOrder]);
GO

PRINT 'ProductTypes table created successfully.';
GO

-- =============================================
-- STEP 2: Create ProductAttributes Table
-- =============================================
PRINT 'Creating ProductAttributes table...';
GO

CREATE TABLE [ProductAttributes] (
    [Id] uniqueidentifier NOT NULL,
    [ProductTypeId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL,
    [IsRequired] bit NOT NULL,
    [AllowMultipleSelection] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProductAttributes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductAttributes_ProductTypes_ProductTypeId]
        FOREIGN KEY ([ProductTypeId]) REFERENCES [ProductTypes] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ProductAttributes_ProductTypeId] ON [ProductAttributes] ([ProductTypeId]);
GO

CREATE INDEX [IX_ProductAttributes_IsActive] ON [ProductAttributes] ([IsActive]);
GO

CREATE INDEX [IX_ProductAttributes_DisplayOrder] ON [ProductAttributes] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductAttributes_ProductTypeId_IsActive_DisplayOrder]
    ON [ProductAttributes] ([ProductTypeId], [IsActive], [DisplayOrder]);
GO

PRINT 'ProductAttributes table created successfully.';
GO

-- =============================================
-- STEP 3: Create ProductAttributeValues Table
-- =============================================
PRINT 'Creating ProductAttributeValues table...';
GO

CREATE TABLE [ProductAttributeValues] (
    [Id] uniqueidentifier NOT NULL,
    [ProductAttributeId] uniqueidentifier NOT NULL,
    [Value] nvarchar(200) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProductAttributeValues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductAttributeValues_ProductAttributes_ProductAttributeId]
        FOREIGN KEY ([ProductAttributeId]) REFERENCES [ProductAttributes] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ProductAttributeValues_ProductAttributeId]
    ON [ProductAttributeValues] ([ProductAttributeId]);
GO

CREATE INDEX [IX_ProductAttributeValues_IsActive] ON [ProductAttributeValues] ([IsActive]);
GO

CREATE INDEX [IX_ProductAttributeValues_DisplayOrder] ON [ProductAttributeValues] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductAttributeValues_ProductAttributeId_IsActive_DisplayOrder]
    ON [ProductAttributeValues] ([ProductAttributeId], [IsActive], [DisplayOrder]);
GO

PRINT 'ProductAttributeValues table created successfully.';
GO

-- =============================================
-- STEP 4: Create ProductAttributeSelections Table
-- =============================================
PRINT 'Creating ProductAttributeSelections table...';
GO

CREATE TABLE [ProductAttributeSelections] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductAttributeId] uniqueidentifier NOT NULL,
    [ProductAttributeValueId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProductAttributeSelections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductAttributeSelections_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductAttributeSelections_ProductAttributes_ProductAttributeId]
        FOREIGN KEY ([ProductAttributeId]) REFERENCES [ProductAttributes] ([Id]),
    CONSTRAINT [FK_ProductAttributeSelections_ProductAttributeValues_ProductAttributeValueId]
        FOREIGN KEY ([ProductAttributeValueId]) REFERENCES [ProductAttributeValues] ([Id])
);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductId]
    ON [ProductAttributeSelections] ([ProductId]);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductAttributeId]
    ON [ProductAttributeSelections] ([ProductAttributeId]);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductAttributeValueId]
    ON [ProductAttributeSelections] ([ProductAttributeValueId]);
GO

CREATE UNIQUE INDEX [IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId]
    ON [ProductAttributeSelections] ([ProductId], [ProductAttributeId], [ProductAttributeValueId]);
GO

PRINT 'ProductAttributeSelections table created successfully.';
GO

-- =============================================
-- STEP 5: Add ProductTypeId to Products Table
-- =============================================
PRINT 'Adding ProductTypeId to Products table...';
GO

ALTER TABLE [Products] ADD [ProductTypeId] uniqueidentifier NULL;
GO

CREATE INDEX [IX_Products_ProductTypeId] ON [Products] ([ProductTypeId]);
GO

ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_ProductTypes_ProductTypeId]
    FOREIGN KEY ([ProductTypeId]) REFERENCES [ProductTypes] ([Id]) ON DELETE RESTRICT;
GO

PRINT 'ProductTypeId added to Products table successfully.';
GO

-- =============================================
-- STEP 6: Seed Product Types (Coffee & Clothes)
-- =============================================
PRINT 'Seeding Product Types...';
GO

DECLARE @CoffeeTypeId uniqueidentifier = NEWID();
DECLARE @ClothesTypeId uniqueidentifier = NEWID();

INSERT INTO [ProductTypes] ([Id], [Name], [Description], [Slug], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (@CoffeeTypeId, 'Coffee', 'Coffee products with various origins and roasts', 'coffee', 1, 1, GETUTCDATE(), NULL, 0),
    (@ClothesTypeId, 'Clothes', 'Apparel and clothing items', 'clothes', 2, 1, GETUTCDATE(), NULL, 0);
GO

PRINT 'Product Types seeded successfully.';
GO

-- =============================================
-- STEP 7: Seed Coffee Attributes
-- =============================================
PRINT 'Seeding Coffee Attributes...';
GO

DECLARE @CoffeeTypeId uniqueidentifier = (SELECT [Id] FROM [ProductTypes] WHERE [Slug] = 'coffee');

-- Size Attribute (Multi-select)
DECLARE @CoffeeSizeAttrId uniqueidentifier = NEWID();
INSERT INTO [ProductAttributes] ([Id], [ProductTypeId], [Name], [DisplayName], [Description], [DisplayOrder], [IsRequired], [AllowMultipleSelection], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES (@CoffeeSizeAttrId, @CoffeeTypeId, 'Size', 'Size', 'Coffee package size', 1, 0, 1, 1, GETUTCDATE(), NULL, 0);

INSERT INTO [ProductAttributeValues] ([Id], [ProductAttributeId], [Value], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (NEWID(), @CoffeeSizeAttrId, '200g', 1, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeSizeAttrId, '340g', 2, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeSizeAttrId, '500g', 3, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeSizeAttrId, '1kg', 4, 1, GETUTCDATE(), NULL, 0);

-- Grind Attribute (Multi-select)
DECLARE @CoffeeGrindAttrId uniqueidentifier = NEWID();
INSERT INTO [ProductAttributes] ([Id], [ProductTypeId], [Name], [DisplayName], [Description], [DisplayOrder], [IsRequired], [AllowMultipleSelection], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES (@CoffeeGrindAttrId, @CoffeeTypeId, 'Grind', 'Grind Type', 'Coffee grind options', 2, 0, 1, 1, GETUTCDATE(), NULL, 0);

INSERT INTO [ProductAttributeValues] ([Id], [ProductAttributeId], [Value], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (NEWID(), @CoffeeGrindAttrId, 'Whole Bean', 1, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeGrindAttrId, 'Espresso', 2, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeGrindAttrId, 'Filter', 3, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeGrindAttrId, 'French Press', 4, 1, GETUTCDATE(), NULL, 0);

-- Region Attribute (Single-select)
DECLARE @CoffeeRegionAttrId uniqueidentifier = NEWID();
INSERT INTO [ProductAttributes] ([Id], [ProductTypeId], [Name], [DisplayName], [Description], [DisplayOrder], [IsRequired], [AllowMultipleSelection], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES (@CoffeeRegionAttrId, @CoffeeTypeId, 'Region', 'Region/Origin', 'Coffee origin region', 3, 0, 0, 1, GETUTCDATE(), NULL, 0);

INSERT INTO [ProductAttributeValues] ([Id], [ProductAttributeId], [Value], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (NEWID(), @CoffeeRegionAttrId, 'Ethiopia', 1, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeRegionAttrId, 'Colombia', 2, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeRegionAttrId, 'Brazil', 3, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeRegionAttrId, 'Kenya', 4, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @CoffeeRegionAttrId, 'Vietnam', 5, 1, GETUTCDATE(), NULL, 0);

PRINT 'Coffee Attributes seeded successfully.';
GO

-- =============================================
-- STEP 8: Seed Clothes Attributes
-- =============================================
PRINT 'Seeding Clothes Attributes...';
GO

DECLARE @ClothesTypeId uniqueidentifier = (SELECT [Id] FROM [ProductTypes] WHERE [Slug] = 'clothes');

-- Size Attribute (Multi-select)
DECLARE @ClothesSizeAttrId uniqueidentifier = NEWID();
INSERT INTO [ProductAttributes] ([Id], [ProductTypeId], [Name], [DisplayName], [Description], [DisplayOrder], [IsRequired], [AllowMultipleSelection], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES (@ClothesSizeAttrId, @ClothesTypeId, 'Size', 'Size', 'Clothing size', 1, 0, 1, 1, GETUTCDATE(), NULL, 0);

INSERT INTO [ProductAttributeValues] ([Id], [ProductAttributeId], [Value], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (NEWID(), @ClothesSizeAttrId, 'S', 1, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesSizeAttrId, 'M', 2, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesSizeAttrId, 'L', 3, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesSizeAttrId, 'XL', 4, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesSizeAttrId, 'XXL', 5, 1, GETUTCDATE(), NULL, 0);

-- Color Attribute (Multi-select)
DECLARE @ClothesColorAttrId uniqueidentifier = NEWID();
INSERT INTO [ProductAttributes] ([Id], [ProductTypeId], [Name], [DisplayName], [Description], [DisplayOrder], [IsRequired], [AllowMultipleSelection], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES (@ClothesColorAttrId, @ClothesTypeId, 'Color', 'Color', 'Available colors', 2, 0, 1, 1, GETUTCDATE(), NULL, 0);

INSERT INTO [ProductAttributeValues] ([Id], [ProductAttributeId], [Value], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
    (NEWID(), @ClothesColorAttrId, 'Black', 1, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesColorAttrId, 'White', 2, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesColorAttrId, 'Navy', 3, 1, GETUTCDATE(), NULL, 0),
    (NEWID(), @ClothesColorAttrId, 'Grey', 4, 1, GETUTCDATE(), NULL, 0);

PRINT 'Clothes Attributes seeded successfully.';
GO

-- =============================================
-- STEP 9: Verification Queries
-- =============================================
PRINT 'Verifying setup...';
GO

PRINT 'Product Types Count:';
SELECT COUNT(*) AS ProductTypesCount FROM [ProductTypes] WHERE [IsDeleted] = 0;

PRINT 'Product Attributes Count:';
SELECT COUNT(*) AS ProductAttributesCount FROM [ProductAttributes] WHERE [IsDeleted] = 0;

PRINT 'Product Attribute Values Count:';
SELECT COUNT(*) AS ProductAttributeValuesCount FROM [ProductAttributeValues] WHERE [IsDeleted] = 0;

PRINT 'Product Types with Attributes:';
SELECT
    pt.[Name] AS ProductType,
    COUNT(DISTINCT pa.[Id]) AS AttributeCount,
    COUNT(pav.[Id]) AS ValueCount
FROM [ProductTypes] pt
LEFT JOIN [ProductAttributes] pa ON pa.[ProductTypeId] = pt.[Id] AND pa.[IsDeleted] = 0
LEFT JOIN [ProductAttributeValues] pav ON pav.[ProductAttributeId] = pa.[Id] AND pav.[IsDeleted] = 0
WHERE pt.[IsDeleted] = 0
GROUP BY pt.[Name]
ORDER BY pt.[DisplayOrder];

PRINT '';
PRINT '===========================================';
PRINT 'Product Configuration System Setup Complete!';
PRINT '===========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- 4 new tables created';
PRINT '- ProductTypeId added to Products table';
PRINT '- 2 product types seeded (Coffee, Clothes)';
PRINT '- 5 attributes created';
PRINT '- 18 attribute values created';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Deploy backend services (DTOs, Services, API Controllers)';
PRINT '2. Update product form for dynamic rendering';
PRINT '3. Test product creation with new attributes';
PRINT '===========================================';
GO

COMMIT TRANSACTION;
GO
