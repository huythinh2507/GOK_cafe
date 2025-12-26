using GOKCafe.Domain.Interfaces;
using GOKCafe.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers.Api
{
    /// <summary>
    /// API controller for Umbraco blog synchronization
    /// </summary>
    [Route("api/v1/blog-sync")]
    [ApiController]
    public class BlogSyncController : ControllerBase
    {
        private readonly IUmbracoSyncService _syncService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BlogSyncController> _logger;

        public BlogSyncController(
            IUmbracoSyncService syncService,
            IUnitOfWork unitOfWork,
            ILogger<BlogSyncController> logger)
        {
            _syncService = syncService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all blogs from database
        /// </summary>
        [HttpGet("blogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                var blogs = await _unitOfWork.Blogs.GetAllAsync();
                var result = blogs.Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    slug = b.Slug,
                    excerpt = b.Excerpt,
                    authorId = b.AuthorId,
                    categoryId = b.CategoryId,
                    isPublished = b.IsPublished,
                    publishedAt = b.PublishedAt,
                    viewCount = b.ViewCount,
                    tags = b.Tags,
                    createdAt = b.CreatedAt,
                    updatedAt = b.UpdatedAt,
                    metaDescription = b.MetaDescription
                }).ToList();

                return Ok(new
                {
                    success = true,
                    count = result.Count,
                    blogs = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs from database");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Manually trigger sync of all blogs from Umbraco
        /// </summary>
        [HttpPost("sync-all")]
        public async Task<IActionResult> SyncAllBlogs()
        {
            try
            {
                var count = await _syncService.SyncAllBlogsAsync();
                return Ok(new
                {
                    success = true,
                    message = $"Successfully synced {count} blogs",
                    syncedCount = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing all blogs");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Manually sync a single blog by Umbraco node ID
        /// </summary>
        [HttpPost("sync/{nodeId}")]
        public async Task<IActionResult> SyncBlogByNodeId(int nodeId)
        {
            try
            {
                var blog = await _syncService.SyncBlogByNodeIdAsync(nodeId);
                if (blog == null)
                {
                    return NotFound(new { success = false, message = "Blog not found or invalid type" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Blog synced successfully",
                    blog = blog
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error syncing blog with node ID: {nodeId}");
                return StatusCode(500, new { success = false, error = ex.Message, innerException = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Get all users to check for authors
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var result = users.Select(u => new
                {
                    id = u.Id,
                    email = u.Email,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    fullName = $"{u.FirstName} {u.LastName}".Trim(),
                    role = u.Role.ToString()
                }).ToList();

                return Ok(new
                {
                    success = true,
                    count = result.Count,
                    users = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
