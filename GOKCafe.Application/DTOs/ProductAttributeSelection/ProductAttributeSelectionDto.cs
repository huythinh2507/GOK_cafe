namespace GOKCafe.Application.DTOs.ProductAttributeSelection;

public class ProductAttributeSelectionDto
{
    public Guid ProductAttributeId { get; set; }
    public List<Guid> ProductAttributeValueIds { get; set; } = new();
}

public class ProductAttributeSelectionDisplayDto
{
    public Guid Id { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string AttributeDisplayName { get; set; } = string.Empty;
    public List<string> SelectedValues { get; set; } = new();
}
