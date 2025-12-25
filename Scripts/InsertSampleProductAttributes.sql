-- Insert sample Product Types
DECLARE @CoffeeTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @TeaTypeId UNIQUEIDENTIFIER = NEWID();
DECLARE @GeneralTypeId UNIQUEIDENTIFIER = NEWID();

-- Insert Product Types
IF NOT EXISTS (SELECT * FROM ProductTypes WHERE Name = 'Coffee')
BEGIN
    SET @CoffeeTypeId = NEWID();
    INSERT INTO ProductTypes (Id, Name, Description, IsActive, CreatedDate)
    VALUES (@CoffeeTypeId, 'Coffee', 'Coffee products with size and grind options', 1, GETUTCDATE());
    PRINT 'Coffee product type inserted';
END
ELSE
BEGIN
    SELECT @CoffeeTypeId = Id FROM ProductTypes WHERE Name = 'Coffee';
    PRINT 'Coffee product type already exists';
END

IF NOT EXISTS (SELECT * FROM ProductTypes WHERE Name = 'Tea')
BEGIN
    SET @TeaTypeId = NEWID();
    INSERT INTO ProductTypes (Id, Name, Description, IsActive, CreatedDate)
    VALUES (@TeaTypeId, 'Tea', 'Tea products with packaging options', 1, GETUTCDATE());
    PRINT 'Tea product type inserted';
END
ELSE
BEGIN
    SELECT @TeaTypeId = Id FROM ProductTypes WHERE Name = 'Tea';
    PRINT 'Tea product type already exists';
END

IF NOT EXISTS (SELECT * FROM ProductTypes WHERE Name = 'General')
BEGIN
    SET @GeneralTypeId = NEWID();
    INSERT INTO ProductTypes (Id, Name, Description, IsActive, CreatedDate)
    VALUES (@GeneralTypeId, 'General', 'General merchandise', 1, GETUTCDATE());
    PRINT 'General product type inserted';
END
ELSE
BEGIN
    SELECT @GeneralTypeId = Id FROM ProductTypes WHERE Name = 'General';
    PRINT 'General product type already exists';
END
GO

-- Insert Coffee Product Attributes
DECLARE @CoffeeTypeId UNIQUEIDENTIFIER;
SELECT @CoffeeTypeId = Id FROM ProductTypes WHERE Name = 'Coffee';

DECLARE @SizeAttributeId UNIQUEIDENTIFIER;
DECLARE @GrindAttributeId UNIQUEIDENTIFIER;

-- Size Attribute for Coffee
IF NOT EXISTS (SELECT * FROM ProductAttributes WHERE ProductTypeId = @CoffeeTypeId AND Name = 'Size')
BEGIN
    SET @SizeAttributeId = NEWID();
    INSERT INTO ProductAttributes (Id, ProductTypeId, Name, DisplayName, Description, DisplayOrder, IsRequired, AllowMultipleSelection, IsActive, CreatedDate)
    VALUES (@SizeAttributeId, @CoffeeTypeId, 'Size', 'Size', 'Coffee package size', 1, 1, 0, 1, GETUTCDATE());
    PRINT 'Size attribute for Coffee inserted';

    -- Insert Size Values
    INSERT INTO ProductAttributeValues (Id, ProductAttributeId, Value, DisplayOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), @SizeAttributeId, '100g', 1, 1, GETUTCDATE()),
        (NEWID(), @SizeAttributeId, '250g', 2, 1, GETUTCDATE()),
        (NEWID(), @SizeAttributeId, '500g', 3, 1, GETUTCDATE()),
        (NEWID(), @SizeAttributeId, '1kg', 4, 1, GETUTCDATE());
    PRINT 'Size values inserted';
END
ELSE
BEGIN
    SELECT @SizeAttributeId = Id FROM ProductAttributes WHERE ProductTypeId = @CoffeeTypeId AND Name = 'Size';
    PRINT 'Size attribute already exists';
END

