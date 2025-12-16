BEGIN TRANSACTION;
GO

ALTER TABLE [Carts] ADD [AppliedCouponCode] nvarchar(max) NULL;
GO

ALTER TABLE [Carts] ADD [AppliedCouponId] uniqueidentifier NULL;
GO

ALTER TABLE [Carts] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Carts] ADD [ShippingFee] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [CartItems] ADD [SelectedGrind] nvarchar(max) NULL;
GO

ALTER TABLE [CartItems] ADD [SelectedSize] nvarchar(max) NULL;
GO

CREATE INDEX [IX_Carts_AppliedCouponId] ON [Carts] ([AppliedCouponId]);
GO

ALTER TABLE [Carts] ADD CONSTRAINT [FK_Carts_Coupons_AppliedCouponId] FOREIGN KEY ([AppliedCouponId]) REFERENCES [Coupons] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251210040406_AddCartDiscountAndOptions', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Users] ALTER COLUMN [Role] int NOT NULL;
GO

ALTER TABLE [Products] ADD [AvailableColors] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [Material] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [ProductTypeId] uniqueidentifier NULL;
GO

ALTER TABLE [Products] ADD [Sku] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [Style] nvarchar(max) NULL;
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
    CONSTRAINT [FK_ProductAttributes_ProductTypes_ProductTypeId] FOREIGN KEY ([ProductTypeId]) REFERENCES [ProductTypes] ([Id]) ON DELETE CASCADE
);
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
    CONSTRAINT [FK_ProductAttributeValues_ProductAttributes_ProductAttributeId] FOREIGN KEY ([ProductAttributeId]) REFERENCES [ProductAttributes] ([Id]) ON DELETE CASCADE
);
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
    CONSTRAINT [FK_ProductAttributeSelections_ProductAttributeValues_ProductAttributeValueId] FOREIGN KEY ([ProductAttributeValueId]) REFERENCES [ProductAttributeValues] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductAttributeSelections_ProductAttributes_ProductAttributeId] FOREIGN KEY ([ProductAttributeId]) REFERENCES [ProductAttributes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductAttributeSelections_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Products_ProductTypeId] ON [Products] ([ProductTypeId]);
GO

CREATE INDEX [IX_ProductAttributes_DisplayOrder] ON [ProductAttributes] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductAttributes_IsActive] ON [ProductAttributes] ([IsActive]);
GO

CREATE INDEX [IX_ProductAttributes_ProductTypeId] ON [ProductAttributes] ([ProductTypeId]);
GO

CREATE INDEX [IX_ProductAttributes_ProductTypeId_IsActive_DisplayOrder] ON [ProductAttributes] ([ProductTypeId], [IsActive], [DisplayOrder]);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductAttributeId] ON [ProductAttributeSelections] ([ProductAttributeId]);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductAttributeValueId] ON [ProductAttributeSelections] ([ProductAttributeValueId]);
GO

CREATE INDEX [IX_ProductAttributeSelections_ProductId] ON [ProductAttributeSelections] ([ProductId]);
GO

CREATE UNIQUE INDEX [IX_ProductAttributeSelections_ProductId_ProductAttributeId_ProductAttributeValueId] ON [ProductAttributeSelections] ([ProductId], [ProductAttributeId], [ProductAttributeValueId]);
GO

CREATE INDEX [IX_ProductAttributeValues_DisplayOrder] ON [ProductAttributeValues] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductAttributeValues_IsActive] ON [ProductAttributeValues] ([IsActive]);
GO

CREATE INDEX [IX_ProductAttributeValues_ProductAttributeId] ON [ProductAttributeValues] ([ProductAttributeId]);
GO

CREATE INDEX [IX_ProductAttributeValues_ProductAttributeId_IsActive_DisplayOrder] ON [ProductAttributeValues] ([ProductAttributeId], [IsActive], [DisplayOrder]);
GO

CREATE INDEX [IX_ProductTypes_DisplayOrder] ON [ProductTypes] ([DisplayOrder]);
GO

CREATE INDEX [IX_ProductTypes_IsActive] ON [ProductTypes] ([IsActive]);
GO

CREATE INDEX [IX_ProductTypes_IsActive_DisplayOrder] ON [ProductTypes] ([IsActive], [DisplayOrder]);
GO

CREATE UNIQUE INDEX [IX_ProductTypes_Slug] ON [ProductTypes] ([Slug]);
GO

ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_ProductTypes_ProductTypeId] FOREIGN KEY ([ProductTypeId]) REFERENCES [ProductTypes] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251215084635_AddProductConfigurationSystem', N'8.0.22');
GO

COMMIT;
GO

