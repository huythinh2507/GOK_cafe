namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a configurable attribute for a product type (e.g., Size, Grind, Color, Material)
/// </summary>
public class ProductAttribute : BaseEntity
{
    /// <summary>
    /// Product type this attribute belongs to
    /// </summary>
    public Guid ProductTypeId { get; set; }

    /// <summary>
    /// Internal name of the attribute (e.g., "Size", "Grind")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the attribute (e.g., "Size", "Grind Type", "Region/Origin")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the attribute
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting attributes
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates if this attribute is required when creating a product
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indicates if multiple values can be selected (checkboxes vs radio buttons)
    /// </summary>
    public bool AllowMultipleSelection { get; set; }

    /// <summary>
    /// Indicates if the attribute is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// The product type this attribute belongs to
    /// </summary>
    public virtual ProductType ProductType { get; set; } = null!;

    /// <summary>
    /// Available values for this attribute
    /// </summary>
    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();

    /// <summary>
    /// Product selections using this attribute
    /// </summary>
    public virtual ICollection<ProductAttributeSelection> ProductAttributeSelections { get; set; } = new List<ProductAttributeSelection>();
}
