-- Update admin@example.com password with BCrypt hash for "Huythinh1"
USE [GOKCafe];
GO

-- First, let's check if the user exists
SELECT * FROM [Users] WHERE [Email] = 'admin@example.com';
GO

-- Update the password hash
-- This hash is for "Huythinh1" with PBKDF2 (the actual hasher used by the app)
UPDATE [Users]
SET [PasswordHash] = 'xRGO4Z5Sntt4+L8Lh2wRKPd5u4nGKz3ZUrp1i1r3fVV2aeFLrOzpUqwLxuKaE6YH'
WHERE [Email] = 'admin@example.com';
GO

-- Verify the update
SELECT [Email], [FirstName], [LastName], [Role], [IsActive] FROM [Users] WHERE [Email] = 'admin@example.com';
GO
