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

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<ProductFlavourProfile> ProductFlavourProfiles { get; set; } = new List<ProductFlavourProfile>();
    public virtual ICollection<ProductEquipment> ProductEquipments { get; set; } = new List<ProductEquipment>();
}
