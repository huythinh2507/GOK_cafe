using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public ProductDto Product { get; set; } = new();
        public List<ProductDto> RelatedProducts { get; set; } = new();
        public CategoryDto? Category { get; set; }
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    }

    public class BreadcrumbItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
