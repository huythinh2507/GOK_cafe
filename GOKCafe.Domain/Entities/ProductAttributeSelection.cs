namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a product's selected attribute values (join table between Product, ProductAttribute, and ProductAttributeValue)
/// For multi-select attributes, multiple rows exist for the same ProductId + ProductAttributeId
/// </summary>
public class ProductAttributeSelection : BaseEntity
{
    /// <summary>
    /// The product this selection belongs to
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The attribute being selected
    /// </summary>
    public Guid ProductAttributeId { get; set; }

    /// <summary>
    /// The value being selected (null if using CustomValue instead)
    /// </summary>
    public Guid? ProductAttributeValueId { get; set; }

    /// <summary>
    /// Custom/free-form value (used when ProductAttributeValueId is null)
    /// </summary>
    public string? CustomValue { get; set; }

    // Navigation properties

    /// <summary>
    /// The product this selection belongs to
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// The attribute being selected
    /// </summary>
    public virtual ProductAttribute ProductAttribute { get; set; } = null!;

    /// <summary>
    /// The value being selected (null if using CustomValue)
    /// </summary>
    public virtual ProductAttributeValue? ProductAttributeValue { get; set; }
}
