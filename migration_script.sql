BEGIN TRANSACTION;
GO

ALTER TABLE [Products] ADD [AvailableGrinds] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [AvailableSizes] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [Process] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [Region] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [ShortDescription] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [TastingNote] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251209094417_AddProductDetailsAndOptions', N'8.0.22');
GO

COMMIT;
GO

