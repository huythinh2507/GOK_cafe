namespace GOKCafe.Domain.Entities;

public class BlogComment : BaseEntity
{
    public Guid BlogId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false; // For moderation
    public Guid? ParentCommentId { get; set; } // For nested replies

    // Navigation properties
    public virtual Blog Blog { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual BlogComment? ParentComment { get; set; }
    public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
}