-- Grind Attribute for Coffee
IF NOT EXISTS (SELECT * FROM ProductAttributes WHERE ProductTypeId = @CoffeeTypeId AND Name = 'Grind')
BEGIN
    SET @GrindAttributeId = NEWID();
    INSERT INTO ProductAttributes (Id, ProductTypeId, Name, DisplayName, Description, DisplayOrder, IsRequired, AllowMultipleSelection, IsActive, CreatedDate)
    VALUES (@GrindAttributeId, @CoffeeTypeId, 'Grind', 'Grind Type', 'Coffee grind preference', 2, 1, 0, 1, GETUTCDATE());
    PRINT 'Grind attribute for Coffee inserted';

    -- Insert Grind Values
    INSERT INTO ProductAttributeValues (Id, ProductAttributeId, Value, DisplayOrder, IsActive, CreatedDate)
    VALUES
        (NEWID(), @GrindAttributeId, 'Whole Bean', 1, 1, GETUTCDATE()),
        (NEWID(), @GrindAttributeId, 'Espresso', 2, 1, GETUTCDATE()),
        (NEWID(), @GrindAttributeId, 'Filter', 3, 1, GETUTCDATE()),
        (NEWID(), @GrindAttributeId, 'French Press', 4, 1, GETUTCDATE()),
        (NEWID(), @GrindAttributeId, 'Cold Brew', 5, 1, GETUTCDATE());
    PRINT 'Grind values inserted';
END
ELSE
BEGIN
    SELECT @GrindAttributeId = Id FROM ProductAttributes WHERE ProductTypeId = @CoffeeTypeId AND Name = 'Grind';
    PRINT 'Grind attribute already exists';
END
GO

-- Update existing Coffee products to use the Coffee product type
DECLARE @CoffeeTypeId UNIQUEIDENTIFIER;
SELECT @CoffeeTypeId = Id FROM ProductTypes WHERE Name = 'Coffee';

-- Find Coffee category
DECLARE @CoffeeCategoryId UNIQUEIDENTIFIER;
SELECT TOP 1 @CoffeeCategoryId = Id FROM Categories WHERE Name LIKE '%Coffee%' OR Name LIKE '%Coffees%';

IF @CoffeeCategoryId IS NOT NULL
BEGIN
    UPDATE Products
    SET ProductTypeId = @CoffeeTypeId
    WHERE CategoryId = @CoffeeCategoryId AND ProductTypeId IS NULL;

    PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' coffee products updated with product type';
END

-- Assign attributes to a sample product for testing
DECLARE @SampleProductId UNIQUEIDENTIFIER;
SELECT TOP 1 @SampleProductId = Id FROM Products WHERE ProductTypeId = @CoffeeTypeId;

IF @SampleProductId IS NOT NULL
BEGIN
    DECLARE @SizeAttributeId UNIQUEIDENTIFIER;
    DECLARE @GrindAttributeId UNIQUEIDENTIFIER;
    DECLARE @Size250gId UNIQUEIDENTIFIER;
    DECLARE @GrindEspressoId UNIQUEIDENTIFIER;

    SELECT @SizeAttributeId = Id FROM ProductAttributes WHERE Name = 'Size' AND ProductTypeId = @CoffeeTypeId;
    SELECT @GrindAttributeId = Id FROM ProductAttributes WHERE Name = 'Grind' AND ProductTypeId = @CoffeeTypeId;

    SELECT @Size250gId = Id FROM ProductAttributeValues WHERE ProductAttributeId = @SizeAttributeId AND Value = '250g';
    SELECT @GrindEspressoId = Id FROM ProductAttributeValues WHERE ProductAttributeId = @GrindAttributeId AND Value = 'Espresso';

    -- Add Size selection (250g)
    IF NOT EXISTS (SELECT * FROM ProductAttributeSelections WHERE ProductId = @SampleProductId AND ProductAttributeId = @SizeAttributeId)
    BEGIN
        INSERT INTO ProductAttributeSelections (Id, ProductId, ProductAttributeId, ProductAttributeValueId, CreatedDate)
        VALUES (NEWID(), @SampleProductId, @SizeAttributeId, @Size250gId, GETUTCDATE());
        PRINT 'Sample size selection added';
    END

    -- Add Grind selection (Espresso)
    IF NOT EXISTS (SELECT * FROM ProductAttributeSelections WHERE ProductId = @SampleProductId AND ProductAttributeId = @GrindAttributeId)
    BEGIN
        INSERT INTO ProductAttributeSelections (Id, ProductId, ProductAttributeId, ProductAttributeValueId, CreatedDate)
        VALUES (NEWID(), @SampleProductId, @GrindAttributeId, @GrindEspressoId, GETUTCDATE());
        PRINT 'Sample grind selection added';
    END

    PRINT 'Sample product configured: ' + CAST(@SampleProductId AS VARCHAR(50));
END
ELSE
BEGIN
    PRINT 'No coffee products found to configure';
END
GO

PRINT 'Sample product attributes inserted successfully!';
