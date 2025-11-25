using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface IProductService
    {
        IEnumerable<ProductDto> GetFeaturedProducts(int count = 8);
        IEnumerable<ProductDto> GetAllProducts();
        IEnumerable<ProductDto> GetProductsByCategory(Guid categoryId);
        ProductDto? GetProductById(Guid id);
        ProductDto? GetProductBySlug(string slug);
        PaginatedResponse<ProductDto> GetProducts(int pageNumber = 1, int pageSize = 12, string? categoryId = null, string? searchTerm = null);
    }
}
