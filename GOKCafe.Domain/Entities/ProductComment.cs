namespace GOKCafe.Domain.Entities;

public class ProductComment : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 stars
    public bool IsApproved { get; set; } = false; // For moderation
    public Guid? ParentCommentId { get; set; } // For nested replies (optional)

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ProductComment? ParentComment { get; set; }
    public virtual ICollection<ProductComment> Replies { get; set; } = new List<ProductComment>();
}
