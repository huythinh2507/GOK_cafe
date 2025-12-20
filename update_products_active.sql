-- Update all products to IsActive = 1
UPDATE Products 
SET IsActive = 1 
WHERE IsActive = 0;

-- Verify the update
SELECT COUNT(*) as TotalProducts,
       SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveProducts,
       SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) as InactiveProducts
FROM Products;
