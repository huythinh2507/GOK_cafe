namespace GOKCafe.Application.DTOs.Blog;

public class BlogCommentDto
{
    public Guid Id { get; set; }
    public Guid BlogId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<BlogCommentDto> Replies { get; set; } = new();
}
