using GOKCafe.Web.Models.DTOs;
using GOKCafe.Web.Services.Interfaces;
using System.Text.Json;

namespace GOKCafe.Web.Services.Implementations
{
    public class ApiHttpClient : IApiHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiHttpClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiHttpClient(HttpClient httpClient, ILogger<ApiHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            List<Guid>? categoryIds = null,
            bool? isFeatured = null,
            string? search = null,
            List<Guid>? flavourProfileIds = null,
            List<Guid>? equipmentIds = null,
            bool? inStock = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (isFeatured.HasValue)
                    queryParams.Add($"isFeatured={isFeatured.Value}");

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (categoryIds != null && categoryIds.Any())
                {
                    foreach (var catId in categoryIds)
                        queryParams.Add($"categoryIds={catId}");
                }

                if (flavourProfileIds != null && flavourProfileIds.Any())
                {
                    foreach (var fpId in flavourProfileIds)
                        queryParams.Add($"flavourProfileIds={fpId}");
                }

                if (equipmentIds != null && equipmentIds.Any())
                {
                    foreach (var eqId in equipmentIds)
                        queryParams.Add($"equipmentIds={eqId}");
                }

                if (inStock.HasValue)
                    queryParams.Add($"inStock={inStock.Value}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/api/v1/products?{query}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API returned {StatusCode} when fetching products", response.StatusCode);
                    return new ApiResponse<PaginatedResponse<ProductDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<PaginatedResponse<ProductDto>>>(content, _jsonOptions);

                return result ?? new ApiResponse<PaginatedResponse<ProductDto>>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from API");
                return new ApiResponse<PaginatedResponse<ProductDto>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<ProductFiltersDto>> GetProductFiltersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/products/filters");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<ProductFiltersDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<ProductFiltersDto>>(content, _jsonOptions);

                return result ?? new ApiResponse<ProductFiltersDto>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product filters from API");
                return new ApiResponse<ProductFiltersDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/products/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, _jsonOptions);

                return result ?? new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {Id} from API", id);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/categories");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<List<CategoryDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(content, _jsonOptions);

                return result ?? new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories from API");
                return new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/categories/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, _jsonOptions);

                return result ?? new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category {Id} from API", id);
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PaginatedResponse<ProductCommentDto>>> GetProductCommentsAsync(
            Guid productId,
            ProductCommentFilterDto filter)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={filter.PageNumber}",
                    $"pageSize={filter.PageSize}"
                };

                if (filter.IsApproved.HasValue)
                    queryParams.Add($"isApproved={filter.IsApproved.Value}");

                if (filter.Ratings != null && filter.Ratings.Any())
                {
                    foreach (var rating in filter.Ratings)
                        queryParams.Add($"ratings={rating}");
                }

                if (filter.HasReplies.HasValue)
                    queryParams.Add($"hasReplies={filter.HasReplies.Value}");

                if (filter.HasImages.HasValue)
                    queryParams.Add($"hasImages={filter.HasImages.Value}");

                if (!string.IsNullOrWhiteSpace(filter.Search))
                    queryParams.Add($"search={Uri.EscapeDataString(filter.Search)}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/api/v1/products/{productId}/comments?{query}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API returned {StatusCode} when fetching product comments", response.StatusCode);
                    return new ApiResponse<PaginatedResponse<ProductCommentDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<PaginatedResponse<ProductCommentDto>>>(content, _jsonOptions);

                return result ?? new ApiResponse<PaginatedResponse<ProductCommentDto>>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product comments from API for product {ProductId}", productId);
                return new ApiResponse<PaginatedResponse<ProductCommentDto>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<ProductCommentSummaryDto>> GetProductCommentSummaryAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/products/{productId}/comments/summary");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API returned {StatusCode} when fetching comment summary", response.StatusCode);
                    return new ApiResponse<ProductCommentSummaryDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<ProductCommentSummaryDto>>(content, _jsonOptions);

                return result ?? new ApiResponse<ProductCommentSummaryDto>
                {
                    Success = false,
                    Message = "Failed to deserialize response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment summary from API for product {ProductId}", productId);
                return new ApiResponse<ProductCommentSummaryDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
