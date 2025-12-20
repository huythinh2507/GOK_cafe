namespace GOKCafe.Web.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ProductTypeId { get; set; }

        // Additional product detail properties
        public string? TastingNote { get; set; }
        public string? Region { get; set; }
        public string? Process { get; set; }

        // Properties from API response
        public List<ProductImageDto> Images { get; set; } = new();
        public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
        public List<EquipmentDto> Equipments { get; set; } = new();

        // Available product options
        public List<string>? AvailableSizes { get; set; }
        public List<string>? AvailableGrinds { get; set; }

        // Dynamic product attributes
        public List<ProductAttributeDisplayDto>? ProductAttributes { get; set; }
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class ProductAttributeDisplayDto
    {
        public Guid AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool AllowMultipleSelection { get; set; }
        public bool IsRequired { get; set; }
        public List<ProductAttributeValueDisplayDto> Values { get; set; } = new();
    }

    public class ProductAttributeValueDisplayDto
    {
        public Guid? ValueId { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class FlavourProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class EquipmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ProductFiltersDto
    {
        public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
        public List<EquipmentDto> Equipments { get; set; } = new();
        public AvailabilityDto Availability { get; set; } = new();
    }

    public class AvailabilityDto
    {
        public int InStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}
