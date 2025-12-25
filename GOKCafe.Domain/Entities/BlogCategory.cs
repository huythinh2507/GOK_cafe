namespace GOKCafe.Domain.Entities;

public class BlogCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
