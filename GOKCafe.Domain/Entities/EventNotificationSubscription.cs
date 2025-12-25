namespace GOKCafe.Domain.Entities;

public class EventNotificationSubscription : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? City { get; set; } // Specific city or "All Locations"
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
}
