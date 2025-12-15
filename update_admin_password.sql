-- Update admin password with proper BCrypt hash
USE [GOKCafe];
GO

UPDATE [Users]
SET [PasswordHash] = '$2a$11$3wRIY6PxMXZKZMW1b9vQ5.VEG/Bl/sN4z0QhH3H3D6sKs40HH6XBi'  -- This is the hash for "Admin123@"
WHERE [Email] = 'admin@gokcafe.com';

SELECT * FROM [Users] WHERE [Email] = 'admin@gokcafe.com';
