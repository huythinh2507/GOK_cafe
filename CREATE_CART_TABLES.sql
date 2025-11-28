-- =============================================
-- GOK Cafe - Create Cart Tables Script
-- Run this script on Azure SQL Database: GOKCafe
-- =============================================

USE [GOKCafe];
GO

-- Check if tables already exist and skip if they do
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

    -- Index for faster lookups
    CREATE INDEX [IX_Carts_UserId] ON [dbo].[Carts] ([UserId]);
    CREATE INDEX [IX_Carts_SessionId] ON [dbo].[Carts] ([SessionId]);
    CREATE INDEX [IX_Carts_IsDeleted] ON [dbo].[Carts] ([IsDeleted]);

    PRINT 'Carts table created successfully.'
END
ELSE
BEGIN
    PRINT 'Carts table already exists, skipping...'
END
GO

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

    -- Indexes for faster lookups
    CREATE INDEX [IX_CartItems_CartId] ON [dbo].[CartItems] ([CartId]);
    CREATE INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems] ([ProductId]);
    CREATE INDEX [IX_CartItems_IsDeleted] ON [dbo].[CartItems] ([IsDeleted]);

    PRINT 'CartItems table created successfully.'
END
ELSE
BEGIN
    PRINT 'CartItems table already exists, skipping...'
END
GO

-- Add ReservedQuantity column to Products table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'ReservedQuantity')
BEGIN
    PRINT 'Adding ReservedQuantity column to Products table...'

    ALTER TABLE [dbo].[Products]
    ADD [ReservedQuantity] int NOT NULL DEFAULT 0;

    PRINT 'ReservedQuantity column added successfully.'
END
ELSE
BEGIN
    PRINT 'ReservedQuantity column already exists, skipping...'
END
GO

-- Create FlavourProfiles table if it doesn't exist
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

    CREATE INDEX [IX_FlavourProfiles_IsActive] ON [dbo].[FlavourProfiles] ([IsActive]);
    CREATE INDEX [IX_FlavourProfiles_DisplayOrder] ON [dbo].[FlavourProfiles] ([DisplayOrder]);
    CREATE UNIQUE INDEX [IX_FlavourProfiles_Name] ON [dbo].[FlavourProfiles] ([Name]) WHERE [IsDeleted] = 0;

    PRINT 'FlavourProfiles table created successfully.'
END
ELSE
BEGIN
    PRINT 'FlavourProfiles table already exists, skipping...'
END
GO

-- Create Equipments table if it doesn't exist
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

    CREATE INDEX [IX_Equipments_IsActive] ON [dbo].[Equipments] ([IsActive]);
    CREATE INDEX [IX_Equipments_DisplayOrder] ON [dbo].[Equipments] ([DisplayOrder]);
    CREATE UNIQUE INDEX [IX_Equipments_Name] ON [dbo].[Equipments] ([Name]) WHERE [IsDeleted] = 0;

    PRINT 'Equipments table created successfully.'
END
ELSE
BEGIN
    PRINT 'Equipments table already exists, skipping...'
END
GO

-- Create ProductFlavourProfiles junction table
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

    CREATE INDEX [IX_ProductFlavourProfiles_FlavourProfileId] ON [dbo].[ProductFlavourProfiles] ([FlavourProfileId]);

    PRINT 'ProductFlavourProfiles table created successfully.'
END
ELSE
BEGIN
    PRINT 'ProductFlavourProfiles table already exists, skipping...'
END
GO

-- Create ProductEquipments junction table
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

    CREATE INDEX [IX_ProductEquipments_EquipmentId] ON [dbo].[ProductEquipments] ([EquipmentId]);

    PRINT 'ProductEquipments table created successfully.'
END
ELSE
BEGIN
    PRINT 'ProductEquipments table already exists, skipping...'
END
GO

-- Update migration history
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20251128035451_AddCartFlavourEquipmentAndReservedStock')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251128035451_AddCartFlavourEquipmentAndReservedStock', N'8.0.0');

    PRINT 'Migration history updated.'
END
GO

PRINT '========================================='
PRINT 'Cart tables creation script completed!'
PRINT '========================================='
PRINT ''
PRINT 'Tables created/verified:'
PRINT '  ✓ Carts'
PRINT '  ✓ CartItems'
PRINT '  ✓ FlavourProfiles'
PRINT '  ✓ Equipments'
PRINT '  ✓ ProductFlavourProfiles'
PRINT '  ✓ ProductEquipments'
PRINT '  ✓ Products.ReservedQuantity column'
PRINT ''
PRINT 'You can now use the shopping cart feature!'
GO
