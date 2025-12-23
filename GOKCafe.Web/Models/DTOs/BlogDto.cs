namespace GOKCafe.Web.Models.DTOs
{
    public class BlogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? Author { get; set; }
        public bool IsPublished { get; set; }

        // Additional properties
        public List<string> Tags { get; set; } = new();
        public string? CategoryName { get; set; }
        public int ViewCount { get; set; }
    }
}
