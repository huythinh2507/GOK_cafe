using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Blog;

public class UpdateBlogDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Excerpt { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    public Guid? CategoryId { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    [MaxLength(200)]
    public string MetaTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MetaDescription { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;
}
