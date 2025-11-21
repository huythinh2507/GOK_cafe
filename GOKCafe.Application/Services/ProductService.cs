using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string ProductCacheKeyPrefix = "product:";
    private const string ProductListCacheKeyPrefix = "products:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public ProductService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(
        int pageNumber, int pageSize, List<Guid>? categoryIds = null, bool? isFeatured = null, string? search = null)
    {
        try
        {
            // Generate cache key based on parameters
            var categoryIdsKey = categoryIds != null && categoryIds.Any()
                ? string.Join("-", categoryIds.OrderBy(x => x))
                : "all";
            var featuredKey = isFeatured.HasValue ? isFeatured.Value.ToString() : "all";
            var searchKey = !string.IsNullOrWhiteSpace(search) ? search.ToLower() : "none";
            var cacheKey = $"{ProductListCacheKeyPrefix}page:{pageNumber}:size:{pageSize}:cats:{categoryIdsKey}:feat:{featuredKey}:search:{searchKey}";

            // Try to get from cache
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<ProductDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            // If not in cache, fetch from database
            var query = _unitOfWork.Products.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive);

            // Filter by multiple categories
            if (categoryIds != null && categoryIds.Any())
                query = query.Where(p => categoryIds.Contains(p.CategoryId));

            // Filter by featured
            if (isFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == isFeatured.Value);

            // Search by name or description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Slug = p.Slug,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText,
                        DisplayOrder = pi.DisplayOrder,
                        IsPrimary = pi.IsPrimary
                    }).ToList()
                }).ToListAsync();

            var response = new PaginatedResponse<ProductDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = ApiResponse<PaginatedResponse<ProductDto>>.SuccessResult(response);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, CacheExpiration);

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductDto>>.FailureResult(
                "An error occurred while retrieving products",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{ProductCacheKeyPrefix}{id}";

            // Try to get from cache
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ProductDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            // If not in cache, fetch from database
            var product = await _unitOfWork.Products.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return ApiResponse<ProductDto>.FailureResult("Product not found");

            var productDto = MapToDto(product);
            var result = ApiResponse<ProductDto>.SuccessResult(productDto);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, CacheExpiration);

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.FailureResult(
                "An error occurred while retrieving the product",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Slug == slug);
            if (existingProduct != null)
                return ApiResponse<ProductDto>.FailureResult("A product with this name already exists");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                ImageUrl = dto.ImageUrl,
                StockQuantity = dto.StockQuantity,
                IsFeatured = dto.IsFeatured,
                CategoryId = dto.CategoryId,
                IsActive = true
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate product list cache
            await _cacheService.RemoveByPrefixAsync(ProductListCacheKeyPrefix);

            var productDto = MapToDto(product);
            return ApiResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.FailureResult(
                "An error occurred while creating the product",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<ProductDto>.FailureResult("Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Slug = GenerateSlug(dto.Name);
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.ImageUrl = dto.ImageUrl;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.IsFeatured = dto.IsFeatured;
            product.CategoryId = dto.CategoryId;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate caches
            await _cacheService.RemoveAsync($"{ProductCacheKeyPrefix}{id}");
            await _cacheService.RemoveByPrefixAsync(ProductListCacheKeyPrefix);

            var productDto = MapToDto(product);
            return ApiResponse<ProductDto>.SuccessResult(productDto, "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.FailureResult(
                "An error occurred while updating the product",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<bool>.FailureResult("Product not found");

            _unitOfWork.Products.SoftDelete(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate caches
            await _cacheService.RemoveAsync($"{ProductCacheKeyPrefix}{id}");
            await _cacheService.RemoveByPrefixAsync(ProductListCacheKeyPrefix);

            return ApiResponse<bool>.SuccessResult(true, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deleting the product",
                new List<string> { ex.Message });
        }
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrl = product.ImageUrl,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Images = product.ProductImages?.Select(pi => new ProductImageDto
            {
                Id = pi.Id,
                ImageUrl = pi.ImageUrl,
                AltText = pi.AltText,
                DisplayOrder = pi.DisplayOrder,
                IsPrimary = pi.IsPrimary
            }).ToList() ?? new List<ProductImageDto>()
        };
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Trim();
    }
}
