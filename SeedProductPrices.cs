using GOKCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GOKCafe;

/// <summary>
/// Standalone script to seed prices for products with zero price
/// Run this with: dotnet run --project GOKCafe.API SeedProductPrices.cs
/// Or compile and run: dotnet run SeedProductPrices.cs
/// </summary>
public class SeedProductPrices
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== GOK Cafe Product Price Seeder ===\n");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("GOKCafe.API/appsettings.json", optional: false)
            .AddJsonFile("GOKCafe.API/appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("ERROR: Connection string not found in appsettings.json");
            return;
        }

        Console.WriteLine($"Using connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...\n");

        // Create DbContext
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        try
        {
            // Test database connection
            await context.Database.CanConnectAsync();
            Console.WriteLine("Database connection successful!\n");

            // Run the price seeder
            await PriceSeeder.SeedProductPricesAsync(context);

            Console.WriteLine("\n=== Price seeding completed successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
