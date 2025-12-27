using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface IProductCommentService
    {
        Task<PaginatedResponse<ProductCommentDto>> GetProductCommentsAsync(
            Guid productId,
            ProductCommentFilterDto filter);

        Task<ProductCommentSummaryDto?> GetProductCommentSummaryAsync(Guid productId);
    }
}
