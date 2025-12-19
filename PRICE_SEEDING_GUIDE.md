# Product Price Seeding Guide

This guide explains how to seed prices for products that currently have a price of $0.

## Overview

The price seeding system automatically sets reasonable prices for products based on their category and name. It intelligently determines appropriate price ranges for different types of products.

## Price Ranges by Category

| Category | Price Range (USD) | Examples |
|----------|------------------|----------|
| **Coffee** | $3.00 - $6.50 | Espresso, Latte, Cappuccino, Cold Brew |
| **Tea** | $3.00 - $5.50 | Green Tea, Earl Grey, Chamomile |
| **Specialty Drinks** | $4.50 - $7.50 | Matcha Latte, Signature Blends |
| **Food** | $2.50 - $8.00 | Pastries, Sandwiches, Snacks |
| **Merchandise** | $10.00 - $50.00 | Equipment, Accessories, Branded Items |

## Features

- **Smart Price Detection**: Automatically determines appropriate price ranges based on product category and name
- **Random Variation**: Generates varied prices within the range for realistic pricing
- **Discount Prices**: 20% chance of applying a discount (10-30% off)
- **Detailed Logging**: Shows which products were updated and their new prices

## Usage Methods

### Method 1: Using PowerShell Script (Windows - Recommended)

```powershell
.\seed-prices.ps1
```

This script will:
1. Show you what will be updated
2. Ask for confirmation
3. Run the seeding process
4. Display the results

### Method 2: Using Bash Script (Linux/Mac)

First, make the script executable:
```bash
chmod +x seed-prices.sh
```

Then run it:
```bash
./seed-prices.sh
```

### Method 3: Using dotnet CLI Directly

```bash
dotnet run --project GOKCafe.API -- --seed-prices --exit-after-seed
```

### Method 4: Run as Part of Application Startup

The price seeder can be triggered every time the application starts by passing the argument:

```bash
dotnet run --project GOKCafe.API -- --seed-prices
```

Note: Without `--exit-after-seed`, the application will continue to run after seeding.

## How It Works

1. **Detection**: The seeder finds all products where `Price == 0`
2. **Categorization**: It examines the product's category and name
3. **Price Assignment**: Based on the category, it assigns a random price within the appropriate range
4. **Discount Application**: There's a 20% chance a product will receive a discount price
5. **Database Update**: All changes are saved to the database

## Example Output

```
=== GOK Cafe Product Price Seeder ===

Found 15 products with zero price. Updating...

Updated: Espresso - Category: coffee - Price: $3.75
Updated: Latte - Category: coffee - Price: $4.85 (Discount: $3.88)
Updated: Green Tea - Category: tea - Price: $3.95
Updated: Iced Matcha Latte - Category: specialty drinks - Price: $5.50
Updated: Croissant - Category: food - Price: $4.25

Successfully updated 15 products with prices.
```

## Code Structure

### PriceSeeder.cs

Located at: `GOKCafe.Infrastructure/Data/PriceSeeder.cs`

**Main Methods:**

1. **`SeedProductPricesAsync(ApplicationDbContext context)`**
   - Seeds approximate prices for all products with price = 0
   - Automatically categorizes and assigns appropriate prices

2. **`SeedSpecificProductPricesAsync(ApplicationDbContext context, Dictionary<Guid, decimal> productPrices)`**
   - Allows you to set specific prices for specific product IDs
   - Useful when you know exactly what price each product should have

3. **`SetUniformPriceAsync(ApplicationDbContext context, decimal uniformPrice)`**
   - Sets the same price for all products with price = 0
   - Useful for testing or when all products should have the same price

## Advanced Usage

### Setting Specific Prices

If you need to set specific prices for certain products, you can modify the code or create a custom script:

```csharp
var productPrices = new Dictionary<Guid, decimal>
{
    { Guid.Parse("product-id-1"), 5.99m },
    { Guid.Parse("product-id-2"), 7.50m },
    { Guid.Parse("product-id-3"), 12.99m }
};

await PriceSeeder.SeedSpecificProductPricesAsync(context, productPrices);
```

### Setting a Uniform Price

To set all zero-priced products to the same price:

```csharp
await PriceSeeder.SetUniformPriceAsync(context, 4.99m);
```

## Integration with Program.cs

The seeder is integrated into the application startup in `Program.cs`:

```csharp
if (args.Contains("--seed-prices"))
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting product price seeding...");
    await PriceSeeder.SeedProductPricesAsync(context);
    logger.LogInformation("Product price seeding completed.");

    if (args.Contains("--exit-after-seed"))
    {
        return;
    }
}
```

## Troubleshooting

### No Products Found

If you see "No products with zero price found", it means all your products already have prices set. This is normal and means the seeder has nothing to do.

### Database Connection Errors

Make sure your `appsettings.json` has the correct connection string and that your database is running.

### Build Errors

If you get build errors, make sure you've run:
```bash
dotnet build
```

## Notes

- **Idempotent**: The seeder only updates products with `Price == 0`, so it's safe to run multiple times
- **Transaction Safety**: All updates are done within a single transaction
- **Logging**: Detailed console output shows exactly what was updated
- **No Data Loss**: Only the `Price` and optionally `DiscountPrice` fields are modified

## Customization

To customize the price ranges or logic, edit the `PriceSeeder.cs` file in `GOKCafe.Infrastructure/Data/PriceSeeder.cs`.

You can modify:
- Price ranges for each category
- Discount percentage and probability
- Price determination logic
- Rounding behavior

## Support

For issues or questions about price seeding, please refer to the main project documentation or contact the development team.
