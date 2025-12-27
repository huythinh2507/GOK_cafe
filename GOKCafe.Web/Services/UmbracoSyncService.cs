using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace GOKCafe.Web.Services;

public class UmbracoSyncService : IUmbracoSyncService
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UmbracoSyncService> _logger;

    public UmbracoSyncService(
        IUmbracoContextFactory umbracoContextFactory,
        IUnitOfWork unitOfWork,
        ILogger<UmbracoSyncService> logger)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> SyncAllBlogsAsync()
    {
        try
        {
            using var umbracoContext = _umbracoContextFactory.EnsureUmbracoContext();
            var contentCache = umbracoContext.UmbracoContext.Content;

            if (contentCache == null)
            {
                _logger.LogError("Content cache is null");
                return 0;
            }

            // Get all blog items from Umbraco
            var allContent = contentCache.GetAtRoot();
            var blogItems = allContent
                .SelectMany(x => x.DescendantsOrSelf())
                .Where(x => x.ContentType.Alias == "blogsItem")
                .OrderByDescending(x => x.Value<DateTime>("publishDate"))
                .ToList();

            _logger.LogInformation($"Found {blogItems.Count} blog items in Umbraco");

            int syncedCount = 0;

            foreach (var blogItem in blogItems)
            {
                try
                {
                    await SyncBlogItemAsync(blogItem);
                    syncedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error syncing blog item {blogItem.Id}: {blogItem.Name}");
                }
            }

            _logger.LogInformation($"Successfully synced {syncedCount} blogs");
            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing all blogs from Umbraco");
            throw;
        }
    }

    public async Task<BlogDto?> SyncBlogByNodeIdAsync(int nodeId)
    {
        try
        {
            using var umbracoContext = _umbracoContextFactory.EnsureUmbracoContext();
            var contentCache = umbracoContext.UmbracoContext.Content;

            if (contentCache == null)
            {
                _logger.LogError("Content cache is null");
                return null;
            }

            var blogItem = contentCache.GetById(nodeId);
            if (blogItem == null || blogItem.ContentType.Alias != "blogsItem")
            {
                _logger.LogWarning($"Blog item not found or invalid type for node ID: {nodeId}");
                return null;
            }

            var blog = await SyncBlogItemAsync(blogItem);
            return MapToDto(blog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error syncing blog by node ID: {nodeId}");
            throw;
        }
    }

    public async Task<bool> DeleteBlogByNodeIdAsync(int nodeId)
    {
        try
        {
            // Find blog by Umbraco node ID (you might need to add UmbracoNodeId field to Blog entity)
            var blogs = await _unitOfWork.Blogs.GetAllAsync();
            var blog = blogs.FirstOrDefault(b => b.MetaDescription.Contains($"NodeId:{nodeId}"));

            if (blog != null)
            {
                _unitOfWork.Blogs.Remove(blog);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Deleted blog for Umbraco node ID: {nodeId}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting blog by node ID: {nodeId}");
            throw;
        }
    }

    private async Task<Blog> SyncBlogItemAsync(IPublishedContent blogItem)
    {
        // Extract data from Umbraco
        var title = blogItem.Value<string>("title") ?? blogItem.Name;
        var slug = blogItem.UrlSegment;

        // Handle content - support both string and BlockGridModel
        string content = string.Empty;
        var contentValue = blogItem.Value("content");

        if (contentValue is string strContent)
        {
            content = strContent;
        }
        else if (contentValue is Umbraco.Cms.Core.Models.Blocks.BlockGridModel blockGrid)
        {
            // Convert BlockGrid to HTML string
            var htmlParts = new List<string>();
            foreach (var item in blockGrid)
            {
                // Try to get the "content" property from each Rich Text Block
                var blockContent = item.Content.Value<string>("content");
                if (!string.IsNullOrWhiteSpace(blockContent))
                {
                    htmlParts.Add(blockContent);
                }
            }
            content = string.Join("", htmlParts);
        }
        else if (contentValue != null)
        {
            // Fallback: try to convert to string
            content = contentValue.ToString() ?? string.Empty;
        }

        var excerpt = blogItem.Value<string>("excerpt") ?? string.Empty;
        var featuredImage = blogItem.Value<string>("featuredImage") ?? string.Empty;
        var publishDate = blogItem.Value<DateTime?>("publishDate") ?? blogItem.CreateDate;
        var tags = blogItem.Value<string>("tags") ?? string.Empty;
        var author = blogItem.Value<string>("author") ?? "Admin";
        var categoryName = blogItem.Value<string>("category") ?? string.Empty;

        // Check if blog already exists (by slug)
        var existingBlogs = await _unitOfWork.Blogs.GetAllAsync();
        var existingBlog = existingBlogs.FirstOrDefault(b => b.Slug == slug);

        // Get or create author (you might need a default user)
        var users = await _unitOfWork.Users.GetAllAsync();
        var fullName = $"{author}".Trim();
        var authorUser = users.FirstOrDefault(u => $"{u.FirstName} {u.LastName}".Trim().Equals(fullName, StringComparison.OrdinalIgnoreCase))
                        ?? users.FirstOrDefault(u => u.Role == UserRole.Admin);

        if (authorUser == null)
        {
            _logger.LogWarning($"No author found for blog: {title}");
            throw new Exception("Author not found. Please create an admin user first.");
        }

        // Get or create category
        // If no category name provided, use "Uncategorized" as default
        if (string.IsNullOrEmpty(categoryName))
        {
            categoryName = "Uncategorized";
        }

        var categories = await _unitOfWork.BlogCategories.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null)
        {
            category = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = categoryName,
                Slug = categoryName.ToLower().Replace(" ", "-"),
                Description = string.IsNullOrEmpty(categoryName) || categoryName == "Uncategorized"
                    ? "Default category for uncategorized blogs"
                    : $"Category for {categoryName}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _unitOfWork.BlogCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Created new category: {categoryName}");
        }

        if (existingBlog != null)
        {
            // Update existing blog
            existingBlog.Title = title;
            existingBlog.Content = content;
            existingBlog.Excerpt = excerpt;
            existingBlog.FeaturedImageUrl = featuredImage;
            existingBlog.IsPublished = true;
            existingBlog.PublishedAt = publishDate;
            existingBlog.Tags = tags;
            existingBlog.AuthorId = authorUser.Id;
            existingBlog.CategoryId = category.Id;
            existingBlog.MetaTitle = title;
            existingBlog.MetaDescription = $"NodeId:{blogItem.Id} | {excerpt.Substring(0, Math.Min(150, excerpt.Length))}";
            existingBlog.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Blogs.Update(existingBlog);
            _logger.LogInformation($"Updated blog: {title}");
            await _unitOfWork.SaveChangesAsync();
            return existingBlog;
        }
        else
        {
            // Create new blog
            var newBlog = new Blog
            {
                Id = Guid.NewGuid(),
                Title = title,
                Slug = slug,
                Content = content,
                Excerpt = excerpt,
                FeaturedImageUrl = featuredImage,
                AuthorId = authorUser.Id,
                CategoryId = category.Id,
                IsPublished = true,
                PublishedAt = publishDate,
                ViewCount = 0,
                MetaTitle = title,
                MetaDescription = $"NodeId:{blogItem.Id} | {excerpt.Substring(0, Math.Min(150, excerpt.Length))}",
                Tags = tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Blogs.AddAsync(newBlog);
            _logger.LogInformation($"Created new blog: {title}");
            await _unitOfWork.SaveChangesAsync();
            return newBlog;
        }
    }

    private BlogDto MapToDto(Blog blog)
    {
        var authorName = blog.Author != null
            ? $"{blog.Author.FirstName} {blog.Author.LastName}".Trim()
            : "Unknown";

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Slug = blog.Slug,
            Content = blog.Content,
            Excerpt = blog.Excerpt,
            FeaturedImageUrl = blog.FeaturedImageUrl,
            AuthorId = blog.AuthorId,
            AuthorName = authorName,
            CategoryId = blog.CategoryId,
            CategoryName = blog.Category?.Name,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            ViewCount = blog.ViewCount,
            MetaTitle = blog.MetaTitle,
            MetaDescription = blog.MetaDescription,
            Tags = blog.Tags ?? string.Empty,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}
