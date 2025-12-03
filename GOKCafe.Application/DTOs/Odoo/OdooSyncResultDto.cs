namespace GOKCafe.Application.DTOs.Odoo;

/// <summary>
/// Result of Odoo product synchronization
/// </summary>
public class OdooSyncResultDto
{
    public int TotalFetched { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
