namespace GOKCafe.Domain.Entities;

public class FlavourProfile : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ProductFlavourProfile> ProductFlavourProfiles { get; set; } = new List<ProductFlavourProfile>();
}
