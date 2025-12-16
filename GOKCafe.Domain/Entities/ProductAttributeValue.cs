namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a specific value for a product attribute (e.g., "200g", "Espresso", "Ethiopia", "Black")
/// </summary>
public class ProductAttributeValue : BaseEntity
{
    /// <summary>
    /// The attribute this value belongs to
    /// </summary>
    public Guid ProductAttributeId { get; set; }

    /// <summary>
    /// The value (e.g., "200g", "Espresso", "Ethiopia")
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Display order for sorting values
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates if the value is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// The attribute this value belongs to
    /// </summary>
    public virtual ProductAttribute ProductAttribute { get; set; } = null!;

    /// <summary>
    /// Product selections using this value
    /// </summary>
    public virtual ICollection<ProductAttributeSelection> ProductAttributeSelections { get; set; } = new List<ProductAttributeSelection>();
}
