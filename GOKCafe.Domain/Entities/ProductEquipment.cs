namespace GOKCafe.Domain.Entities;

public class ProductEquipment
{
    public Guid ProductId { get; set; }
    public Guid EquipmentId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Equipment Equipment { get; set; } = null!;
}
