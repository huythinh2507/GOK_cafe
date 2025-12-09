namespace GOKCafe.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; } = 0; // Stock reserved for pending orders
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public string? ShortDescription { get; set; }
    public string? TastingNote { get; set; }
    public string? Region { get; set; }
    public string? Process { get; set; }

    // Product options stored as JSON arrays
    // Example: ["250g", "500g", "1kg"]
    public string? AvailableSizes { get; set; }

    // Example: ["Whole Bean", "French Press", "Filter", "Espresso"]
    public string? AvailableGrinds { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<ProductFlavourProfile> ProductFlavourProfiles { get; set; } = new List<ProductFlavourProfile>();
    public virtual ICollection<ProductEquipment> ProductEquipments { get; set; } = new List<ProductEquipment>();
}
