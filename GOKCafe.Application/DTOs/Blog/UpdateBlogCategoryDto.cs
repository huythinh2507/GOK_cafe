using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Blog;

public class UpdateBlogCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
