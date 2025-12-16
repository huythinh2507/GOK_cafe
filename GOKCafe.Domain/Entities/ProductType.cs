namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a product type (e.g., Coffee, Clothes, Bike, Car)
/// </summary>
public class ProductType : BaseEntity
{
    /// <summary>
    /// Name of the product type
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product type
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL-friendly slug for the product type
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates if the product type is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Products of this type
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Attributes associated with this product type
    /// </summary>
    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
