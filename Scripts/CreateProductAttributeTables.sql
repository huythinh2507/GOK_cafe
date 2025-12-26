-- Create ProductTypes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductTypes')
BEGIN
    CREATE TABLE ProductTypes (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CreatedBy NVARCHAR(256) NULL,
        UpdatedBy NVARCHAR(256) NULL
    );

    CREATE INDEX IX_ProductTypes_Name ON ProductTypes(Name);

    PRINT 'ProductTypes table created successfully';
END
ELSE
BEGIN
    PRINT 'ProductTypes table already exists';
END
GO

-- Create ProductAttributes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductAttributes')
BEGIN
    CREATE TABLE ProductAttributes (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductTypeId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        DisplayName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsRequired BIT NOT NULL DEFAULT 0,
        AllowMultipleSelection BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CreatedBy NVARCHAR(256) NULL,
        UpdatedBy NVARCHAR(256) NULL,
        CONSTRAINT FK_ProductAttributes_ProductTypes FOREIGN KEY (ProductTypeId)
            REFERENCES ProductTypes(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductAttributes_ProductTypeId ON ProductAttributes(ProductTypeId);
    CREATE INDEX IX_ProductAttributes_Name ON ProductAttributes(Name);

    PRINT 'ProductAttributes table created successfully';
END
ELSE
BEGIN
    PRINT 'ProductAttributes table already exists';
END
GO

-- Create ProductAttributeValues table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductAttributeValues')
BEGIN
    CREATE TABLE ProductAttributeValues (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductAttributeId UNIQUEIDENTIFIER NOT NULL,
        Value NVARCHAR(200) NOT NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CreatedBy NVARCHAR(256) NULL,
        UpdatedBy NVARCHAR(256) NULL,
        CONSTRAINT FK_ProductAttributeValues_ProductAttributes FOREIGN KEY (ProductAttributeId)
            REFERENCES ProductAttributes(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductAttributeValues_ProductAttributeId ON ProductAttributeValues(ProductAttributeId);

    PRINT 'ProductAttributeValues table created successfully';
END
ELSE
BEGIN
    PRINT 'ProductAttributeValues table already exists';
END
GO

-- Add ProductTypeId column to Products table if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ProductTypeId')
BEGIN
    ALTER TABLE Products ADD ProductTypeId UNIQUEIDENTIFIER NULL;

    -- Add foreign key constraint
    ALTER TABLE Products ADD CONSTRAINT FK_Products_ProductTypes
        FOREIGN KEY (ProductTypeId) REFERENCES ProductTypes(Id);

    CREATE INDEX IX_Products_ProductTypeId ON Products(ProductTypeId);

    PRINT 'ProductTypeId column added to Products table';
END
ELSE
BEGIN
    PRINT 'ProductTypeId column already exists in Products table';
END
GO

-- Create ProductAttributeSelections table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductAttributeSelections')
BEGIN
    CREATE TABLE ProductAttributeSelections (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductAttributeId UNIQUEIDENTIFIER NOT NULL,
        ProductAttributeValueId UNIQUEIDENTIFIER NULL,
        CustomValue NVARCHAR(200) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CreatedBy NVARCHAR(256) NULL,
        UpdatedBy NVARCHAR(256) NULL,
        CONSTRAINT FK_ProductAttributeSelections_Products FOREIGN KEY (ProductId)
            REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductAttributeSelections_ProductAttributes FOREIGN KEY (ProductAttributeId)
            REFERENCES ProductAttributes(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ProductAttributeSelections_ProductAttributeValues FOREIGN KEY (ProductAttributeValueId)
            REFERENCES ProductAttributeValues(Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_ProductAttributeSelections_ProductId ON ProductAttributeSelections(ProductId);
    CREATE INDEX IX_ProductAttributeSelections_ProductAttributeId ON ProductAttributeSelections(ProductAttributeId);
    CREATE INDEX IX_ProductAttributeSelections_ProductAttributeValueId ON ProductAttributeSelections(ProductAttributeValueId);

    PRINT 'ProductAttributeSelections table created successfully';
END
ELSE
BEGIN
    PRINT 'ProductAttributeSelections table already exists';
END
GO

PRINT 'All ProductAttribute tables created successfully!';
