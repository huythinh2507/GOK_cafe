namespace GOKCafe.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price at the time of adding to cart
    public decimal? DiscountPrice { get; set; } // Discount price at the time of adding

    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;

    // Computed property
    public decimal TotalPrice => (DiscountPrice ?? UnitPrice) * Quantity;
}
