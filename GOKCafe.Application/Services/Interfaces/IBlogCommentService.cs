using GOKCafe.Application.DTOs.Blog;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface IBlogCommentService
{
    Task<ApiResponse<PaginatedResponse<BlogCommentDto>>> GetBlogCommentsAsync(
        Guid blogId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = null);

    Task<ApiResponse<BlogCommentDto>> GetCommentByIdAsync(Guid id);
    Task<ApiResponse<BlogCommentDto>> CreateCommentAsync(CreateBlogCommentDto dto, Guid userId);
    Task<ApiResponse<BlogCommentDto>> UpdateCommentAsync(Guid id, UpdateBlogCommentDto dto, Guid userId);
    Task<ApiResponse<bool>> DeleteCommentAsync(Guid id, Guid userId);
    Task<ApiResponse<bool>> ApproveCommentAsync(Guid id);
    Task<ApiResponse<PaginatedResponse<BlogCommentDto>>> GetUserCommentsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10);
}
