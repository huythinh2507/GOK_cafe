-- Setup Bank Transfer Configurations for QR Code Payment
-- Run this script against your GOKCafe database

USE [GOKCafe];
GO

-- Create BankTransferConfigs table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BankTransferConfigs')
BEGIN
    PRINT 'Creating BankTransferConfigs table...';

    CREATE TABLE [dbo].[BankTransferConfigs] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [BankCode] NVARCHAR(20) NOT NULL,
        [BankName] NVARCHAR(200) NOT NULL,
        [AccountNumber] NVARCHAR(50) NOT NULL,
        [AccountName] NVARCHAR(200) NOT NULL,
        [BankBranch] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_BankTransferConfigs_BankCode ON [BankTransferConfigs]([BankCode]) WHERE [IsActive] = 1;
    CREATE INDEX IX_BankTransferConfigs_IsActive ON [BankTransferConfigs]([IsActive]);

    PRINT 'BankTransferConfigs table created.';
END
ELSE
BEGIN
    PRINT 'BankTransferConfigs table already exists.';
END
GO

-- Clear existing test data
DELETE FROM [dbo].[BankTransferConfigs];
PRINT 'Cleared existing bank configurations.';
GO

-- Insert sample bank configurations
PRINT 'Inserting sample bank configurations...';

-- MB Bank (Military Bank)
INSERT INTO [dbo].[BankTransferConfigs]
    ([Id], [BankCode], [BankName], [AccountNumber], [AccountName], [BankBranch], [IsActive], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), '970422', 'MB Bank', '0123456789012', 'CONG TY GOK CAFE', 'Chi nhanh TP HCM', 1, GETUTCDATE(), GETUTCDATE());

-- Vietcombank
INSERT INTO [dbo].[BankTransferConfigs]
    ([Id], [BankCode], [BankName], [AccountNumber], [AccountName], [BankBranch], [IsActive], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), '970436', 'Vietcombank', '9876543210987', 'CONG TY GOK CAFE', 'Chi nhanh TP HCM', 1, GETUTCDATE(), GETUTCDATE());

-- Techcombank
INSERT INTO [dbo].[BankTransferConfigs]
    ([Id], [BankCode], [BankName], [AccountNumber], [AccountName], [BankBranch], [IsActive], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), '970407', 'Techcombank', '1234567890123', 'CONG TY GOK CAFE', 'Chi nhanh TP HCM', 1, GETUTCDATE(), GETUTCDATE());

PRINT 'Bank configurations inserted successfully.';
GO

-- Verify the data
SELECT
    BankCode,
    BankName,
    AccountNumber,
    AccountName,
    BankBranch,
    IsActive,
    CreatedAt
FROM [dbo].[BankTransferConfigs]
WHERE IsActive = 1
ORDER BY BankName;
GO

PRINT 'Setup complete! You now have 3 active bank configurations.';
