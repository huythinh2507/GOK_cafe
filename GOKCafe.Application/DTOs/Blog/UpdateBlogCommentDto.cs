using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Blog;

public class UpdateBlogCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
