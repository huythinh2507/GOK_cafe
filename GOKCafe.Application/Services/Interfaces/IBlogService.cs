using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface IBlogService
{
    Task<ApiResponse<PaginatedResponse<BlogDto>>> GetBlogsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isPublished = null,
        string? tag = null,
        Guid? authorId = null);

    Task<ApiResponse<BlogDto>> GetBlogByIdAsync(Guid id);
    Task<ApiResponse<BlogDto>> GetBlogBySlugAsync(string slug);
    Task<ApiResponse<BlogDto>> CreateBlogAsync(CreateBlogDto dto, Guid authorId);
    Task<ApiResponse<BlogDto>> UpdateBlogAsync(Guid id, UpdateBlogDto dto);
    Task<ApiResponse<bool>> DeleteBlogAsync(Guid id);
    Task<ApiResponse<bool>> IncrementViewCountAsync(Guid id);
    Task<ApiResponse<List<string>>> GetAllTagsAsync();
}
