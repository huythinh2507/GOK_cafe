using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;

namespace GOKCafe.Web.Services.Implementations
{
    public class ProductCommentService : IProductCommentService
    {
        private readonly IApiHttpClient _apiClient;
        private readonly ILogger<ProductCommentService> _logger;

        public ProductCommentService(
            IApiHttpClient apiClient,
            ILogger<ProductCommentService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<PaginatedResponse<ProductCommentDto>> GetProductCommentsAsync(
            Guid productId,
            ProductCommentFilterDto filter)
        {
            try
            {
                var response = await _apiClient.GetProductCommentsAsync(productId, filter);

                if (response.Success && response.Data != null)
                {
                    return new PaginatedResponse<ProductCommentDto>
                    {
                        Items = response.Data.Items,
                        PageNumber = response.Data.PageNumber,
                        PageSize = response.Data.PageSize,
                        TotalItems = response.Data.TotalItems,
                        TotalPages = response.Data.TotalPages
                    };
                }

                _logger.LogWarning("Failed to get product comments from API: {Message}", response.Message);
                return new PaginatedResponse<ProductCommentDto>
                {
                    Items = new List<ProductCommentDto>(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product comments for product {ProductId}", productId);
                return new PaginatedResponse<ProductCommentDto>
                {
                    Items = new List<ProductCommentDto>(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0
                };
            }
        }

        public async Task<ProductCommentSummaryDto?> GetProductCommentSummaryAsync(Guid productId)
        {
            try
            {
                var response = await _apiClient.GetProductCommentSummaryAsync(productId);

                if (response.Success && response.Data != null)
                {
                    return response.Data;
                }

                _logger.LogWarning("Failed to get comment summary from API: {Message}", response.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment summary for product {ProductId}", productId);
                return null;
            }
        }
    }
}
