using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Categories if not exist
        if (!context.Categories.Any())
        {
            await SeedCategoriesAndProducts(context);
        }

        // Seed new homepage entities if not exist
        if (!context.TeaAttributes.Any())
        {
            await SeedHomePageEntities(context);
        }

        // Seed FlavourProfiles and Equipment if not exist
        if (!context.FlavourProfiles.Any())
        {
            await SeedFlavourProfilesAndEquipment(context);
        }

        // Seed Coupons and Bank Transfer Configs if not exist
        if (!context.Coupons.Any())
        {
            await SeedCouponsAndBankConfigs(context);
        }
    }

    private static async Task SeedCategoriesAndProducts(ApplicationDbContext context)
    {

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

        await context.SaveChangesAsync();
    }

    private static async Task SeedHomePageEntities(ApplicationDbContext context)
    {
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

        // Seed Tea Attributes
        var teaAttributes = new List<TeaAttribute>
        {
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Freshness",
                Description = "Freshly picked tea leaves",
                ImageUrl = "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=800",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Authentic",
                Description = "Traditional brewing methods",
                ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=800",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Flavorful",
                Description = "Rich and aromatic flavors",
                ImageUrl = "https://images.unsplash.com/photo-1597481499750-3e6b22637e12?w=800",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Colorful",
                Description = "Vibrant tea presentations",
                ImageUrl = "https://images.unsplash.com/photo-1576092768241-dec231879fc3?w=800",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Sustainability",
                Description = "Eco-friendly and organic",
                ImageUrl = "https://images.unsplash.com/photo-1627662055896-e8f7f65c7da4?w=800",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TeaAttribute
            {
                Id = Guid.NewGuid(),
                Title = "Natural",
                Description = "Pure and natural ingredients",
                ImageUrl = "https://images.unsplash.com/photo-1594631252845-29fc4cc8cde9?w=800",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.TeaAttributes.AddRangeAsync(teaAttributes);

        // Seed Banners
        var coldBrewProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Natural Cold Brew Coffee");
        var banners = new List<Banner>
        {
            new Banner
            {
                Id = Guid.NewGuid(),
                Title = "Natural Cold Brew Coffee",
                Subtitle = "MORE THAN JUST COLD COFFEE",
                Description = "Refresh your mind with organic, sustainably-sourced cold brew coffee, delivered straight to your door.",
                ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=800",
                ButtonText = "Discover More",
                ButtonLink = "/products/natural-cold-brew-coffee",
                ProductId = coldBrewProduct?.Id,
                DisplayOrder = 1,
                IsActive = true,
                Type = BannerType.Hero,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Banners.AddRangeAsync(banners);

        // Seed Missions
        var missions = new List<Mission>
        {
            new Mission
            {
                Id = Guid.NewGuid(),
                Title = "OUR MISSIONS",
                Subtitle = "YOU'RE THE REASON WE'RE HERE",
                Description = "Everything we do is a matter of heart, body and soul. We strive to form profound partnerships with farmers from all over the world to create perspective together and form healthy working relationships built on trust and respect.",
                MediaUrl = "https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=800",
                MediaType = MediaType.Image,
                ButtonText = "Read More",
                ButtonLink = "/about",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Missions.AddRangeAsync(missions);

        // Seed Info Cards
        var infoCards = new List<InfoCard>
        {
            new InfoCard
            {
                Id = Guid.NewGuid(),
                Title = "A quiet morning with the farmers at the high peaks tea loa",
                Description = "Experience the journey from farm to cup",
                ImageUrl = "https://images.unsplash.com/photo-1464226184884-fa280b87c399?w=800",
                ButtonText = "Read more",
                ButtonLink = "/farmers",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InfoCard
            {
                Id = Guid.NewGuid(),
                Title = "Explore Wholesale. We help build coffee programs to grow.",
                Description = "Partner with us for your business",
                ImageUrl = "https://images.unsplash.com/photo-1511920170033-f8396924c348?w=800",
                ButtonText = "Read more",
                ButtonLink = "/wholesale",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new InfoCard
            {
                Id = Guid.NewGuid(),
                Title = "Quality coffee delivered to your door.",
                Description = "Subscribe and never run out of fresh coffee",
                ImageUrl = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=800",
                ButtonText = "Read more",
                ButtonLink = "/subscribe",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.InfoCards.AddRangeAsync(infoCards);

        // Seed Contact Info
        var contactInfo = new ContactInfo
        {
            Id = Guid.NewGuid(),
            Title = "WHERE TO FIND US",
            Subtitle = "COFFEE, TEA, FOOD AND MORE",
            Description = "Stop by and enjoy beautiful coffees and delicious food prepared in house by our friendly staff.",
            Address = "232/33 Vo Thi Sau, Phuong Vo Thi Sau, Quan 3, TP. Ho Chi Minh",
            Phone = "084 838 302 882",
            Email = "hello@gok.com",
            WorkingHours = "7:00 - 22:30 (Monday - Sunday)",
            ImageUrl = "https://images.unsplash.com/photo-1554118811-1e0d58224f24?w=800",
            MapUrl = "https://maps.google.com",
            ButtonText = "View Map",
            ButtonLink = "/contact",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.ContactInfos.AddAsync(contactInfo);

        // Seed Service Features
        var serviceFeatures = new List<ServiceFeature>
        {
            new ServiceFeature
            {
                Id = Guid.NewGuid(),
                Title = "Shipped FREE and to your Door",
                Description = "with orders above 350,000 VNĐ",
                IconUrl = "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=100",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ServiceFeature
            {
                Id = Guid.NewGuid(),
                Title = "Carefully delivered within 1-3 days",
                Description = "and packaged with love",
                IconUrl = "https://images.unsplash.com/photo-1534536281715-e28d76689b4d?w=100",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ServiceFeature
            {
                Id = Guid.NewGuid(),
                Title = "From Seed to Cup",
                Description = "An experience starts with only the best ingredients",
                IconUrl = "https://images.unsplash.com/photo-1447933601403-0c6688de566e?w=100",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ServiceFeature
            {
                Id = Guid.NewGuid(),
                Title = "We would love to help you",
                Description = "084 838 302 882",
                IconUrl = "https://images.unsplash.com/photo-1423666639041-f56000c27a9a?w=100",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.ServiceFeatures.AddRangeAsync(serviceFeatures);

        await context.SaveChangesAsync();
    }

    private static async Task SeedFlavourProfilesAndEquipment(ApplicationDbContext context)
    {
        // Seed FlavourProfiles
        var flavourProfiles = new List<FlavourProfile>
        {
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Balanced",
                Description = "Well-rounded with harmonious flavors",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Bold and Bitter",
                Description = "Strong, intense flavors with pronounced bitterness",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Chocolatey and Nutty",
                Description = "Rich chocolate notes with nutty undertones",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Delicate and Complex",
                Description = "Subtle flavors with layered complexity",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Experimental",
                Description = "Unique and unconventional flavor combinations",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new FlavourProfile
            {
                Id = Guid.NewGuid(),
                Name = "Fruity and Punchy",
                Description = "Bright fruit notes with vibrant acidity",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.FlavourProfiles.AddRangeAsync(flavourProfiles);

        // Seed Equipment
        var equipments = new List<Equipment>
        {
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Aeropress",
                Description = "Portable brewing device for smooth coffee",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Channі",
                Description = "Traditional Vietnamese coffee filter",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Cold Brew",
                Description = "Immersion brewing for cold coffee concentrate",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Espresso",
                Description = "Pressurized extraction for concentrated coffee",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "French Press",
                Description = "Full immersion brewing with metal filter",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Inverted Aeropress",
                Description = "Aeropress brewing method with extended steeping",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Moka Pot",
                Description = "Stovetop espresso-style brewing",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Pourover",
                Description = "Manual drip brewing for clean flavors",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "South Indian Filter",
                Description = "Traditional South Indian coffee filter",
                DisplayOrder = 9,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Equipments.AddRangeAsync(equipments);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCouponsAndBankConfigs(ApplicationDbContext context)
    {
        // Seed Bank Transfer Configurations
        var bankConfigs = new List<BankTransferConfig>
        {
            new BankTransferConfig
            {
                Id = Guid.NewGuid(),
                BankCode = "970422", // MB Bank
                BankName = "MB Bank (Military Commercial Joint Stock Bank)",
                AccountNumber = "0123456789012",
                AccountName = "CONG TY GOK CAFE",
                BankBranch = "Ho Chi Minh City Branch",
                IsActive = true,
                DisplayOrder = 1,
                LogoUrl = "https://via.placeholder.com/150x50?text=MBBank",
                CreatedAt = DateTime.UtcNow
            },
            new BankTransferConfig
            {
                Id = Guid.NewGuid(),
                BankCode = "970415", // Vietinbank
                BankName = "Vietinbank (Vietnam Joint Stock Commercial Bank for Industry and Trade)",
                AccountNumber = "1234567890123",
                AccountName = "CONG TY GOK CAFE",
                BankBranch = "District 1 Branch",
                IsActive = true,
                DisplayOrder = 2,
                LogoUrl = "https://via.placeholder.com/150x50?text=Vietinbank",
                CreatedAt = DateTime.UtcNow
            },
            new BankTransferConfig
            {
                Id = Guid.NewGuid(),
                BankCode = "970436", // Vietcombank
                BankName = "Vietcombank (Joint Stock Commercial Bank for Foreign Trade of Vietnam)",
                AccountNumber = "2345678901234",
                AccountName = "CONG TY GOK CAFE",
                BankBranch = "Saigon Branch",
                IsActive = true,
                DisplayOrder = 3,
                LogoUrl = "https://via.placeholder.com/150x50?text=Vietcombank",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.BankTransferConfigs.AddRangeAsync(bankConfigs);

        // Seed System Coupons (Available for all users)
        var systemCoupons = new List<Coupon>
        {
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "WELCOME2025",
                Name = "Welcome Coupon 2025",
                Description = "Special welcome discount for new customers - 50,000 VNĐ off your first order!",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 50000,
                MaxDiscountAmount = null,
                MinOrderAmount = 100000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                MaxUsageCount = null, // Unlimited usage
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "COFFEE20",
                Name = "Coffee Lover's Discount",
                Description = "Get 20% off on all coffee products",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20,
                MaxDiscountAmount = 100000,
                MinOrderAmount = 50000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                MaxUsageCount = 100, // Limited to 100 uses
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "TEATIME15",
                Name = "Tea Time Special",
                Description = "15% discount on tea purchases",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15,
                MaxDiscountAmount = 75000,
                MinOrderAmount = 30000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                MaxUsageCount = null,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "FREESHIP",
                Name = "Free Shipping Voucher",
                Description = "Free shipping for orders above 200,000 VNĐ",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 30000,
                MaxDiscountAmount = null,
                MinOrderAmount = 200000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(12),
                MaxUsageCount = null,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "LOYALTY500K",
                Name = "Loyalty Reward - 500K Balance",
                Description = "Gradual discount coupon with 500,000 VNĐ balance - can be used multiple times until balance runs out",
                Type = CouponType.Gradual,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 500000, // Initial balance
                MaxDiscountAmount = 100000, // Max 100K per order
                MinOrderAmount = 50000,
                RemainingBalance = 500000,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                MaxUsageCount = null,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "WEEKEND25",
                Name = "Weekend Special",
                Description = "25% off on weekend orders (Friday-Sunday)",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 25,
                MaxDiscountAmount = 150000,
                MinOrderAmount = 100000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                MaxUsageCount = 50,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "NEWYEAR2025",
                Name = "New Year Mega Sale",
                Description = "Special New Year discount - 100,000 VNĐ off!",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 100000,
                MaxDiscountAmount = null,
                MinOrderAmount = 300000,
                RemainingBalance = null,
                IsSystemCoupon = true,
                UserId = null,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc),
                MaxUsageCount = 200,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Coupons.AddRangeAsync(systemCoupons);

        // Seed Sample Personal Coupons (for demonstration)
        // Note: In production, these would be created when users register or earn rewards
        var personalCoupons = new List<Coupon>
        {
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "PERSONAL100K",
                Name = "Personal Loyalty Reward",
                Description = "Your exclusive loyalty reward - 100,000 VNĐ gradual discount",
                Type = CouponType.Gradual,
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 100000,
                MaxDiscountAmount = 50000, // Max 50K per order
                MinOrderAmount = 30000,
                RemainingBalance = 100000,
                IsSystemCoupon = false,
                UserId = null, // Would be set to actual user ID
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                MaxUsageCount = null,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            },
            new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "BIRTHDAY50",
                Name = "Birthday Gift Coupon",
                Description = "Happy Birthday! Enjoy 50% off your order",
                Type = CouponType.OneTime,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 50,
                MaxDiscountAmount = 200000,
                MinOrderAmount = 0,
                RemainingBalance = null,
                IsSystemCoupon = false,
                UserId = null, // Would be set to actual user ID
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                MaxUsageCount = 1,
                UsageCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Coupons.AddRangeAsync(personalCoupons);

        await context.SaveChangesAsync();
    }
}
