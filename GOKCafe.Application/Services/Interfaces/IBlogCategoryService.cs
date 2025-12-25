using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface IBlogCategoryService
{
    Task<ApiResponse<PaginatedResponse<BlogCategoryDto>>> GetCategoriesAsync(
        int pageNumber = 1,
        int pageSize = 10);

    Task<ApiResponse<List<BlogCategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResponse<BlogCategoryDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<BlogCategoryDto>> GetCategoryBySlugAsync(string slug);
    Task<ApiResponse<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryDto dto);
    Task<ApiResponse<BlogCategoryDto>> UpdateCategoryAsync(Guid id, UpdateBlogCategoryDto dto);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
}
