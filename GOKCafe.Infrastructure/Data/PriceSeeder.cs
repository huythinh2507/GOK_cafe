using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Infrastructure.Data;

public static class PriceSeeder
{
    /// <summary>
    /// Seeds approximate prices for products that have a price of 0
    /// Prices are in USD and categorized by product type
    /// </summary>
    public static async Task SeedProductPricesAsync(ApplicationDbContext context)
    {
        var productsWithZeroPrice = await context.Products
            .Include(p => p.Category)
            .Where(p => p.Price == 0)
            .ToListAsync();

        if (!productsWithZeroPrice.Any())
        {
            Console.WriteLine("No products with zero price found. All products have prices set.");
            return;
        }

        Console.WriteLine($"Found {productsWithZeroPrice.Count} products with zero price. Updating...");

        var random = new Random();
        var updatedCount = 0;

        foreach (var product in productsWithZeroPrice)
        {
            var categoryName = product.Category?.Name?.ToLower() ?? "";
            var productName = product.Name?.ToLower() ?? "";

            // Determine price range based on category and product name
            decimal minPrice, maxPrice;

            if (categoryName.Contains("coffee") || productName.Contains("coffee"))
            {
                // Coffee products: $3.00 - $6.50
                if (productName.Contains("espresso"))
                {
                    minPrice = 3.00m;
                    maxPrice = 4.00m;
                }
                else if (productName.Contains("latte") || productName.Contains("cappuccino"))
                {
                    minPrice = 4.00m;
                    maxPrice = 5.50m;
                }
                else if (productName.Contains("cold brew") || productName.Contains("specialty"))
                {
                    minPrice = 4.50m;
                    maxPrice = 6.50m;
                }
                else
                {
                    minPrice = 3.50m;
                    maxPrice = 5.50m;
                }
            }
            else if (categoryName.Contains("tea") || productName.Contains("tea"))
            {
                // Tea products: $3.00 - $5.50
                if (productName.Contains("matcha") || productName.Contains("specialty"))
                {
                    minPrice = 4.50m;
                    maxPrice = 6.00m;
                }
                else if (productName.Contains("green") || productName.Contains("black") || productName.Contains("earl grey"))
                {
                    minPrice = 3.50m;
                    maxPrice = 4.50m;
                }
                else
                {
                    minPrice = 3.00m;
                    maxPrice = 5.00m;
                }
            }
            else if (categoryName.Contains("specialty") || categoryName.Contains("drink"))
            {
                // Specialty drinks: $4.50 - $7.50
                minPrice = 4.50m;
                maxPrice = 7.50m;
            }
            else if (categoryName.Contains("food") || categoryName.Contains("pastry") || categoryName.Contains("bakery"))
            {
                // Food items: $2.50 - $8.00
                minPrice = 2.50m;
                maxPrice = 8.00m;
            }
            else if (categoryName.Contains("merch") || categoryName.Contains("equipment") || categoryName.Contains("accessory"))
            {
                // Merchandise/Equipment: $10.00 - $50.00
                minPrice = 10.00m;
                maxPrice = 50.00m;
            }
            else
            {
                // Default range: $3.00 - $6.00
                minPrice = 3.00m;
                maxPrice = 6.00m;
            }

            // Generate a random price within the range, rounded to 2 decimal places
            var priceRange = maxPrice - minPrice;
            var randomPrice = minPrice + (decimal)(random.NextDouble() * (double)priceRange);
            product.Price = Math.Round(randomPrice, 2);

            // Optionally set a discount price (20% chance of having a discount)
            if (random.Next(100) < 20)
            {
                var discountPercentage = random.Next(10, 30) / 100m; // 10-30% discount
                product.DiscountPrice = Math.Round(product.Price * (1 - discountPercentage), 2);
            }

            updatedCount++;
            Console.WriteLine($"Updated: {product.Name} - Category: {categoryName} - Price: ${product.Price}" +
                            (product.DiscountPrice.HasValue ? $" (Discount: ${product.DiscountPrice})" : ""));
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"\nSuccessfully updated {updatedCount} products with prices.");
    }

    /// <summary>
    /// Seeds prices for specific product IDs
    /// </summary>
    public static async Task SeedSpecificProductPricesAsync(ApplicationDbContext context, Dictionary<Guid, decimal> productPrices)
    {
        foreach (var (productId, price) in productPrices)
        {
            var product = await context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Price = price;
                Console.WriteLine($"Updated: {product.Name} - Price: ${price}");
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Sets a uniform price for all products with zero price
    /// </summary>
    public static async Task SetUniformPriceAsync(ApplicationDbContext context, decimal uniformPrice)
    {
        var productsWithZeroPrice = await context.Products
            .Where(p => p.Price == 0)
            .ToListAsync();

        if (!productsWithZeroPrice.Any())
        {
            Console.WriteLine("No products with zero price found.");
            return;
        }

        foreach (var product in productsWithZeroPrice)
        {
            product.Price = uniformPrice;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Updated {productsWithZeroPrice.Count} products with price ${uniformPrice}");
    }
}
