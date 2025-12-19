using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ProductComment;

namespace GOKCafe.Application.Services.Interfaces;

public interface IProductCommentService
{
    Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetProductCommentsAsync(
        Guid productId,
        int pageNumber = 1,
        int pageSize = 10,
        bool? isApproved = true);

    Task<ApiResponse<ProductCommentDto>> GetCommentByIdAsync(Guid id);

    Task<ApiResponse<ProductCommentDto>> CreateCommentAsync(Guid userId, CreateProductCommentDto dto);

    Task<ApiResponse<ProductCommentDto>> UpdateCommentAsync(Guid id, Guid userId, UpdateProductCommentDto dto);

    Task<ApiResponse<bool>> DeleteCommentAsync(Guid id, Guid userId);

    Task<ApiResponse<bool>> ApproveCommentAsync(Guid id, ApproveProductCommentDto dto);

    Task<ApiResponse<ProductCommentSummaryDto>> GetProductCommentSummaryAsync(Guid productId);

    Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetUserCommentsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10);
}
