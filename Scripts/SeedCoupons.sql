-- =============================================
-- Coupon System - Seed Sample Data
-- Description: Seeds sample coupons with images for testing
-- =============================================

-- Check if coupons already exist to avoid duplicates
IF NOT EXISTS (SELECT 1 FROM Coupons WHERE Code = 'WELCOME10')
BEGIN
    DECLARE @Coupon1Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Coupon2Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Coupon3Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Coupon4Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Coupon5Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @Coupon6Id UNIQUEIDENTIFIER = NEWID();

    -- Insert Sample Coupons
    INSERT INTO [Coupons] (
        [Id], [Code], [Name], [Description], [Type], [DiscountType], [DiscountValue],
        [MaxDiscountAmount], [MinOrderAmount], [RemainingBalance], [IsSystemCoupon],
        [UserId], [IsActive], [StartDate], [EndDate], [MaxUsageCount], [UsageCount],
        [IsUsed], [ImageUrl], [CreatedAt], [UpdatedAt], [IsDeleted]
    )
    VALUES
    -- Coupon 1: Welcome Discount (10% Off)
    (
        @Coupon1Id,
        'WELCOME10',
        'Welcome Discount - 10% Off',
        'New customer welcome offer! Get 10% off your first order at GOK Cafe.',
        1, -- OneTime
        1, -- Percentage
        10,
        50000, -- Max discount 50,000 VND
        100000, -- Min order 100,000 VND
        NULL,
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(MONTH, 3, GETUTCDATE()),
        NULL, -- Unlimited usage
        0,
        0,
        'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    ),

    -- Coupon 2: Summer Sale (15% Off)
    (
        @Coupon2Id,
        'SUMMER2024',
        'Summer Special - 15% Off',
        'Beat the heat with our summer special! Enjoy 15% off all cold beverages.',
        1, -- OneTime
        1, -- Percentage
        15,
        75000, -- Max discount 75,000 VND
        150000, -- Min order 150,000 VND
        NULL,
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(MONTH, 2, GETUTCDATE()),
        1000, -- Max 1000 uses
        0,
        0,
        'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    ),

    -- Coupon 3: Fixed Amount Discount (50,000 VND)
    (
        @Coupon3Id,
        'GOKCAFE50K',
        'GOK Cafe 50K Discount',
        'Get 50,000 VND off your order when you spend 300,000 VND or more!',
        1, -- OneTime
        2, -- FixedAmount
        50000,
        NULL,
        300000, -- Min order 300,000 VND
        NULL,
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(MONTH, 6, GETUTCDATE()),
        500, -- Max 500 uses
        0,
        0,
        'https://images.unsplash.com/photo-1511920170033-f8396924c348?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    ),

    -- Coupon 4: Gradual Coupon (200,000 VND Balance)
    (
        @Coupon4Id,
        'LOYALTY200K',
        'Loyalty Reward - 200K Balance',
        'Thank you for being a loyal customer! Use this 200,000 VND balance gradually on your future orders.',
        2, -- Gradual
        2, -- FixedAmount
        200000,
        NULL,
        50000, -- Min order 50,000 VND
        200000, -- Remaining balance
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(YEAR, 1, GETUTCDATE()),
        NULL, -- Unlimited uses until balance depleted
        0,
        0,
        'https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    ),

    -- Coupon 5: Flash Sale (20% Off - Limited Time)
    (
        @Coupon5Id,
        'FLASH20',
        'Flash Sale - 20% Off',
        'Limited time flash sale! Get 20% off your entire order. Hurry, while supplies last!',
        1, -- OneTime
        1, -- Percentage
        20,
        100000, -- Max discount 100,000 VND
        200000, -- Min order 200,000 VND
        NULL,
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(DAY, 7, GETUTCDATE()), -- Valid for 7 days only
        100, -- Max 100 uses
        0,
        0,
        'https://images.unsplash.com/photo-1442512595331-e89e73853f31?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    ),

    -- Coupon 6: Coffee Lover Special (100,000 VND Gradual)
    (
        @Coupon6Id,
        'COFFEELOVER',
        'Coffee Lover Special',
        'For true coffee enthusiasts! Enjoy 100,000 VND to use on all coffee products.',
        2, -- Gradual
        2, -- FixedAmount
        100000,
        NULL,
        30000, -- Min order 30,000 VND
        100000, -- Remaining balance
        1, -- IsSystemCoupon
        NULL,
        1, -- IsActive
        GETUTCDATE(),
        DATEADD(MONTH, 6, GETUTCDATE()),
        NULL, -- Unlimited uses until balance depleted
        0,
        0,
        'https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=500',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );

    PRINT 'Sample coupons inserted successfully.';
    PRINT '==============================================';
    PRINT 'Coupon Seed Summary:';
    PRINT '- 6 Coupons created';
    PRINT '  * 3 One-Time coupons';
    PRINT '  * 3 Gradual coupons';
    PRINT '  * 3 Percentage discounts';
    PRINT '  * 3 Fixed amount discounts';
    PRINT '==============================================';
END
ELSE
BEGIN
    PRINT 'Sample coupons already exist. Skipping seed data insertion.';
END

GO
