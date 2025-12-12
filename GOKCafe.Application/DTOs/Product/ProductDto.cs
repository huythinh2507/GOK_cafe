using GOKCafe.Application.DTOs.Equipment;
using GOKCafe.Application.DTOs.FlavourProfile;

namespace GOKCafe.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
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
    /// Available size options (e.g., ["250g", "500g", "1kg"])
    /// </summary>
    public List<string>? AvailableSizes { get; set; }

    /// <summary>
    /// Available grind options (e.g., ["Whole Bean", "French Press", "Filter", "Espresso"])
    /// </summary>
    public List<string>? AvailableGrinds { get; set; }
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
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> FlavourProfileIds { get; set; } = new();
    public List<Guid> EquipmentIds { get; set; } = new();
}
