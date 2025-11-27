namespace GOKCafe.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; } // For anonymous users
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // Computed properties
    public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
    public int TotalItems => CartItems.Sum(item => item.Quantity);
}
