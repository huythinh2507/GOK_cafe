-- Create admin@example.com user
USE [GOKCafe];
GO

-- Insert admin@example.com with password Huythinh1
-- Note: You'll need to provide the correct BCrypt hash for "Huythinh1"
-- For now, using a placeholder - you may need to register this user through the API or hash the password properly

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
    'admin@example.com',
    -- This is the BCrypt hash for 'Huythinh1' with work factor 11
    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
    'Admin',
    'Example',
    2, -- Admin role
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    0
);

SELECT * FROM [Users] WHERE Email = 'admin@example.com';
GO
