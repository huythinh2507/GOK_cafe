-- Script to create Cart, CartItem, Coupon tables and add discount columns
-- For GOKCafe Database

PRINT 'Creating Cart and related tables...';

-- Check if Users table exists, if not we need to handle that
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    PRINT 'ERROR: Users table does not exist. Please create it first.';
    -- You may need to create Users table or adjust FK constraint
END

-- Create Carts table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Carts')
BEGIN
    CREATE TABLE [dbo].[Carts] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [UserId] uniqueidentifier NULL,
        [SessionId] nvarchar(max) NULL,
        [ExpiresAt] datetime2 NULL,
        [AppliedCouponId] uniqueidentifier NULL,
        [AppliedCouponCode] nvarchar(50) NULL,
        [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0,
        [ShippingFee] decimal(18,2) NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0
    );
    PRINT 'Carts table created successfully';
END
ELSE
BEGIN
    PRINT 'Carts table already exists, adding missing columns...';

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'AppliedCouponId')
        ALTER TABLE [dbo].[Carts] ADD [AppliedCouponId] uniqueidentifier NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'AppliedCouponCode')
        ALTER TABLE [dbo].[Carts] ADD [AppliedCouponCode] nvarchar(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'DiscountAmount')
        ALTER TABLE [dbo].[Carts] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'ShippingFee')
        ALTER TABLE [dbo].[Carts] ADD [ShippingFee] decimal(18,2) NOT NULL DEFAULT 0;

    PRINT 'Cart columns updated';
END

-- Create CartItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CartItems')
BEGIN
    CREATE TABLE [dbo].[CartItems] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [CartId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountPrice] decimal(18,2) NULL,
        [SelectedSize] nvarchar(50) NULL,
        [SelectedGrind] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_CartItems_CartId] ON [dbo].[CartItems]([CartId]);
    CREATE INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems]([ProductId]);

    PRINT 'CartItems table created successfully';
END
ELSE
BEGIN
    PRINT 'CartItems table already exists, adding missing columns...';

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND name = 'SelectedSize')
        ALTER TABLE [dbo].[CartItems] ADD [SelectedSize] nvarchar(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND name = 'SelectedGrind')
        ALTER TABLE [dbo].[CartItems] ADD [SelectedGrind] nvarchar(50) NULL;

    PRINT 'CartItems columns updated';
END

-- Create Coupons table (needed for FK constraint)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Coupons')
BEGIN
    CREATE TABLE [dbo].[Coupons] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Type] int NOT NULL, -- 0=OneTime, 1=Gradual
        [DiscountType] int NOT NULL, -- 0=Percentage, 1=FixedAmount
        [DiscountValue] decimal(18,2) NOT NULL,
        [MaxDiscountAmount] decimal(18,2) NULL,
        [MinOrderAmount] decimal(18,2) NULL,
        [RemainingBalance] decimal(18,2) NULL,
        [IsSystemCoupon] bit NOT NULL DEFAULT 0,
        [UserId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [MaxUsageCount] int NULL,
        [UsageCount] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [UQ_Coupons_Code] UNIQUE ([Code])
    );

    CREATE INDEX [IX_Coupons_Code] ON [dbo].[Coupons]([Code]);
    CREATE INDEX [IX_Coupons_UserId] ON [dbo].[Coupons]([UserId]);

    PRINT 'Coupons table created successfully';
END

-- Add foreign key from Carts to Coupons if not exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Carts_Coupons_AppliedCouponId')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD CONSTRAINT [FK_Carts_Coupons_AppliedCouponId]
    FOREIGN KEY ([AppliedCouponId]) REFERENCES [dbo].[Coupons]([Id])
    ON DELETE SET NULL;

    CREATE INDEX [IX_Carts_AppliedCouponId] ON [dbo].[Carts]([AppliedCouponId]);

    PRINT 'Foreign key constraint added';
END

-- Add UserId FK if Users table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Carts_Users_UserId')
    BEGIN
        ALTER TABLE [dbo].[Carts]
        ADD CONSTRAINT [FK_Carts_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
        ON DELETE SET NULL;

        CREATE INDEX [IX_Carts_UserId] ON [dbo].[Carts]([UserId]);

        PRINT 'User foreign key constraint added';
    END
END

PRINT 'All cart tables and columns created/updated successfully!';

-- Insert sample coupon for testing (optional)
IF NOT EXISTS (SELECT * FROM [dbo].[Coupons] WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO [dbo].[Coupons]
    ([Id], [Code], [Name], [Description], [Type], [DiscountType], [DiscountValue],
     [MaxDiscountAmount], [MinOrderAmount], [IsSystemCoupon], [IsActive],
     [StartDate], [EndDate], [MaxUsageCount], [UsageCount], [CreatedAt], [IsDeleted])
    VALUES
    (NEWID(), 'WELCOME10', 'Welcome Discount', '10% off for new customers',
     0, 0, 10, 100000, 200000, 1, 1,
     GETUTCDATE(), DATEADD(year, 1, GETUTCDATE()), 1000, 0, GETUTCDATE(), 0);

    PRINT 'Sample coupon WELCOME10 created';
END
