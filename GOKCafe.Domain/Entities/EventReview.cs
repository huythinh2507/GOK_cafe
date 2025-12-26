namespace GOKCafe.Domain.Entities;

public class EventReview : BaseEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    // Review details
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerEmail { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;

    // Moderation
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }

    // Display
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}
