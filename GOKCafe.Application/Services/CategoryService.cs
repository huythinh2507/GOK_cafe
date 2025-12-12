using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;

namespace GOKCafe.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoryDtos = categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => MapToDto(c))
                .ToList();

            return ApiResponse<List<CategoryDto>>.SuccessResult(categoryDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoryDto>>.FailureResult(
                "An error occurred while retrieving categories",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<CategoryDto>.FailureResult("Category not found");

            var categoryDto = MapToDto(category);
            return ApiResponse<CategoryDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoryDto>.FailureResult(
                "An error occurred while retrieving the category",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryBySlugAsync(string slug)
    {
        try
        {
            var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null)
                return ApiResponse<CategoryDto>.FailureResult("Category not found");

            var categoryDto = MapToDto(category);
            return ApiResponse<CategoryDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoryDto>.FailureResult(
                "An error occurred while retrieving the category",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (existingCategory != null)
                return ApiResponse<CategoryDto>.FailureResult("A category with this name already exists");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = MapToDto(category);
            return ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoryDto>.FailureResult(
                "An error occurred while creating the category",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<CategoryDto>.FailureResult("Category not found");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.Slug = GenerateSlug(dto.Name);
            category.ImageUrl = dto.ImageUrl;
            category.DisplayOrder = dto.DisplayOrder;
            category.IsActive = dto.IsActive;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = MapToDto(category);
            return ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoryDto>.FailureResult(
                "An error occurred while updating the category",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<bool>.FailureResult("Category not found");

            var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return ApiResponse<bool>.FailureResult("Cannot delete category with existing products");

            _unitOfWork.Categories.SoftDelete(category);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deleting the category",
                new List<string> { ex.Message });
        }
    }

    private CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        };
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Trim();
    }
}
