using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Equipment;
using GOKCafe.Application.DTOs.FlavourProfile;
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
        int pageNumber,
        int pageSize,
        List<Guid>? categoryIds = null,
        bool? isFeatured = null,
        string? search = null,
        List<Guid>? flavourProfileIds = null,
        List<Guid>? equipmentIds = null,
        bool? inStock = null)
    {
        try
        {
            // Generate cache key based on parameters
            var categoryIdsKey = categoryIds != null && categoryIds.Any()
                ? string.Join("-", categoryIds.OrderBy(x => x))
                : "all";
            var featuredKey = isFeatured.HasValue ? isFeatured.Value.ToString() : "all";
            var searchKey = !string.IsNullOrWhiteSpace(search) ? search.ToLower() : "none";
            var flavourProfileIdsKey = flavourProfileIds != null && flavourProfileIds.Any()
                ? string.Join("-", flavourProfileIds.OrderBy(x => x))
                : "all";
            var equipmentIdsKey = equipmentIds != null && equipmentIds.Any()
                ? string.Join("-", equipmentIds.OrderBy(x => x))
                : "all";
            var inStockKey = inStock.HasValue ? inStock.Value.ToString() : "all";
            var cacheKey = $"{ProductListCacheKeyPrefix}page:{pageNumber}:size:{pageSize}:cats:{categoryIdsKey}:feat:{featuredKey}:search:{searchKey}:flavours:{flavourProfileIdsKey}:equip:{equipmentIdsKey}:stock:{inStockKey}";

            // Try to get from cache
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<PaginatedResponse<ProductDto>>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            // If not in cache, fetch from database
            var query = _unitOfWork.Products.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFlavourProfiles)
                    .ThenInclude(pfp => pfp.FlavourProfile)
                .Include(p => p.ProductEquipments)
                    .ThenInclude(pe => pe.Equipment)
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

            // Filter by flavour profiles
            if (flavourProfileIds != null && flavourProfileIds.Any())
            {
                query = query.Where(p => p.ProductFlavourProfiles
                    .Any(pfp => flavourProfileIds.Contains(pfp.FlavourProfileId)));
            }

            // Filter by equipment
            if (equipmentIds != null && equipmentIds.Any())
            {
                query = query.Where(p => p.ProductEquipments
                    .Any(pe => equipmentIds.Contains(pe.EquipmentId)));
            }

            // Filter by stock availability
            if (inStock.HasValue)
            {
                query = inStock.Value
                    ? query.Where(p => p.StockQuantity > 0)
                    : query.Where(p => p.StockQuantity <= 0);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs with JSON deserialization done in-memory
            var productDtos = items.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ShortDescription = p.ShortDescription,
                    TastingNote = p.TastingNote,
                    Region = p.Region,
                    Process = p.Process,
                    Slug = p.Slug,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AvailableSizes = !string.IsNullOrEmpty(p.AvailableSizes)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.AvailableSizes)
                        : null,
                    AvailableGrinds = !string.IsNullOrEmpty(p.AvailableGrinds)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.AvailableGrinds)
                        : null,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl,
                        AltText = pi.AltText,
                        DisplayOrder = pi.DisplayOrder,
                        IsPrimary = pi.IsPrimary
                    }).ToList(),
                    FlavourProfiles = p.ProductFlavourProfiles.Select(pfp => new FlavourProfileDto
                    {
                        Id = pfp.FlavourProfile.Id,
                        Name = pfp.FlavourProfile.Name,
                        Description = pfp.FlavourProfile.Description,
                        DisplayOrder = pfp.FlavourProfile.DisplayOrder,
                        IsActive = pfp.FlavourProfile.IsActive
                    }).ToList(),
                    Equipments = p.ProductEquipments.Select(pe => new EquipmentDto
                    {
                        Id = pe.Equipment.Id,
                        Name = pe.Equipment.Name,
                        Description = pe.Equipment.Description,
                        DisplayOrder = pe.Equipment.DisplayOrder,
                        IsActive = pe.Equipment.IsActive
                    }).ToList()
                }).ToList();

            var response = new PaginatedResponse<ProductDto>
            {
                Items = productDtos,
                TotalItems = totalCount,
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
                .Include(p => p.ProductFlavourProfiles)
                    .ThenInclude(pfp => pfp.FlavourProfile)
                .Include(p => p.ProductEquipments)
                    .ThenInclude(pe => pe.Equipment)
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

            // Add flavour profile relationships
            if (dto.FlavourProfileIds != null && dto.FlavourProfileIds.Any())
            {
                foreach (var flavourProfileId in dto.FlavourProfileIds)
                {
                    product.ProductFlavourProfiles.Add(new ProductFlavourProfile
                    {
                        ProductId = product.Id,
                        FlavourProfileId = flavourProfileId
                    });
                }
            }

            // Add equipment relationships
            if (dto.EquipmentIds != null && dto.EquipmentIds.Any())
            {
                foreach (var equipmentId in dto.EquipmentIds)
                {
                    product.ProductEquipments.Add(new ProductEquipment
                    {
                        ProductId = product.Id,
                        EquipmentId = equipmentId
                    });
                }
            }

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate product list cache
            await _cacheService.RemoveByPrefixAsync(ProductListCacheKeyPrefix);

            // Load the product with relationships for the response
            var createdProduct = await _unitOfWork.Products.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFlavourProfiles)
                    .ThenInclude(pfp => pfp.FlavourProfile)
                .Include(p => p.ProductEquipments)
                    .ThenInclude(pe => pe.Equipment)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            var productDto = MapToDto(createdProduct!);
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
            var product = await _unitOfWork.Products.GetQueryable()
                .Include(p => p.ProductFlavourProfiles)
                .Include(p => p.ProductEquipments)
                .FirstOrDefaultAsync(p => p.Id == id);

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

            // Update flavour profile relationships
            product.ProductFlavourProfiles.Clear();
            if (dto.FlavourProfileIds != null && dto.FlavourProfileIds.Any())
            {
                foreach (var flavourProfileId in dto.FlavourProfileIds)
                {
                    product.ProductFlavourProfiles.Add(new ProductFlavourProfile
                    {
                        ProductId = product.Id,
                        FlavourProfileId = flavourProfileId
                    });
                }
            }

            // Update equipment relationships
            product.ProductEquipments.Clear();
            if (dto.EquipmentIds != null && dto.EquipmentIds.Any())
            {
                foreach (var equipmentId in dto.EquipmentIds)
                {
                    product.ProductEquipments.Add(new ProductEquipment
                    {
                        ProductId = product.Id,
                        EquipmentId = equipmentId
                    });
                }
            }

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate caches
            await _cacheService.RemoveAsync($"{ProductCacheKeyPrefix}{id}");
            await _cacheService.RemoveByPrefixAsync(ProductListCacheKeyPrefix);

            // Load the product with all relationships for the response
            var updatedProduct = await _unitOfWork.Products.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFlavourProfiles)
                    .ThenInclude(pfp => pfp.FlavourProfile)
                .Include(p => p.ProductEquipments)
                    .ThenInclude(pe => pe.Equipment)
                .FirstOrDefaultAsync(p => p.Id == id);

            var productDto = MapToDto(updatedProduct!);
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

    public async Task<ApiResponse<ProductFiltersDto>> GetProductFiltersAsync()
    {
        try
        {
            const string cacheKey = "product:filters";

            // Try to get from cache
            var cachedResponse = await _cacheService.GetAsync<ApiResponse<ProductFiltersDto>>(cacheKey);
            if (cachedResponse != null)
                return cachedResponse;

            // Get all active flavour profiles
            var flavourProfiles = await _unitOfWork.FlavourProfiles.GetQueryable()
                .Where(fp => fp.IsActive)
                .OrderBy(fp => fp.DisplayOrder)
                .Select(fp => new FlavourProfileDto
                {
                    Id = fp.Id,
                    Name = fp.Name,
                    Description = fp.Description,
                    DisplayOrder = fp.DisplayOrder,
                    IsActive = fp.IsActive
                })
                .ToListAsync();

            // Get all active equipment
            var equipments = await _unitOfWork.Equipments.GetQueryable()
                .Where(e => e.IsActive)
                .OrderBy(e => e.DisplayOrder)
                .Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    DisplayOrder = e.DisplayOrder,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            // Get stock availability counts
            var products = _unitOfWork.Products.GetQueryable().Where(p => p.IsActive);
            var inStockCount = await products.CountAsync(p => p.StockQuantity > 0);
            var outOfStockCount = await products.CountAsync(p => p.StockQuantity <= 0);

            var filtersDto = new ProductFiltersDto
            {
                FlavourProfiles = flavourProfiles,
                Equipments = equipments,
                Availability = new AvailabilityDto
                {
                    InStockCount = inStockCount,
                    OutOfStockCount = outOfStockCount
                }
            };

            var result = ApiResponse<ProductFiltersDto>.SuccessResult(filtersDto);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, CacheExpiration);

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductFiltersDto>.FailureResult(
                "An error occurred while retrieving product filters",
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
            ShortDescription = product.ShortDescription,
            TastingNote = product.TastingNote,
            Region = product.Region,
            Process = product.Process,
            Slug = product.Slug,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrl = product.ImageUrl,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            AvailableSizes = !string.IsNullOrEmpty(product.AvailableSizes)
                ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(product.AvailableSizes)
                : null,
            AvailableGrinds = !string.IsNullOrEmpty(product.AvailableGrinds)
                ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(product.AvailableGrinds)
                : null,
            Images = product.ProductImages?.Select(pi => new ProductImageDto
            {
                Id = pi.Id,
                ImageUrl = pi.ImageUrl,
                AltText = pi.AltText,
                DisplayOrder = pi.DisplayOrder,
                IsPrimary = pi.IsPrimary
            }).ToList() ?? new List<ProductImageDto>(),
            FlavourProfiles = product.ProductFlavourProfiles?.Select(pfp => new FlavourProfileDto
            {
                Id = pfp.FlavourProfile.Id,
                Name = pfp.FlavourProfile.Name,
                Description = pfp.FlavourProfile.Description,
                DisplayOrder = pfp.FlavourProfile.DisplayOrder,
                IsActive = pfp.FlavourProfile.IsActive
            }).ToList() ?? new List<FlavourProfileDto>(),
            Equipments = product.ProductEquipments?.Select(pe => new EquipmentDto
            {
                Id = pe.Equipment.Id,
                Name = pe.Equipment.Name,
                Description = pe.Equipment.Description,
                DisplayOrder = pe.Equipment.DisplayOrder,
                IsActive = pe.Equipment.IsActive
            }).ToList() ?? new List<EquipmentDto>()
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
