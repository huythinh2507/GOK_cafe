using GOKCafe.Domain.Entities;

namespace GOKCafe.Tests.Unit.Helpers;

public static class TestDataBuilder
{
    public static Category CreateCategory(
        string name = "Test Category",
        string? description = null,
        string? slug = null,
        bool isActive = true,
        int displayOrder = 1)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? $"{name} Description",
            Slug = slug ?? name.ToLower().Replace(" ", "-"),
            ImageUrl = "https://example.com/image.jpg",
            DisplayOrder = displayOrder,
            IsActive = isActive,
            Products = new List<Product>()
        };
    }

    public static Product CreateProduct(
        string name = "Test Product",
        Guid? categoryId = null,
        decimal price = 10.99m,
        bool isActive = true,
        bool isFeatured = false)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"{name} Description",
            Slug = name.ToLower().Replace(" ", "-"),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Price = price,
            ImageUrl = "https://example.com/product.jpg",
            IsActive = isActive,
            IsFeatured = isFeatured,
            StockQuantity = 100,
            ProductImages = new List<ProductImage>()
        };
    }

    public static Order CreateOrder(
        string? orderNumber = null,
        OrderStatus status = OrderStatus.Pending,
        decimal totalAmount = 50.00m)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber ?? $"ORD-{DateTime.UtcNow.Ticks}",
            Status = status,
            TotalAmount = totalAmount,
            SubTotal = totalAmount * 0.9m,
            Tax = totalAmount * 0.08m,
            ShippingFee = totalAmount * 0.02m,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "1234567890",
            ShippingAddress = "123 Main St",
            PaymentMethod = PaymentMethod.Cash,
            PaymentStatus = PaymentStatus.Pending,
            OrderItems = new List<OrderItem>()
        };
    }

    public static OrderItem CreateOrderItem(
        Guid? orderId = null,
        Guid? productId = null,
        int quantity = 1,
        decimal unitPrice = 10.99m)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity
        };
    }

    public static Banner CreateBanner(
        string title = "Test Banner",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new Banner
        {
            Id = Guid.NewGuid(),
            Title = title,
            Subtitle = "Test Subtitle",
            ImageUrl = "https://example.com/banner.jpg",
            ButtonLink = "https://example.com",
            DisplayOrder = displayOrder,
            IsActive = isActive,
            Type = BannerType.Hero
        };
    }

    public static Mission CreateMission(
        string title = "Test Mission",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new Mission
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Mission Description",
            MediaUrl = "https://example.com/media.jpg",
            MediaType = MediaType.Image,
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public static InfoCard CreateInfoCard(
        string title = "Test InfoCard",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new InfoCard
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Description",
            ImageUrl = "https://example.com/info.jpg",
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public static Partner CreatePartner(
        string name = "Test Partner",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new Partner
        {
            Id = Guid.NewGuid(),
            Name = name,
            LogoUrl = "https://example.com/logo.jpg",
            WebsiteUrl = "https://example.com",
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public static TeaAttribute CreateTeaAttribute(
        string title = "Test Attribute",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new TeaAttribute
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Description",
            ImageUrl = "https://example.com/tea.jpg",
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public static ServiceFeature CreateServiceFeature(
        string title = "Test Service",
        bool isActive = true,
        int displayOrder = 1)
    {
        return new ServiceFeature
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Description",
            IconUrl = "https://example.com/icon.svg",
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
    }

    public static ContactInfo CreateContactInfo(
        string? email = "test@example.com",
        string? phone = "1234567890")
    {
        return new ContactInfo
        {
            Id = Guid.NewGuid(),
            Title = "Contact Us",
            Description = "Get in touch with us",
            Email = email,
            Phone = phone,
            Address = "123 Test St",
            WorkingHours = "Mon-Fri 9-5",
            IsActive = true
        };
    }
}
