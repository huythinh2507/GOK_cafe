using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Models.ViewModels
{
    public class CategoryViewModel
    {
        public CategoryDto Category { get; set; } = new();
        public PaginatedResponse<ProductDto> Products { get; set; } = new();
    }
}
