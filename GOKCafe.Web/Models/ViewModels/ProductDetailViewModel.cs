using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public ProductDto Product { get; set; } = new();
        public List<ProductDto> RelatedProducts { get; set; } = new();
        public CategoryDto? Category { get; set; }
    }
}
