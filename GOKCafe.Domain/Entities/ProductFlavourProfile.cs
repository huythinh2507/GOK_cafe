namespace GOKCafe.Domain.Entities;

public class ProductFlavourProfile
{
    public Guid ProductId { get; set; }
    public Guid FlavourProfileId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual FlavourProfile FlavourProfile { get; set; } = null!;
}
