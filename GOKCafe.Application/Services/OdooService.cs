using System.Text;
using System.Text.Json;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Odoo;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GOKCafe.Application.Services;

public class OdooService : IOdooService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OdooService> _logger;
    private readonly string _odooUrl;
    private readonly string _odooDatabase;
    private readonly string _odooUsername;
    private readonly string _odooApiKey;

    public OdooService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<OdooService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;

        _odooUrl = _configuration["Odoo:Url"] ?? throw new InvalidOperationException("Odoo URL not configured");
        _odooDatabase = _configuration["Odoo:Database"] ?? throw new InvalidOperationException("Odoo Database not configured");
        _odooUsername = _configuration["Odoo:Username"] ?? throw new InvalidOperationException("Odoo Username not configured");
        _odooApiKey = _configuration["Odoo:ApiKey"] ?? throw new InvalidOperationException("Odoo API Key not configured");
    }

    public async Task<ApiResponse<List<OdooProductDto>>> FetchProductsFromOdooAsync()
    {
        try
        {
            _logger.LogInformation("Fetching products from Odoo...");

            // Fetch products directly using Bearer token (no separate auth needed)
            var products = await FetchProductsFromOdooRPCAsync();

            _logger.LogInformation("Successfully fetched {Count} products from Odoo", products.Count);
            return ApiResponse<List<OdooProductDto>>.SuccessResult(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from Odoo");
            return ApiResponse<List<OdooProductDto>>.FailureResult(
                "An error occurred while fetching products from Odoo",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Synchronizes products from Odoo to the local database.
    /// Optimized for large datasets (1M+ products) with:
    /// - Batch queries to eliminate N+1 problem
    /// - Batched saves to prevent transaction timeouts
    /// - Memory-efficient processing
    /// - Progress logging
    /// </summary>
    public async Task<ApiResponse<OdooSyncResultDto>> SyncProductsFromOdooAsync()
    {
        try
        {
            _logger.LogInformation("Starting Odoo product synchronization...");

            var result = new OdooSyncResultDto();

            // Fetch products from Odoo
            var odooProductsResponse = await FetchProductsFromOdooAsync();
            if (!odooProductsResponse.Success || odooProductsResponse.Data == null)
            {
                return ApiResponse<OdooSyncResultDto>.FailureResult(
                    "Failed to fetch products from Odoo",
                    odooProductsResponse.Errors);
            }

            result.TotalFetched = odooProductsResponse.Data.Count;

            if (odooProductsResponse.Data.Count == 0)
            {
                _logger.LogInformation("No products to sync from Odoo");
                return ApiResponse<OdooSyncResultDto>.SuccessResult(result, "No products to sync");
            }

            // Get default category (or create one for Odoo products)
            var defaultCategory = await GetOrCreateOdooCategoryAsync();

            // OPTIMIZATION: Fetch all existing products matching the slugs in one query
            // This eliminates N+1 query problem
            var slugs = odooProductsResponse.Data
                .Select(p => GenerateSlug($"{p.Name}-{p.Id}"))
                .ToList();

            var existingProducts = await _unitOfWork.Products
                .Where(p => slugs.Contains(p.Slug))
                .ToDictionaryAsync(p => p.Slug, p => p);

            _logger.LogInformation("Found {ExistingCount} existing products out of {TotalCount}",
                existingProducts.Count, odooProductsResponse.Data.Count);

            // Process products in batches to avoid memory issues
            const int batchSize = 100;
            var productsToAdd = new List<Product>();
            var productsToUpdate = new List<Product>();
            var processedCount = 0;
            var totalCount = odooProductsResponse.Data.Count;

            foreach (var odooProduct in odooProductsResponse.Data)
            {
                try
                {
                    var slug = GenerateSlug($"{odooProduct.Name}-{odooProduct.Id}");

                    if (existingProducts.TryGetValue(slug, out var existingProduct))
                    {
                        // Update existing product
                        existingProduct.Name = odooProduct.Name;
                        existingProduct.Description = odooProduct.Description;
                        existingProduct.Price = odooProduct.Price;
                        existingProduct.StockQuantity = odooProduct.StockQuantity;
                        existingProduct.ImageUrl = odooProduct.ImageUrl;
                        existingProduct.IsActive = odooProduct.Active;
                        existingProduct.UpdatedAt = DateTime.UtcNow;

                        productsToUpdate.Add(existingProduct);
                        result.Updated++;
                    }
                    else
                    {
                        // Create new product
                        var newProduct = new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = odooProduct.Name,
                            Description = odooProduct.Description,
                            Slug = slug,
                            Price = odooProduct.Price,
                            StockQuantity = odooProduct.StockQuantity,
                            ImageUrl = odooProduct.ImageUrl,
                            IsActive = odooProduct.Active,
                            IsFeatured = false,
                            CategoryId = defaultCategory.Id,
                            DisplayOrder = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        productsToAdd.Add(newProduct);
                        result.Created++;
                    }

                    processedCount++;

                    // Save in batches to avoid transaction timeout
                    if ((productsToAdd.Count + productsToUpdate.Count) >= batchSize)
                    {
                        await SaveBatchAsync(productsToAdd, productsToUpdate);
                        _logger.LogInformation("Progress: {Processed}/{Total} products processed ({Percentage:F1}%)",
                            processedCount, totalCount, (processedCount * 100.0 / totalCount));
                        productsToAdd.Clear();
                        productsToUpdate.Clear();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing product: {ProductName}", odooProduct.Name);
                    result.Errors.Add($"Error syncing {odooProduct.Name}: {ex.Message}");
                    result.Skipped++;
                }
            }

            // Save remaining products
            if (productsToAdd.Count > 0 || productsToUpdate.Count > 0)
            {
                await SaveBatchAsync(productsToAdd, productsToUpdate);
            }

            _logger.LogInformation(
                "Odoo sync completed. Created: {Created}, Updated: {Updated}, Skipped: {Skipped}",
                result.Created, result.Updated, result.Skipped);

            return ApiResponse<OdooSyncResultDto>.SuccessResult(result, "Products synchronized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing products from Odoo");

            var errorMessages = new List<string> { ex.Message };
            if (ex.InnerException != null)
            {
                errorMessages.Add($"Inner exception: {ex.InnerException.Message}");
                _logger.LogError(ex.InnerException, "Inner exception details");
            }

            return ApiResponse<OdooSyncResultDto>.FailureResult(
                "An error occurred while syncing products from Odoo",
                errorMessages);
        }
    }

    private async Task SaveBatchAsync(List<Product> productsToAdd, List<Product> productsToUpdate)
    {
        if (productsToAdd.Count > 0)
        {
            await _unitOfWork.Products.AddRangeAsync(productsToAdd);
        }

        if (productsToUpdate.Count > 0)
        {
            _unitOfWork.Products.UpdateRange(productsToUpdate);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Saved batch: {Added} added, {Updated} updated",
            productsToAdd.Count, productsToUpdate.Count);
    }

    private async Task<List<OdooProductDto>> FetchProductsFromOdooRPCAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var rpcUrl = $"{_odooUrl}/json/2/product.product/search_read";

            // Add required headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_odooApiKey}");
            client.DefaultRequestHeaders.Add("X-Odoo-Database", _odooDatabase);

            var payload = new
            {
                context = new { lang = "en_US" },
                domain = new object[] { },
                fields = new[] { "id", "name"},
                limit = 20
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Fetching products from Odoo: {Url}", rpcUrl);
            var response = await client.PostAsync(rpcUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Odoo product fetch failed. Status: {Status}, Response: {Response}",
                    response.StatusCode, errorContent);
                throw new ArgumentException($"Odoo API returned {response.StatusCode}: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Odoo response: {Response}", responseBody);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var simpleProducts = JsonSerializer.Deserialize<List<OdooSimpleProductDto>>(responseBody, options) ?? throw new ArgumentException($"No products found");

            var products = simpleProducts.Select(p => new OdooProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = null,
                Price = 0,
                StockQuantity = 0,
                ImageUrl = null,
                CategoryName = null,
                Active = true
            }).ToList();

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from Odoo RPC");
            throw;
        }
    }

    private async Task<Category> GetOrCreateOdooCategoryAsync()
    {
        const string categoryName = "Odoo Products";
        var category = await _unitOfWork.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryName);

        if (category == null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName,
                Slug = GenerateSlug(categoryName),
                Description = "Products imported from Odoo",
                IsActive = true,
                DisplayOrder = 999,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            // Don't save here - will be saved with the products in the main transaction
        }

        return category;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Trim();
    }
}
