using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Models.ViewModels
{
    public class ProductListViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public PaginatedResponse<ProductDto> Products { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public Guid? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
