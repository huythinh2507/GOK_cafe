-- =============================================
-- Seed Partners Data
-- Description: Insert sample partner data for GOK Cafe
-- =============================================

-- Check if Partners table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partners')
BEGIN
    PRINT 'Partners table does not exist. Please run migrations first.';
    RETURN;
END

-- Clear existing partner data (optional - comment out if you want to keep existing data)
-- DELETE FROM Partners WHERE IsDeleted = 0;
-- PRINT 'Existing partners cleared.';

-- Insert Partners based on the partnerships page
DECLARE @PlayBarId UNIQUEIDENTIFIER = NEWID();
DECLARE @BTLibraryId UNIQUEIDENTIFIER = NEWID();
DECLARE @RoyalEnfieldId UNIQUEIDENTIFIER = NEWID();
DECLARE @BlueTokaiId UNIQUEIDENTIFIER = NEWID();
DECLARE @LocalCoffeeId UNIQUEIDENTIFIER = NEWID();

-- Partner 1: Play Bar Project
IF NOT EXISTS (SELECT * FROM Partners WHERE Name = 'Play Bar Project' AND IsDeleted = 0)
BEGIN
    INSERT INTO Partners (Id, Name, LogoUrl, WebsiteUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        @PlayBarId,
        'Play Bar Project',
        'https://images.unsplash.com/photo-1572116469696-31de0f17cc34?w=400&h=300&fit=crop',
        'https://playbarproject.com',
        1,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Play Bar Project partner inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Play Bar Project partner already exists.';
END

-- Partner 2: BT Library
IF NOT EXISTS (SELECT * FROM Partners WHERE Name = 'BT Library' AND IsDeleted = 0)
BEGIN
    INSERT INTO Partners (Id, Name, LogoUrl, WebsiteUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        @BTLibraryId,
        'BT Library',
        'https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400&h=300&fit=crop',
        'https://btlibrary.com',
        2,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'BT Library partner inserted successfully.';
END
ELSE
BEGIN
    PRINT 'BT Library partner already exists.';
END

-- Partner 3: Royal Enfield
IF NOT EXISTS (SELECT * FROM Partners WHERE Name = 'Royal Enfield' AND IsDeleted = 0)
BEGIN
    INSERT INTO Partners (Id, Name, LogoUrl, WebsiteUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        @RoyalEnfieldId,
        'Royal Enfield',
        'https://images.unsplash.com/photo-1558980664-3a031cf67ea8?w=400&h=300&fit=crop',
        'https://www.royalenfield.com',
        3,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Royal Enfield partner inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Royal Enfield partner already exists.';
END

-- Partner 4: Blue Tokai Coffee
IF NOT EXISTS (SELECT * FROM Partners WHERE Name = 'Blue Tokai Coffee' AND IsDeleted = 0)
BEGIN
    INSERT INTO Partners (Id, Name, LogoUrl, WebsiteUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        @BlueTokaiId,
        'Blue Tokai Coffee',
        'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400&h=300&fit=crop',
        'https://bluetokaicoffee.com',
        4,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Blue Tokai Coffee partner inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Blue Tokai Coffee partner already exists.';
END

-- Partner 5: Local Coffee Roasters
IF NOT EXISTS (SELECT * FROM Partners WHERE Name = 'Local Coffee Roasters' AND IsDeleted = 0)
BEGIN
    INSERT INTO Partners (Id, Name, LogoUrl, WebsiteUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        @LocalCoffeeId,
        'Local Coffee Roasters',
        'https://images.unsplash.com/photo-1442512595331-e89e73853f31?w=400&h=300&fit=crop',
        NULL, -- No website
        5,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Local Coffee Roasters partner inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Local Coffee Roasters partner already exists.';
END

-- Verify inserted data
SELECT
    Name,
    WebsiteUrl,
    DisplayOrder,
    IsActive,
    CreatedAt
FROM Partners
WHERE IsDeleted = 0
ORDER BY DisplayOrder;

-- Count and display total partners
DECLARE @TotalPartners INT;
SELECT @TotalPartners = COUNT(*) FROM Partners WHERE IsDeleted = 0 AND IsActive = 1;

PRINT '';
PRINT '=============================================';
PRINT 'Partner seeding completed successfully!';
PRINT 'Total active partners: ' + CAST(@TotalPartners AS VARCHAR(10));
PRINT '=============================================';
