using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GOKCafe.Infrastructure.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GOKCafe.Scripts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Loading configuration...");
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "GOKCafe.API"))
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine("Connection string loaded.");

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                Console.WriteLine("Connecting to database...");
                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                {
                    // Read and execute CreateEventTables.sql
                    Console.WriteLine("Reading CreateEventTables.sql...");
                    var createTablesSql = await File.ReadAllTextAsync("CreateEventTables.sql");

                    Console.WriteLine("Executing table creation script...");
                    await context.Database.ExecuteSqlRawAsync(createTablesSql);
                    Console.WriteLine("Tables created successfully!");

                    // Read and execute SeedEvents.sql
                    Console.WriteLine("\nReading SeedEvents.sql...");
                    var seedDataSql = await File.ReadAllTextAsync("SeedEvents.sql");

                    Console.WriteLine("Executing seed data script...");
                    await context.Database.ExecuteSqlRawAsync(seedDataSql);
                    Console.WriteLine("Data seeded successfully!");
                }

                Console.WriteLine("\nAll scripts executed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
