namespace GOKCafe.Application.DTOs.ProductComment;

public class ProductCommentDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsApproved { get; set; }
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ReplyCount { get; set; }
    public int Depth { get; set; } // Nesting level (0 for top-level comments)
    public List<ProductCommentDto> Replies { get; set; } = new();
}

public class CreateProductCommentDto
{
    public Guid ProductId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 stars
    public Guid? ParentCommentId { get; set; }
}

public class UpdateProductCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
}

public class CreateReplyDto
{
    public Guid ParentCommentId { get; set; }
    public string Comment { get; set; } = string.Empty;
    // Note: Replies don't have ratings, only parent comments do
}

public class ApproveProductCommentDto
{
    public bool IsApproved { get; set; }
}

public class ProductCommentSummaryDto
{
    public int TotalComments { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Key: rating (1-5), Value: count
}

public class ProductCommentFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsApproved { get; set; } = true;
    public List<int>? Ratings { get; set; }
    public bool? HasReplies { get; set; }
    public bool? HasImages { get; set; }
    public string? Search { get; set; }
}
