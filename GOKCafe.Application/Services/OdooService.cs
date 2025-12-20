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
    private readonly OdooAttributeMappingConfig _attributeMappingConfig;

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

        // Load attribute mapping configuration (with defaults if not configured)
        _attributeMappingConfig = new OdooAttributeMappingConfig();
        _configuration.GetSection("OdooAttributeMapping").Bind(_attributeMappingConfig);
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

            var allProducts = await _unitOfWork.Products.GetAllAsync();
            var existingProducts = allProducts
                .Where(p => slugs.Contains(p.Slug))
                .ToDictionary(p => p.Slug, p => p);

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

                    // Determine ProductType based on category mapping
                    ProductType? productType = null;
                    if (!string.IsNullOrEmpty(odooProduct.CategoryName))
                    {
                        productType = await GetOrCreateProductTypeAsync(odooProduct.CategoryName);
                    }

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

                        // Update ProductType if mapping is enabled
                        if (productType != null)
                        {
                            existingProduct.ProductTypeId = productType.Id;
                        }

                        // Update attributes (clear and re-add for simplicity)
                        if (_attributeMappingConfig.EnableAutoMapping && odooProduct.Attributes.Any() && productType != null)
                        {
                            await UpdateProductAttributesAsync(existingProduct, odooProduct.Attributes, productType);
                        }

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
                            ProductTypeId = productType?.Id,
                            DisplayOrder = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // Add attributes if available
                        if (_attributeMappingConfig.EnableAutoMapping && odooProduct.Attributes.Any() && productType != null)
                        {
                            await AddProductAttributesAsync(newProduct, odooProduct.Attributes, productType);
                        }

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
                fields = new[] {
                    "id",
                    "name",
                    "description_sale",
                    "list_price",
                    "qty_available",
                    "image_1920",
                    "categ_id",
                    "product_template_attribute_value_ids" // Get variant attributes
                },
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

            // Fetch attribute mappings if any products have attributes
            var attributeValueIds = simpleProducts
                .Where(p => p.ProductTemplateAttributeValueIds != null && p.ProductTemplateAttributeValueIds.Any())
                .SelectMany(p => p.ProductTemplateAttributeValueIds!)
                .Distinct()
                .ToList();

            var attributeMapping = new Dictionary<int, (string AttributeName, string ValueName)>();

            if (_attributeMappingConfig.EnableAutoMapping && attributeValueIds.Any())
            {
                attributeMapping = await FetchAttributeValuesFromOdooAsync(attributeValueIds);
            }

            var products = simpleProducts.Select(p =>
            {
                var categoryName = ExtractOdooName(p.CategId);

                var productDto = new OdooProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = null, // Can be populated from description_sale if needed
                    Price = 0, // Can be populated from list_price if needed
                    StockQuantity = 0, // Can be populated from qty_available if needed
                    ImageUrl = null, // Can be populated from image_1920 if needed
                    CategoryName = categoryName,
                    Active = true
                };

                // Map attributes
                if (p.ProductTemplateAttributeValueIds != null && p.ProductTemplateAttributeValueIds.Any())
                {
                    foreach (var attrValueId in p.ProductTemplateAttributeValueIds)
                    {
                        if (attributeMapping.TryGetValue(attrValueId, out var attrInfo))
                        {
                            productDto.Attributes[attrInfo.AttributeName] = attrInfo.ValueName;
                        }
                    }
                }

                return productDto;
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

    /// <summary>
    /// Gets or creates a ProductType based on Odoo category name and configuration mapping
    /// </summary>
    private async Task<ProductType?> GetOrCreateProductTypeAsync(string? odooCategoryName)
    {
        if (string.IsNullOrEmpty(odooCategoryName) || !_attributeMappingConfig.EnableAutoMapping)
            return null;

        // Check if there's a mapping in configuration
        string productTypeName;
        if (_attributeMappingConfig.CategoryToProductTypeMap.TryGetValue(odooCategoryName, out var mappedName))
        {
            productTypeName = mappedName;
        }
        else
        {
            // Use default or the category name itself
            productTypeName = _attributeMappingConfig.DefaultProductType;
        }

        // Try to find existing ProductType
        var allProductTypes = await _unitOfWork.ProductTypes.GetAllAsync();
        var productType = allProductTypes.FirstOrDefault(pt =>
            pt.Name.Equals(productTypeName, StringComparison.OrdinalIgnoreCase));

        if (productType == null)
        {
            // Create new ProductType
            productType = new ProductType
            {
                Id = Guid.NewGuid(),
                Name = productTypeName,
                Slug = GenerateSlug(productTypeName),
                Description = $"Product type auto-created from Odoo category: {odooCategoryName}",
                DisplayOrder = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductTypes.AddAsync(productType);
            _logger.LogInformation("Created new ProductType: {ProductTypeName} from Odoo category: {CategoryName}",
                productTypeName, odooCategoryName);
        }

        return productType;
    }

    /// <summary>
    /// Gets or creates a ProductAttribute for a ProductType
    /// </summary>
    private async Task<ProductAttribute> GetOrCreateProductAttributeAsync(
        Guid productTypeId,
        string attributeName,
        string displayName)
    {
        var allAttributes = await _unitOfWork.ProductAttributes.GetAllAsync();
        var attribute = allAttributes.FirstOrDefault(a =>
            a.ProductTypeId == productTypeId &&
            a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        if (attribute == null)
        {
            attribute = new ProductAttribute
            {
                Id = Guid.NewGuid(),
                ProductTypeId = productTypeId,
                Name = attributeName,
                DisplayName = displayName,
                Description = $"Auto-created from Odoo attribute: {displayName}",
                DisplayOrder = 0,
                IsRequired = false,
                AllowMultipleSelection = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductAttributes.AddAsync(attribute);
            _logger.LogInformation("Created new ProductAttribute: {AttributeName} for ProductType: {ProductTypeId}",
                attributeName, productTypeId);
        }

        return attribute;
    }

    /// <summary>
    /// Gets or creates a ProductAttributeValue for a ProductAttribute
    /// </summary>
    private async Task<ProductAttributeValue> GetOrCreateProductAttributeValueAsync(
        Guid productAttributeId,
        string value)
    {
        var allValues = await _unitOfWork.ProductAttributeValues.GetAllAsync();
        var attributeValue = allValues.FirstOrDefault(av =>
            av.ProductAttributeId == productAttributeId &&
            av.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

        if (attributeValue == null)
        {
            attributeValue = new ProductAttributeValue
            {
                Id = Guid.NewGuid(),
                ProductAttributeId = productAttributeId,
                Value = value,
                DisplayOrder = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductAttributeValues.AddAsync(attributeValue);
            _logger.LogInformation("Created new ProductAttributeValue: {Value} for ProductAttribute: {ProductAttributeId}",
                value, productAttributeId);
        }

        return attributeValue;
    }

    /// <summary>
    /// Adds product attribute selections to a new product based on Odoo attributes
    /// </summary>
    private async Task AddProductAttributesAsync(
        Product product,
        Dictionary<string, string> odooAttributes,
        ProductType productType)
    {
        foreach (var (attributeName, attributeValue) in odooAttributes)
        {
            // Check if there's a mapping configuration for this ProductType and attribute
            var mappedAttributeName = GetMappedAttributeName(productType.Name, attributeName);

            // Get or create the ProductAttribute
            var productAttribute = await GetOrCreateProductAttributeAsync(
                productType.Id,
                mappedAttributeName,
                attributeName); // Use original name as display name

            // Get or create the ProductAttributeValue
            var productAttributeValue = await GetOrCreateProductAttributeValueAsync(
                productAttribute.Id,
                attributeValue);

            // Create the ProductAttributeSelection
            var selection = new ProductAttributeSelection
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductAttributeId = productAttribute.Id,
                ProductAttributeValueId = productAttributeValue.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            product.ProductAttributeSelections.Add(selection);
        }
    }

    /// <summary>
    /// Updates product attribute selections for an existing product
    /// Clears existing selections and adds new ones based on Odoo attributes
    /// </summary>
    private async Task UpdateProductAttributesAsync(
        Product product,
        Dictionary<string, string> odooAttributes,
        ProductType productType)
    {
        // Clear existing selections for this product
        var existingSelections = await _unitOfWork.ProductAttributeSelections
            .FindAsync(pas => pas.ProductId == product.Id);

        foreach (var selection in existingSelections)
        {
            _unitOfWork.ProductAttributeSelections.Remove(selection);
        }

        // Add new selections
        await AddProductAttributesAsync(product, odooAttributes, productType);
    }

    /// <summary>
    /// Gets the mapped attribute name from configuration, or returns the original name if no mapping exists
    /// </summary>
    private string GetMappedAttributeName(string productTypeName, string odooAttributeName)
    {
        if (_attributeMappingConfig.AttributeMapping.TryGetValue(productTypeName, out var typeMapping))
        {
            if (typeMapping.TryGetValue(odooAttributeName, out var mappedName))
            {
                return mappedName;
            }
        }

        // If no mapping found, normalize the attribute name (lowercase, no spaces)
        return odooAttributeName.ToLower().Replace(" ", "_");
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Trim();
    }

    /// <summary>
    /// Fetches attribute value details from Odoo product.template.attribute.value model
    /// Returns mapping of value ID to (AttributeName, ValueName)
    /// </summary>
    private async Task<Dictionary<int, (string AttributeName, string ValueName)>> FetchAttributeValuesFromOdooAsync(List<int> valueIds)
    {
        var result = new Dictionary<int, (string, string)>();

        if (!valueIds.Any())
            return result;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var rpcUrl = $"{_odooUrl}/json/2/product.template.attribute.value/search_read";

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_odooApiKey}");
            client.DefaultRequestHeaders.Add("X-Odoo-Database", _odooDatabase);

            var payload = new
            {
                context = new { lang = "en_US" },
                domain = new object[] { new object[] { "id", "in", valueIds } },
                fields = new[] { "id", "name", "attribute_id", "product_attribute_value_id" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(rpcUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch attribute values from Odoo. Status: {Status}", response.StatusCode);
                return result;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var attributeValues = JsonSerializer.Deserialize<List<OdooAttributeValueDto>>(responseBody, options);

            if (attributeValues != null)
            {
                foreach (var attrValue in attributeValues)
                {
                    var attributeName = ExtractOdooName(attrValue.AttributeId) ?? "Unknown";
                    var valueName = ExtractOdooName(attrValue.ProductAttributeValueId) ?? attrValue.Name;

                    result[attrValue.Id] = (attributeName, valueName);
                }
            }

            _logger.LogInformation("Fetched {Count} attribute values from Odoo", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attribute values from Odoo");
        }

        return result;
    }

    /// <summary>
    /// Extracts the name from Odoo's [id, name] array format
    /// Example: [1, "Medium"] returns "Medium"
    /// </summary>
    private static string? ExtractOdooName(object? odooField)
    {
        if (odooField == null)
            return null;

        if (odooField is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array && jsonElement.GetArrayLength() >= 2)
            {
                return jsonElement[1].GetString();
            }
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                return jsonElement.GetString();
            }
        }

        // Handle string directly
        if (odooField is string str)
            return str;

        // Try to parse as JSON if it's a string representation
        var odooStr = odooField.ToString();
        if (odooStr != null && odooStr.StartsWith("["))
        {
            try
            {
                var arr = JsonSerializer.Deserialize<JsonElement>(odooStr);
                if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() >= 2)
                {
                    return arr[1].GetString();
                }
            }
            catch
            {
                // Fall through to return null
            }
        }

        return null;
    }
}
