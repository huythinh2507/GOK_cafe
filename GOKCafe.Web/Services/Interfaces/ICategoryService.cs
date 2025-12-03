using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDto> GetAllCategories();
        CategoryDto? GetCategoryById(Guid id);
        CategoryDto? GetCategoryBySlug(string slug);
    }
}
