using GOKCafe.Application.DTOs.Equipment;
using GOKCafe.Application.DTOs.FlavourProfile;

namespace GOKCafe.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// Primary/featured image URL for quick access (used in product lists, thumbnails).
    /// Should match the image marked with IsPrimary=true in the Images collection.
    /// For Odoo-synced products, this contains the main product image from Odoo.
    /// </summary>
    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Product Type ID for dynamic attribute system
    /// </summary>
    public Guid? ProductTypeId { get; set; }

    public string? ShortDescription { get; set; }
    public string? TastingNote { get; set; }
    public string? Region { get; set; }
    public string? Process { get; set; }

    /// <summary>
    /// Complete gallery of product images for detail views.
    /// The image with IsPrimary=true should match the ImageUrl field.
    /// </summary>
    public List<ProductImageDto> Images { get; set; } = new();

    public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
    public List<EquipmentDto> Equipments { get; set; } = new();

    /// <summary>
    /// Available size options (e.g., ["250g", "500g", "1kg"] for coffee or ["S", "M", "L", "XL"] for clothes)
    /// </summary>
    public List<string>? AvailableSizes { get; set; }

    /// <summary>
    /// Available grind options for coffee (e.g., ["Whole Bean", "French Press", "Filter", "Espresso"])
    /// </summary>
    public List<string>? AvailableGrinds { get; set; }

    /// <summary>
    /// Available color options for clothes (e.g., ["Black", "White", "Navy", "Grey"])
    /// </summary>
    public List<string>? AvailableColors { get; set; }

    /// <summary>
    /// Material information for clothes (e.g., "100% Cotton", "Polyester")
    /// </summary>
    public string? Material { get; set; }

    /// <summary>
    /// Style/Fit information for clothes (e.g., "Regular Fit", "Slim Fit")
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Dynamic product attributes with their selected values
    /// </summary>
    public List<ProductAttributeDisplayDto>? ProductAttributes { get; set; }
}

/// <summary>
/// Represents a product attribute for display on product details page
/// </summary>
public class ProductAttributeDisplayDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool AllowMultipleSelection { get; set; }
    public bool IsRequired { get; set; }
    public List<ProductAttributeValueDisplayDto> Values { get; set; } = new();
}

/// <summary>
/// Represents a product attribute value for display
/// </summary>
public class ProductAttributeValueDisplayDto
{
    public Guid? ValueId { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

/// <summary>
/// Represents a single product image in the gallery.
/// </summary>
public class ProductImageDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// URL to the image file.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Alternative text for accessibility and SEO.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Order of display in the image gallery (lower number = displayed first).
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Indicates if this is the primary/featured image.
    /// The primary image should also be stored in Product.ImageUrl for quick access.
    /// </summary>
    public bool IsPrimary { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public string? Sku { get; set; }

    // Product Type (for dynamic attributes)
    public Guid? ProductTypeId { get; set; }

    // Dynamic product attribute selections
    public List<ProductAttributeSelectionDto>? ProductAttributeSelections { get; set; }

    // Product images (multiple images support)
    public List<CreateProductImageDto>? Images { get; set; }

    // Legacy fields (for backward compatibility - can be removed later)
    public string? TastingNote { get; set; }
    public string? Region { get; set; }
    public string? Process { get; set; }
    public List<string>? AvailableSizes { get; set; }
    public List<string>? AvailableGrinds { get; set; }
    public List<string>? AvailableColors { get; set; }
    public string? Material { get; set; }
    public string? Style { get; set; }

    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}

/// <summary>
/// DTO for creating a product image
/// </summary>
public class CreateProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

/// <summary>
/// DTO for product attribute selection (used when creating/updating products)
/// </summary>
public class ProductAttributeSelectionDto
{
    public Guid ProductAttributeId { get; set; }
    public Guid? ProductAttributeValueId { get; set; }
    public string? CustomValue { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public string? Sku { get; set; }

    // Product Type (for dynamic attributes)
    public Guid? ProductTypeId { get; set; }

    // Dynamic product attribute selections
    public List<ProductAttributeSelectionDto>? ProductAttributeSelections { get; set; }

    // Legacy fields (for backward compatibility - can be removed later)
    public string? TastingNote { get; set; }
    public string? Region { get; set; }
    public string? Process { get; set; }
    public List<string>? AvailableSizes { get; set; }
    public List<string>? AvailableGrinds { get; set; }
    public List<string>? AvailableColors { get; set; }
    public string? Material { get; set; }
    public string? Style { get; set; }

    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}
