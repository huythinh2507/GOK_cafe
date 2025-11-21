using GOKCafe.Domain.Entities;

namespace GOKCafe.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (context.Categories.Any())
        {
            return; // Database has been seeded
        }

        // Seed Categories
        var coffeeCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Coffee",
            Description = "Premium coffee selections from around the world",
            Slug = "coffee",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var teaCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Tea",
            Description = "Natural tea range with unique flavors",
            Slug = "tea",
            DisplayOrder = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var specialtyCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Specialty Drinks",
            Description = "Unique and signature beverages",
            Slug = "specialty-drinks",
            DisplayOrder = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Categories.AddRangeAsync(coffeeCategory, teaCategory, specialtyCategory);

        // Seed Products - Coffee
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Natural Cold Brew Coffee",
                Description = "Smooth and refreshing cold brew coffee, steeped for 24 hours for maximum flavor. Perfect for hot days!",
                Slug = "natural-cold-brew-coffee",
                Price = 4.99m,
                DiscountPrice = 3.99m,
                ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=800",
                StockQuantity = 100,
                IsActive = true,
                IsFeatured = true,
                CategoryId = coffeeCategory.Id,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Espresso",
                Description = "Rich and bold espresso made from premium arabica beans. The perfect wake-up call!",
                Slug = "espresso",
                Price = 3.50m,
                ImageUrl = "https://images.unsplash.com/photo-1510591509098-f4fdc6d0ff04?w=800",
                StockQuantity = 150,
                IsActive = true,
                IsFeatured = true,
                CategoryId = coffeeCategory.Id,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cappuccino",
                Description = "Classic Italian cappuccino with perfectly steamed milk and a touch of foam",
                Slug = "cappuccino",
                Price = 4.50m,
                ImageUrl = "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=800",
                StockQuantity = 120,
                IsActive = true,
                IsFeatured = true,
                CategoryId = coffeeCategory.Id,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Latte",
                Description = "Smooth and creamy latte with perfectly balanced espresso and steamed milk",
                Slug = "latte",
                Price = 4.75m,
                DiscountPrice = 4.25m,
                ImageUrl = "https://images.unsplash.com/photo-1561047029-3000c68339ca?w=800",
                StockQuantity = 130,
                IsActive = true,
                IsFeatured = false,
                CategoryId = coffeeCategory.Id,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            // Tea Products
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Green Tea",
                Description = "Premium organic green tea with natural antioxidants. Light and refreshing.",
                Slug = "green-tea",
                Price = 3.99m,
                ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=800",
                StockQuantity = 80,
                IsActive = true,
                IsFeatured = true,
                CategoryId = teaCategory.Id,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Chamomile Tea",
                Description = "Soothing chamomile tea perfect for relaxation. Naturally caffeine-free.",
                Slug = "chamomile-tea",
                Price = 3.75m,
                ImageUrl = "https://images.unsplash.com/photo-1597481499750-3e6b22637e12?w=800",
                StockQuantity = 90,
                IsActive = true,
                IsFeatured = false,
                CategoryId = teaCategory.Id,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Earl Grey Tea",
                Description = "Classic Earl Grey with bergamot essence. Sophisticated and aromatic.",
                Slug = "earl-grey-tea",
                Price = 4.25m,
                ImageUrl = "https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=800",
                StockQuantity = 70,
                IsActive = true,
                IsFeatured = true,
                CategoryId = teaCategory.Id,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Iced Matcha Latte",
                Description = "Premium Japanese matcha with milk over ice. Energizing and delicious!",
                Slug = "iced-matcha-latte",
                Price = 5.50m,
                DiscountPrice = 4.99m,
                ImageUrl = "https://images.unsplash.com/photo-1536013690560-4a75ad10f1f2?w=800",
                StockQuantity = 60,
                IsActive = true,
                IsFeatured = true,
                CategoryId = specialtyCategory.Id,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Products.AddRangeAsync(products);

        // Seed Offers
        var offers = new List<Offer>
        {
            new Offer
            {
                Id = Guid.NewGuid(),
                Title = "Morning Special",
                Description = "Get 20% off on all coffee drinks before 10 AM",
                ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=800",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                DiscountPercentage = 20,
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Offer
            {
                Id = Guid.NewGuid(),
                Title = "Tea Time Deal",
                Description = "Buy 2 teas, get 1 free every afternoon from 2-4 PM",
                ImageUrl = "https://images.unsplash.com/photo-1558160074-4d7d8bdf4256?w=800",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                DiscountPercentage = 33,
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Offer
            {
                Id = Guid.NewGuid(),
                Title = "Cold Brew Summer Special",
                Description = "Featured cold brew at special price - $3.99!",
                ImageUrl = "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=800",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                DiscountAmount = 1.00m,
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Offers.AddRangeAsync(offers);

        // Seed Partners
        var partners = new List<Partner>
        {
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Arabica Beans Co.",
                LogoUrl = "https://via.placeholder.com/150x50?text=Arabica",
                WebsiteUrl = "https://example.com",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Tea Masters",
                LogoUrl = "https://via.placeholder.com/150x50?text=TeaMasters",
                WebsiteUrl = "https://example.com",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Organic Valley",
                LogoUrl = "https://via.placeholder.com/150x50?text=OrganicValley",
                WebsiteUrl = "https://example.com",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Partners.AddRangeAsync(partners);

        // Seed Admin User
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@gokcafe.com",
            PasswordHash = "AQAAAAEAACcQAAAAEGxyz123...", // This should be properly hashed in production
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(adminUser);

        await context.SaveChangesAsync();
    }
}
