using GOKCafe.Web.Models.DTOs;

namespace GOKCafe.Web.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
    }
}
