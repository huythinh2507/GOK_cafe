-- Add discount and coupon columns to Cart table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'AppliedCouponId')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD [AppliedCouponId] uniqueidentifier NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'AppliedCouponCode')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD [AppliedCouponCode] nvarchar(50) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'DiscountAmount')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND name = 'ShippingFee')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD [ShippingFee] decimal(18,2) NOT NULL DEFAULT 0;
END

-- Add foreign key constraint for AppliedCouponId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Carts_Coupons_AppliedCouponId')
BEGIN
    ALTER TABLE [dbo].[Carts]
    ADD CONSTRAINT [FK_Carts_Coupons_AppliedCouponId]
    FOREIGN KEY ([AppliedCouponId]) REFERENCES [dbo].[Coupons]([Id])
    ON DELETE SET NULL;
END

-- Add size and grind columns to CartItem table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND name = 'SelectedSize')
BEGIN
    ALTER TABLE [dbo].[CartItems]
    ADD [SelectedSize] nvarchar(50) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND name = 'SelectedGrind')
BEGIN
    ALTER TABLE [dbo].[CartItems]
    ADD [SelectedGrind] nvarchar(50) NULL;
END

PRINT 'Cart discount and options columns added successfully!';
