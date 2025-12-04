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
            string? search = null)
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

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/api/v1/products?{query}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode} when fetching products");
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
                _logger.LogError(ex, $"Error fetching product {id} from API");
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
                _logger.LogError(ex, $"Error fetching category {id} from API");
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
