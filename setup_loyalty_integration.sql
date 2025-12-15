-- GOK Cafe - Loyalty Platform Integration Setup Script
-- This script creates the necessary tables for testing the loyalty integration
-- Run this against your GOKCafe database

USE [GOKCafe];
GO

-- Check if Users table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    PRINT 'Creating Users table...';

    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Email] NVARCHAR(256) NOT NULL,
        [PasswordHash] NVARCHAR(512) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [Address] NVARCHAR(500) NULL,
        [Role] INT NOT NULL DEFAULT 0, -- 0=Customer, 1=Staff, 2=Admin
        [IsActive] BIT NOT NULL DEFAULT 1,
        [LastLoginAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_Users_Email ON [Users]([Email]) WHERE [IsDeleted] = 0;
    CREATE INDEX IX_Users_IsDeleted ON [Users]([IsDeleted]);

    PRINT 'Users table created.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- Check if Coupons table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Coupons')
BEGIN
    PRINT 'Creating Coupons table...';

    CREATE TABLE [dbo].[Coupons] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Type] INT NOT NULL, -- 1=OneTime, 2=Gradual
        [DiscountType] INT NOT NULL, -- 1=Percentage, 2=FixedAmount
        [DiscountValue] DECIMAL(18,2) NOT NULL,
        [MaxDiscountAmount] DECIMAL(18,2) NULL,
        [MinOrderAmount] DECIMAL(18,2) NULL,
        [RemainingBalance] DECIMAL(18,2) NULL,
        [IsSystemCoupon] BIT NOT NULL DEFAULT 0,
        [UserId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [MaxUsageCount] INT NULL,
        [UsageCount] INT NOT NULL DEFAULT 0,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Coupons_Users FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_Coupons_Code ON [Coupons]([Code]) WHERE [IsDeleted] = 0;
    CREATE INDEX IX_Coupons_UserId ON [Coupons]([UserId]) WHERE [IsDeleted] = 0;
    CREATE INDEX IX_Coupons_IsSystemCoupon ON [Coupons]([IsSystemCoupon]) WHERE [IsDeleted] = 0;
    CREATE INDEX IX_Coupons_IsActive ON [Coupons]([IsActive]) WHERE [IsDeleted] = 0;

    PRINT 'Coupons table created.';
END
ELSE
BEGIN
    PRINT 'Coupons table already exists.';
END
GO

-- Check if CouponUsage table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CouponUsage')
BEGIN
    PRINT 'Creating CouponUsage table...';

    CREATE TABLE [dbo].[CouponUsage] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [CouponId] UNIQUEIDENTIFIER NOT NULL,
        [OrderId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [SessionId] NVARCHAR(200) NULL,
        [OriginalAmount] DECIMAL(18,2) NOT NULL,
        [DiscountAmount] DECIMAL(18,2) NOT NULL,
        [FinalAmount] DECIMAL(18,2) NOT NULL,
        [RemainingBalance] DECIMAL(18,2) NULL,
        [UsedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CouponUsage_Coupons FOREIGN KEY ([CouponId]) REFERENCES [Coupons]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_CouponUsage_Users FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE SET NULL
    );

    CREATE INDEX IX_CouponUsage_CouponId ON [CouponUsage]([CouponId]);
    CREATE INDEX IX_CouponUsage_UserId ON [CouponUsage]([UserId]);
    CREATE INDEX IX_CouponUsage_OrderId ON [CouponUsage]([OrderId]);

    PRINT 'CouponUsage table created.';
END
ELSE
BEGIN
    PRINT 'CouponUsage table already exists.';
END
GO

-- Insert admin user for testing (if not exists)
IF NOT EXISTS (SELECT * FROM [Users] WHERE [Email] = 'admin@gokcafe.com')
BEGIN
    PRINT 'Creating admin user...';

    -- Password: Admin123@ (hashed with BCrypt work factor 12)
    -- You should change this password after first login
    INSERT INTO [Users] (
        [Id],
        [Email],
        [PasswordHash],
        [FirstName],
        [LastName],
        [Role],
        [IsActive],
        [CreatedAt],
        [UpdatedAt],
        [IsDeleted]
    ) VALUES (
        NEWID(),
        'admin@gokcafe.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5ByE5zgwZceqW', -- Admin123@
        'Admin',
        'User',
        2, -- Admin role
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );

    PRINT 'Admin user created with email: admin@gokcafe.com and password: Admin123@';
    PRINT 'IMPORTANT: Please change this password after first login!';
END
ELSE
BEGIN
    PRINT 'Admin user already exists.';
END
GO

-- Insert test user for testing (if not exists)
IF NOT EXISTS (SELECT * FROM [Users] WHERE [Email] = 'customer@gokcafe.com')
BEGIN
    PRINT 'Creating test customer user...';

    INSERT INTO [Users] (
        [Id],
        [Email],
        [PasswordHash],
        [FirstName],
        [LastName],
        [Role],
        [IsActive],
        [CreatedAt],
        [UpdatedAt],
        [IsDeleted]
    ) VALUES (
        NEWID(),
        'customer@gokcafe.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5ByE5zgwZceqW', -- Admin123@
        'Test',
        'Customer',
        0, -- Customer role
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );

    PRINT 'Test customer created with email: customer@gokcafe.com and password: Admin123@';
END
ELSE
BEGIN
    PRINT 'Test customer already exists.';
END
GO

PRINT '';
PRINT '====================================================';
PRINT 'Loyalty Integration Setup Complete!';
PRINT '====================================================';
PRINT '';
PRINT 'Tables created/verified:';
PRINT '  - Users';
PRINT '  - Coupons';
PRINT '  - CouponUsage';
PRINT '';
PRINT 'Test accounts created:';
PRINT '  Admin:    admin@gokcafe.com / Admin123@';
PRINT '  Customer: customer@gokcafe.com / Admin123@';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Run the PowerShell test script: .\test-simple.ps1';
PRINT '2. Sync vouchers from Loyalty Platform';
PRINT '3. Test coupon application in checkout';
PRINT '';
