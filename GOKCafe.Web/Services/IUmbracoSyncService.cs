using GOKCafe.Application.DTOs.Blog;

namespace GOKCafe.Web.Services;

public interface IUmbracoSyncService
{
    /// <summary>
    /// Sync all published blogs from Umbraco to database
    /// </summary>
    Task<int> SyncAllBlogsAsync();

    /// <summary>
    /// Sync a single blog by its Umbraco node ID
    /// </summary>
    Task<BlogDto?> SyncBlogByNodeIdAsync(int nodeId);

    /// <summary>
    /// Delete blog from database when unpublished in Umbraco
    /// </summary>
    Task<bool> DeleteBlogByNodeIdAsync(int nodeId);
}
