namespace GOKCafe.Domain.Entities;

/// <summary>
/// Represents a revoked JWT token (for logout functionality)
/// </summary>
public class RevokedToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? UserId { get; set; }
    public string? Reason { get; set; }
}
