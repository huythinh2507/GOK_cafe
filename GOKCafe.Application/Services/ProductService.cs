using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;

namespace GOKCafe.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(
        int pageNumber, int pageSize, Guid? categoryId = null, bool? isFeatured = null)
    {
        try
        {
            var query = (await _unitOfWork.Products.GetAllAsync()).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (isFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == isFeatured.Value);

            query = query.Where(p => p.IsActive);

            var totalCount = query.Count();
            var items = query
                .OrderBy(p => p.DisplayOrder)
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
                }).ToList();

            var response = new PaginatedResponse<ProductDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PaginatedResponse<ProductDto>>.SuccessResult(response);
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
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<ProductDto>.FailureResult("Product not found");

            var productDto = MapToDto(product);
            return ApiResponse<ProductDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.FailureResult(
                "An error occurred while retrieving the product",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ProductDto>> GetProductBySlugAsync(string slug)
    {
        try
        {
            var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Slug == slug);
            if (product == null)
                return ApiResponse<ProductDto>.FailureResult("Product not found");

            var productDto = MapToDto(product);
            return ApiResponse<ProductDto>.SuccessResult(productDto);
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
