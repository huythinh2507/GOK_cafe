-- Update existing products with default size and grind options
UPDATE [Products]
SET
    [AvailableSizes] = '["250g","500g","1kg"]',
    [AvailableGrinds] = '["Whole Bean","French Press","Filter","Espresso"]',
    [ShortDescription] = CASE
        WHEN [ShortDescription] IS NULL OR [ShortDescription] = ''
        THEN 'Premium quality coffee'
        ELSE [ShortDescription]
    END,
    [TastingNote] = CASE
        WHEN [TastingNote] IS NULL OR [TastingNote] = ''
        THEN 'Rich and balanced flavor profile'
        ELSE [TastingNote]
    END,
    [Region] = CASE
        WHEN [Region] IS NULL OR [Region] = ''
        THEN 'Vietnam'
        ELSE [Region]
    END,
    [Process] = CASE
        WHEN [Process] IS NULL OR [Process] = ''
        THEN 'Washed'
        ELSE [Process]
    END
WHERE [AvailableSizes] IS NULL OR [AvailableGrinds] IS NULL;

-- Show number of products updated
SELECT @@ROWCOUNT AS ProductsUpdated;
