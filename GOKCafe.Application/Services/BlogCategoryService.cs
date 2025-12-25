using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class BlogCategoryService : IBlogCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CategoryCacheKeyPrefix = "blog-category:";
    private const string CategoriesCacheKeyPrefix = "blog-categories:";
    private const string CategorySlugCacheKeyPrefix = "blog-category-slug:";
    private const string AllCategoriesCacheKey = "blog-categories:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public BlogCategoryService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<BlogCategoryDto>>> GetCategoriesAsync(
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var cacheKey = $"{CategoriesCacheKeyPrefix}page:{pageNumber}:size:{pageSize}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<BlogCategoryDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var query = _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs);

            var totalItems = await query.CountAsync();
            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categoryDtos = categories.Select(MapToDto).ToList();

            var response = ApiResponse<PaginatedResponse<BlogCategoryDto>>.SuccessResult(
                new PaginatedResponse<BlogCategoryDto>
                {
                    Items = categoryDtos,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<BlogCategoryDto>>.FailureResult(
                $"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<BlogCategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<List<BlogCategoryDto>>>(AllCategoriesCacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var categories = await _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categoryDtos = categories.Select(MapToDto).ToList();
            var response = ApiResponse<List<BlogCategoryDto>>.SuccessResult(categoryDtos);

            await _cacheService.SetAsync(AllCategoriesCacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<BlogCategoryDto>>.FailureResult(
                $"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{CategoryCacheKeyPrefix}{id}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<BlogCategoryDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var category = await _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return ApiResponse<BlogCategoryDto>.FailureResult("Category not found");

            var categoryDto = MapToDto(category);
            var response = ApiResponse<BlogCategoryDto>.SuccessResult(categoryDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCategoryDto>.FailureResult(
                $"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> GetCategoryBySlugAsync(string slug)
    {
        try
        {
            var cacheKey = $"{CategorySlugCacheKeyPrefix}{slug}";
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<BlogCategoryDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            var category = await _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return ApiResponse<BlogCategoryDto>.FailureResult("Category not found");

            var categoryDto = MapToDto(category);
            var response = ApiResponse<BlogCategoryDto>.SuccessResult(categoryDto);

            await _cacheService.SetAsync(cacheKey, response, CacheExpiration);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCategoryDto>.FailureResult(
                $"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryDto dto)
    {
        try
        {
            // Auto-generate slug if not provided
            var slug = string.IsNullOrWhiteSpace(dto.Slug)
                ? GenerateSlug(dto.Name)
                : dto.Slug;

            var existingCategory = await _unitOfWork.BlogCategories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (existingCategory != null)
                return ApiResponse<BlogCategoryDto>.FailureResult("A category with this slug already exists");

            var category = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BlogCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateCategoryCache(category.Id);

            var createdCategory = await _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            var categoryDto = MapToDto(createdCategory!);
            return ApiResponse<BlogCategoryDto>.SuccessResult(categoryDto, "Category created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCategoryDto>.FailureResult(
                $"Error creating category: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> UpdateCategoryAsync(Guid id, UpdateBlogCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.BlogCategories.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<BlogCategoryDto>.FailureResult("Category not found");

            // Auto-generate slug if not provided
            var slug = string.IsNullOrWhiteSpace(dto.Slug)
                ? GenerateSlug(dto.Name)
                : dto.Slug;

            if (category.Slug != slug)
            {
                var existingCategory = await _unitOfWork.BlogCategories.FirstOrDefaultAsync(c => c.Slug == slug && c.Id != id);
                if (existingCategory != null)
                    return ApiResponse<BlogCategoryDto>.FailureResult("A category with this slug already exists");
            }

            category.Name = dto.Name;
            category.Slug = slug;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.BlogCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateCategoryCache(category.Id);

            var updatedCategory = await _unitOfWork.BlogCategories.GetQueryable()
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            var categoryDto = MapToDto(updatedCategory!);
            return ApiResponse<BlogCategoryDto>.SuccessResult(categoryDto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<BlogCategoryDto>.FailureResult(
                $"Error updating category: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await _unitOfWork.BlogCategories.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<bool>.FailureResult("Category not found");

            var blogsInCategory = await _unitOfWork.Blogs.CountAsync(b => b.CategoryId == id);
            if (blogsInCategory > 0)
                return ApiResponse<bool>.FailureResult($"Cannot delete category. {blogsInCategory} blog(s) are using this category.");

            _unitOfWork.BlogCategories.SoftDelete(category);
            await _unitOfWork.SaveChangesAsync();

            await InvalidateCategoryCache(category.Id);

            return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                $"Error deleting category: {ex.Message}");
        }
    }

    private BlogCategoryDto MapToDto(BlogCategory category)
    {
        return new BlogCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            BlogCount = category.Blogs?.Count(b => b.IsPublished) ?? 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private async Task InvalidateCategoryCache(Guid categoryId)
    {
        await _cacheService.RemoveAsync($"{CategoryCacheKeyPrefix}{categoryId}");

        var category = await _unitOfWork.BlogCategories.GetByIdAsync(categoryId);
        if (category != null)
        {
            await _cacheService.RemoveAsync($"{CategorySlugCacheKeyPrefix}{category.Slug}");
        }

        await _cacheService.RemoveAsync(AllCategoriesCacheKey);
        await _cacheService.RemoveAsync(CategoriesCacheKeyPrefix);
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
