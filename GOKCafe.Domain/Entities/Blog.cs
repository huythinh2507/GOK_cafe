namespace GOKCafe.Domain.Entities;

public class Blog : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // Comma-separated tags

    // Navigation properties
    public virtual User Author { get; set; } = null!;
    public virtual BlogCategory? Category { get; set; }
    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
}
