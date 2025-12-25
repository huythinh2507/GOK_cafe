using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Blog;

public class CreateBlogCommentDto
{
    [Required]
    public Guid BlogId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }
}
