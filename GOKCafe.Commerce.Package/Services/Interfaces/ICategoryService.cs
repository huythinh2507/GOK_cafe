using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<CategoryDto>> GetCategoryBySlugAsync(string slug);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
}
