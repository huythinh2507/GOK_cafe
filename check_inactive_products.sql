SELECT COUNT(*) as TotalProducts,
       SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveProducts,
       SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) as InactiveProducts
FROM Products;

SELECT Id, Name, IsActive, StockQuantity
FROM Products
WHERE IsActive = 0
ORDER BY Name;
