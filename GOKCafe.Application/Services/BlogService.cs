using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string BlogCacheKeyPrefix = "blog:";
    private const string BlogsCacheKeyPrefix = "blogs:";
    private const string BlogSlugCacheKeyPrefix = "blog-slug:";
    private const string BlogTagsCacheKey = "blog-tags:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public BlogService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<BlogDto>>> GetBlogsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isPublished = null,
        string? tag = null,
        Guid? authorId = null)
    {
        try
        {
            var cacheKey = $"{BlogsCacheKeyPrefix}page:{pageNumber}:size:{pageSize}:search:{searchTerm}:cat:{categoryId}:pub:{isPublished}:tag:{tag}:auth:{authorId}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<BlogDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.Blogs.GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    b.Title.Contains(searchTerm) ||
                    b.Content.Contains(searchTerm) ||
                    b.Excerpt.Contains(searchTerm) ||
                    b.Tags.Contains(searchTerm));
            }

            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId.Value);

            if (isPublished.HasValue)
                query = query.Where(b => b.IsPublished == isPublished.Value);

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(b => b.Tags.Contains(tag));

            if (authorId.HasValue)
                query = query.Where(b => b.AuthorId == authorId.Value);

            var totalItems = await query.CountAsync();
            var blogs = await query
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var blogDtos = blogs.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<BlogDto>>.SuccessResult(
                new PaginatedResponse<BlogDto>
                {
                    Items = blogDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<BlogDto>>.FailureResult(
                $"Error retrieving blogs: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogDto>> GetBlogByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{BlogCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<BlogDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var blog = await _unitOfWork.Blogs.GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return ApiResponse<BlogDto>.FailureResult("Blog not found");

            var blogDto = MapToDto(blog);
            var response = ApiResponse<BlogDto>.SuccessResult(blogDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogDto>.FailureResult(
                $"Error retrieving blog: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogDto>> GetBlogBySlugAsync(string slug)
    {
        try
        {
            var cacheKey = $"{BlogSlugCacheKeyPrefix}{slug}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<BlogDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var blog = await _unitOfWork.Blogs.GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blog == null)
                return ApiResponse<BlogDto>.FailureResult("Blog not found");

            var blogDto = MapToDto(blog);
            var response = ApiResponse<BlogDto>.SuccessResult(blogDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogDto>.FailureResult(
                $"Error retrieving blog: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogDto>> CreateBlogAsync(CreateBlogDto dto, Guid authorId)
    {
        try
        {
            var author = await _unitOfWork.Users.GetByIdAsync(authorId);
            if (author == null)
                return ApiResponse<BlogDto>.FailureResult("Author not found");

            // Auto-generate slug if not provided
            var slug = string.IsNullOrWhiteSpace(dto.Slug)
                ? GenerateSlug(dto.Title)
                : dto.Slug;

            var existingBlog = await _unitOfWork.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
            if (existingBlog != null)
                return ApiResponse<BlogDto>.FailureResult("A blog with this slug already exists");

            if (dto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.BlogCategories.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                    return ApiResponse<BlogDto>.FailureResult("Category not found");
            }

            var blog = new Blog
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                Excerpt = dto.Excerpt,
                FeaturedImageUrl = dto.FeaturedImageUrl,
                AuthorId = authorId,
                CategoryId = dto.CategoryId,
                IsPublished = dto.IsPublished,
                PublishedAt = dto.PublishedAt ?? (dto.IsPublished ? DateTime.UtcNow : null),
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                Tags = dto.Tags,
                ViewCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Blogs.AddAsync(blog);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCache(blog.Id);

            var createdBlog = await _unitOfWork.Blogs.GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == blog.Id);

            var blogDto = MapToDto(createdBlog!);
            return ApiResponse<BlogDto>.SuccessResult(blogDto, "Blog created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogDto>.FailureResult(
                $"Error creating blog: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogDto>> UpdateBlogAsync(Guid id, UpdateBlogDto dto)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
                return ApiResponse<BlogDto>.FailureResult("Blog not found");

            if (blog.Slug != dto.Slug)
            {
                var existingBlog = await _unitOfWork.Blogs.FirstOrDefaultAsync(b => b.Slug == dto.Slug && b.Id != id);
                if (existingBlog != null)
                    return ApiResponse<BlogDto>.FailureResult("A blog with this slug already exists");
            }

            if (dto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.BlogCategories.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                    return ApiResponse<BlogDto>.FailureResult("Category not found");
            }

            blog.Title = dto.Title;
            blog.Slug = dto.Slug;
            blog.Content = dto.Content;
            blog.Excerpt = dto.Excerpt;
            blog.FeaturedImageUrl = dto.FeaturedImageUrl;
            blog.CategoryId = dto.CategoryId;
            blog.IsPublished = dto.IsPublished;
            blog.PublishedAt = dto.PublishedAt ?? (dto.IsPublished && blog.PublishedAt == null ? DateTime.UtcNow : blog.PublishedAt);
            blog.MetaTitle = dto.MetaTitle;
            blog.MetaDescription = dto.MetaDescription;
            blog.Tags = dto.Tags;
            blog.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Blogs.Update(blog);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCache(blog.Id);

            var updatedBlog = await _unitOfWork.Blogs.GetQueryable()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == blog.Id);

            var blogDto = MapToDto(updatedBlog!);
            return ApiResponse<BlogDto>.SuccessResult(blogDto, "Blog updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogDto>.FailureResult(
                $"Error updating blog: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBlogAsync(Guid id)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
                return ApiResponse<bool>.FailureResult("Blog not found");

            _unitOfWork.Blogs.SoftDelete(blog);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCache(blog.Id);

            return ApiResponse<bool>.SuccessResult(true, "Blog deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error deleting blog: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> IncrementViewCountAsync(Guid id)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
                return ApiResponse<bool>.FailureResult("Blog not found");

            blog.ViewCount++;
            _unitOfWork.Blogs.Update(blog);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateBlogCache(blog.Id);

            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error incrementing view count: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<string>>> GetAllTagsAsync()
    {
        try
        {
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<List<string>>>(BlogTagsCacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var blogs = await _unitOfWork.Blogs.GetAllAsync();
            var tags = blogs
                .Where(b => !string.IsNullOrWhiteSpace(b.Tags))
                .SelectMany(b => b.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var response = ApiResponse<List<string>>.SuccessResult(tags);
            await _cacheService.SetAsync(BlogTagsCacheKey, response, CacheExpiration);

            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<string>>.FailureResult(
                $"Error retrieving tags: {ex.Message}");
        }
    }

    private BlogDto MapToDto(Blog blog)
    {
        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Slug = blog.Slug,
            Content = blog.Content,
            Excerpt = blog.Excerpt,
            FeaturedImageUrl = blog.FeaturedImageUrl,
            AuthorId = blog.AuthorId,
            AuthorName = blog.Author != null ? $"{blog.Author.FirstName} {blog.Author.LastName}".Trim() : string.Empty,
            CategoryId = blog.CategoryId,
            CategoryName = blog.Category?.Name,
            IsPublished = blog.IsPublished,
            PublishedAt = blog.PublishedAt,
            ViewCount = blog.ViewCount,
            MetaTitle = blog.MetaTitle,
            MetaDescription = blog.MetaDescription,
            Tags = blog.Tags,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            CommentCount = blog.Comments?.Count(c => c.IsApproved) ?? 0
        };
    }

    private async Task InvalidateBlogCache(Guid blogId)
    {
        await _cacheService.RemoveAsync($"{BlogCacheKeyPrefix}{blogId}");

        var blog = await _unitOfWork.Blogs.GetByIdAsync(blogId);
        if (blog != null)
        {
            await _cacheService.RemoveAsync($"{BlogSlugCacheKeyPrefix}{blog.Slug}");
        }

        var allKeys = new[] { BlogsCacheKeyPrefix, BlogTagsCacheKey };
        foreach (var keyPrefix in allKeys)
        {
            await _cacheService.RemoveAsync(keyPrefix);
        }
    }

    private string GenerateSlug(string text)
    {
        return text.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("--", "-")
            .Trim('-');
    }
}
